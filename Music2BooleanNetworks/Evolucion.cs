using System.Reflection;

/// <summary>
/// Evoluciona una red booleana basada en un archivo de música.
/// </summary>
/// <param name="archivo">Ruta del archivo de música a procesar.</param>
/// <param name="conectividad">Nivel de conectividad de la red.</param>
/// <param name="nodos">Número de nodos en la red.</param>
/// <param name="generaciones">Número de generaciones para la evolución.</param>
/// <remarks>
/// Este método realiza los siguientes pasos:
/// 1. Genera un nombre de archivo basado en los parámetros de entrada.
/// 2. Crea una instancia de la clase Gk para manejar la escritura de datos.
/// 3. Construye una red booleana con los parámetros especificados.
/// 4. Asigna la tarea de interpretar el archivo de música a la red.
/// 5. Mide el error en la interpretación inicial.
/// 6. Entrena la red en lotes durante el número de generaciones especificado.
/// 7. Si el error de la red no mejora después de 10000 iteraciones, exporta y evalúa la red.
/// 8. Al finalizar, exporta la red final, evalúa su desempeño y crea un sonido basado en la red.
/// </remarks>
namespace Music2BooleanNetworks {
    public static class Evolucion {
        public static void EvoluciónDeUnaRed(string archivo, float conectividad, int nodos, int generaciones) {
            var datos = archivo.Substring(0, archivo.Length - 4) + $"_N{nodos}_G{generaciones}_k{conectividad}";
            var FileName = MethodBase.GetCurrentMethod().Name + "_" + datos + ".txt";
            var gk = new Gk(FileName);
            Extra.ImpLinea(datos);
            gk.StopWriting();

            var net = new Red();
            net.construir((int)conectividad, (int)conectividad, nodos, 0.2f);
            net.conectividadForzada = conectividad;
            net.asiganarTarea(archivo);
            net.medirErrorEnLaInterpretacion();

            int noImprovementCount = 0;
            float lastError = net.error;

            // Training parameters
            int tamañoLote = 5;
            int iteracionesPorLote = 100;

            for (int i = 0; i < generaciones; i++) {
                net.entrenarPorLotes(tamañoLote, iteracionesPorLote);

                if (net.error < lastError) {
                    lastError = net.error;
                    noImprovementCount = 0;
                } else {
                    noImprovementCount++;
                }

                if (net.error == 0 || noImprovementCount >= 10000) {
                    gk.StartWriting(FileName, true);
                    net.exportar();
                    net.evaluarDesempeño();
                    net.crearSonido();
                    net.incrementarTareaLocal(1);
                    net.medirErrorEnLaInterpretacion();
                    for (int rgln = 0; rgln < 5; rgln++) System.Console.WriteLine();
                    gk.StopWriting();
                    break;
                }
            }

            gk.StartWriting(FileName, true);
            Extra.ImpLinea(" ");
            Extra.ImpLinea("Resultado de la evolución - Red final");
            Extra.ImpLinea(" ");
            net.exportar();
            net.evaluarDesempeño();
            net.crearSonido();
            Extra.ImpLinea("Melodía original");
            Red.convertirCadenasASonido(net.tareaGlobal, net.notas);

            gk.StopAndOpen(FileName);
        }
    }
}
