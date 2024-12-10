using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Music2BooleanNetworks {
    public class Red : Extra {

        // ThreadLocal Random para esta clase
        private static ThreadLocal<Random> rnd = new ThreadLocal<Random>(() => new Random(GetSeed()));

        // Método para obtener semillas únicas
        private static int seedCounter = Environment.TickCount;
        private static readonly object seedLock = new object();

        private static int GetSeed()
        {
            lock (seedLock)
            {
                return seedCounter++;
            }
        }

        public List<Nodo> nodos = new List<Nodo>();
        public float tasaDeMutación = 1;
        public List<string> tareaGlobal = new List<string>();
        public List<string> tareaLocal = new List<string>();
        public List<int> notas = new List<int>();
        public List<int> nodosMusicales = new List<int>();
        public int error;
        public float conectividadForzada = 2.0f;
        bool[] estadoActual;
        private float tasaMutaciónInicial = 1;
        private float tasaMutaciónMinima = 0.01f;

        // Construye la red con los parámetros dados
        public void construir(int kMínima, int kMáxima, int númeroDeNodos, float tasaDeMutación_)
        {
            tasaDeMutación = tasaDeMutación_;
            for (int i = 0; i < númeroDeNodos; i++)
            {
                var n = new Nodo();
                var conectividad = rnd.Value.Next(kMínima, kMáxima + 1);
                n.asignarEntradas(conectividad, númeroDeNodos);
                n.crearFunciónBooleana();
                nodos.Add(n);
            }
            encontrarSalidas();
        }

        void encontrarSalidas() {
            for (int i = 0; i < nodos.Count; i++) {
                nodos[i].salidas = nodos.Where(x => x.entradas.Contains(i)).Select(x => nodos.IndexOf(x)).ToList();
            }
        }

        void inicializarEstado() {
            estadoActual = new bool[nodos.Count];
            var pasoInicial = tareaGlobal[0];
            for (int i = 0; i < nodos.Count; i++) {
                if (nodosMusicales.Contains(i)) {
                    estadoActual[i] = pasoInicial[nodosMusicales.IndexOf(i)] == '1';
                } else {
                    estadoActual[i] = rnd.Value.Next(2) == 1;
                }
            }
        }

        // Actualiza el estado de la red
        void actualizarEstado() {
            bool[] nuevoEstado = new bool[nodos.Count];
            Parallel.For(0, nodos.Count, i => {
                var n = nodos[i];
                int índice = 0;
                for (int j = 0; j < n.conectividad; j++) {
                    if (estadoActual[n.entradas[j]]) {
                        índice |= 1 << (n.conectividad - j - 1);
                    }
                }
                nuevoEstado[i] = n.funciónBooleana[índice] == 1;
            });
            estadoActual = nuevoEstado;
        }

        // Asigna la tarea a la red desde un archivo
        public void asiganarTarea(string archivo) {
            var contenido = Gk.LeerArchivo(archivo);
            var matriz = new List<List<string>>();
            foreach (var s in contenido) {
                var renglón = s.Split(',');
                matriz.Add(renglón.ToList());
            }
            notas = extraerNotas(matriz);
            tareaGlobal = extraerTarea(matriz, notas);
            tareaLocal.Add(tareaGlobal[0]);
            tareaLocal.Add(tareaGlobal[1]);
            SeleccionarNodosMusicales();
            inicializarEstado();
        }

        // Extrae las notas de la matriz
        static List<int> extraerNotas(List<List<string>> matriz) {
            var listaDeNotas = new List<int>();
            var primerRenglón = matriz.First();

            for (int columna = 0; columna < primerRenglón.Count; columna++) {
                var s0 = primerRenglón[columna];
                int renglón;
                for (renglón = 1; renglón < matriz.Count; renglón++) {
                    if (matriz[renglón][columna] != s0) {
                        break;
                    }
                }
                if (renglón != matriz.Count) {
                    listaDeNotas.Add(columna);
                }
            }
            return listaDeNotas;
        }

        // Extrae la tarea de la matriz
        static List<string> extraerTarea(List<List<string>> matriz, List<int> listaNotas) {
            var listaTarea = new List<string>();

            for (int renglón = 0; renglón < matriz.Count; renglón++) {
                var nuevoRenglón = "";
                foreach (var columna in listaNotas) {
                    nuevoRenglón += matriz[renglón][columna];
                }
                listaTarea.Add(nuevoRenglón);
            }
            return listaTarea;
        }

        // Incrementa la tarea local en un número de pasos extra
        public void incrementarTareaLocal(int NúmeroDePasosExtra) {
            for (int i = 0; i < NúmeroDePasosExtra && tareaLocal.Count < tareaGlobal.Count; i++) {
                tareaLocal.Add(tareaGlobal[tareaLocal.Count]);
            }
        }

        // Selecciona los nodos musicales
        void SeleccionarNodosMusicales() {
            if (nodos.Count < notas.Count) {
                throw new ArgumentException("Número de nodos insuficiente para la tarea.");
            }
            nodosMusicales = Enumerable.Range(0, notas.Count).ToList();
        }

        // Mide el error en la interpretación de la red
        public int medirErrorEnLaInterpretacion() {
            error = 0;
            inicializarEstado();

            for (int i = 1; i < tareaLocal.Count; i++) {
                actualizarEstado();
                var interpretación = resultadoMusical();
                var tarea = tareaGlobal[i];
                error += distanciaHamming(interpretación, tarea);
            }
            return error;
        }

        // Obtiene el resultado musical de la red
        string resultadoMusical() {
            char[] estadoCortado = new char[nodosMusicales.Count];
            for (int i = 0; i < nodosMusicales.Count; i++) {
                estadoCortado[i] = estadoActual[nodosMusicales[i]] ? '1' : '0';
            }
            return new string(estadoCortado);
        }

        // Calcula la distancia de Hamming entre dos cadenas
        int distanciaHamming(string s1, string s2) {
            if (s1.Length != s2.Length) {
                throw new ArgumentException("Cadenas de diferente longitud.");
            }
            int diferencias = 0;
            for (int i = 0; i < s1.Length; i++) {
                if (s1[i] != s2[i]) {
                    diferencias++;
                }
            }
            return diferencias;
        }

        // Calcula la conectividad promedio de la red
        float K() => (float)nodos.Sum(n => n.entradas.Count) / nodos.Count;

        float probabilidadDeAñadirEntradas = 0.3f;

        // Entrena la red por lotes
        public void entrenarPorLotes(int tamañoLote, int iteracionesPorLote) {
            for (int i = 0; i < tamañoLote; i++) {
                var redClonada = this.clonar();
                
                for (int j = 0; j < iteracionesPorLote; j++) {
                    var redMutada = redClonada.clonar();
                    redMutada.mutarConEnfriamiento(500f, 0.99f, iteracionesPorLote);
                    redMutada.medirErrorEnLaInterpretacion();
                    
                    if (redMutada.error <= redClonada.error) {
                        redClonada = redMutada;
                    }
                }
                
                if (redClonada.error < this.error) {
                    this.copiarDesde(redClonada);
                    this.error = redClonada.error;
                }
            }
        }

        // Copia los datos de otra red
        private void copiarDesde(Red otra) {
            this.nodos = otra.nodos.Select(n => n.clonar()).ToList();
            this.error = otra.error;
            this.conectividadForzada = otra.conectividadForzada;
            this.tasaDeMutación = otra.tasaDeMutación;
            this.notas = new List<int>(otra.notas);
            this.nodosMusicales = new List<int>(otra.nodosMusicales);
            this.tareaGlobal = new List<string>(otra.tareaGlobal);
            this.tareaLocal = new List<string>(otra.tareaLocal);
        }

        // Muta la red
        public void mutar()
        {
            probabilidadDeAñadirEntradas += (conectividadForzada - K()) / 100f;
            var indices = listaDesordenada(nodos.Count);
            foreach (int n_rnd in indices)
            {
                var n = nodos[n_rnd];
                if (rnd.Value.NextDouble() < tasaDeMutación)
                {
                    int mutación = rnd.Value.Next(3);
                    switch (mutación)
                    {
                        case 0:
                            var entrada = elementoAleatorio(n.entradas);
                            n.cambiarFunciónBooleana(entrada);
                            break;
                        case 1:
                            if (n.entradas.Count < 6 && rnd.Value.NextDouble() < probabilidadDeAñadirEntradas)
                            {
                                var nuevaEntrada = númeroAleatorioConExcepciones(0, nodos.Count, n.entradas);
                                n.añadirEntrada(nuevaEntrada);
                            }
                            break;
                        case 2:
                            if (n.entradas.Count > 1)
                            {
                                var entradaAEliminar = elementoAleatorio(n.entradas);
                                n.eliminarEntrada(entradaAEliminar);
                            }
                            break;
                    }
                }
            }
        }

        // Muta la red con enfriamiento simulado
        // Acá estan los dos hiperparametros que se pueden modificar (temperatura inicial y factor enfriamiento)
        // por lo que vi, el factor de enfriamiento es un valor entre 0 y 1, y la temperatura inicial es un valor entre 1 y 1000
        // En entrenar por lotes se llama a este metodo con temperatura inicial 500 y factor de enfriamiento 0.99
        public void mutarConEnfriamiento(float temperaturaInicial, float factorEnfriamiento, int iteraciones) {
            float temperaturaActual = temperaturaInicial;
            var mejorRed = this.clonar();
            for (int i = 0; i < iteraciones; i++) {
                var redMutada = this.clonar();
                redMutada.mutar();
                redMutada.medirErrorEnLaInterpretacion();
                int deltaError = redMutada.error - this.error;
                if (deltaError < 0 || Math.Exp(-deltaError / temperaturaActual) > rnd.Value.NextDouble()) {
                    this.copiarDesde(redMutada);
                    if (this.error < mejorRed.error) {
                        mejorRed = this.clonar();
                        this.error = redMutada.error;
                    }
                }
                temperaturaActual = Math.Max(temperaturaActual * factorEnfriamiento, 0.0001f);
            }
            this.copiarDesde(mejorRed);
        }

        // Ajusta la tasa de mutación en función de las generaciones sin mejora
        public void ajustarTasaMutacion(int generacionesSinMejora) {
            if (generacionesSinMejora > 10) {
                tasaDeMutación = Math.Max(tasaDeMutación * 0.9f, tasaMutaciónMinima);
            }
            if (generacionesSinMejora > 20) {
                tasaDeMutación = tasaMutaciónInicial;
            }
        }

        // Evalúa el desempeño de la red
        public void evaluarDesempeño() {
            ImpLinea("Desempeño de la red: ");
            Console.WriteLine($"El error es de {medirErrorEnLaInterpretacion()}");
            inicializarEstado();
            Console.WriteLine($"Tarea\tEjecución\tError");
            for (int i2 = 0; i2 < tareaLocal.Count; i2++) {
                string s1 = tareaLocal[i2];
                var s2 = resultadoMusical();
                Console.WriteLine($"{s1}\t{s2}\t{distanciaHamming(s1, s2)}");
                actualizarEstado();
            }
        }

        // Crea el sonido de la red
        public void crearSonido() {
            ImpLinea("Melodía en Mathematica: ");
            inicializarEstado();
            var interpretación = new List<string>();
            for (int i = 0; i < tareaGlobal.Count; i++) {
                interpretación.Add(resultadoMusical());
                actualizarEstado();
            }
            convertirCadenasASonido(interpretación, notas);
        }

        // Convierte las cadenas a sonido
        public static void convertirCadenasASonido(List<string> cadenas, List<int> notas) {
            var melodía = new List<string>();
            foreach (var s in cadenas) {
                var sonidos = new List<string>();
                for (int i = 0; i < s.Length; i++) {
                    if (s[i] == '1') {
                        sonidos.Add((notas[i] - 36).ToString());
                    }
                }
                melodía.Add("{" + string.Join(",", sonidos) + "}");
            }
            var melodíaFormateada = string.Join(",", melodía);
            Console.WriteLine($"ListaDeNotas = {{{melodíaFormateada}}};");
            Console.WriteLine($"Sound[{{Table[SoundNote[rk023, 0.5, \"Guitar\"], {{rk023, ListaDeNotas}}]}}]");
        }

        // Clona la red
        public Red clonar() {
            var net2 = new Red();
            foreach (var n in nodos) {
                net2.nodos.Add(n.clonar());
            }
            net2.conectividadForzada = conectividadForzada;
            net2.error = error;
            net2.nodosMusicales = new List<int>(nodosMusicales);
            net2.notas = new List<int>(notas);
            net2.probabilidadDeAñadirEntradas = probabilidadDeAñadirEntradas;
            net2.tareaGlobal = new List<string>(tareaGlobal);
            net2.tareaLocal = new List<string>(tareaLocal);
            net2.tasaDeMutación = tasaDeMutación;
            return net2;
        }

        // Exporta la red
        public void exportar() {
            Console.WriteLine("***InicioRed***");
            exportarInformacion();
            exportarBasico();
            Console.WriteLine("***FinRed***");
        }

        // Calcula la probabilidad booleana de la red
        float probabilidadBooleana() {
            return nodos.Sum(n => n.probabilidadBooleana()) / nodos.Count;
        }

        // Calcula la sensibilidad promedio de la red
        public float SProm() {
            var p = probabilidadBooleana();
            return 2 * p * (1 - p) * K();
        }

        // Exporta la información de la red
        public void exportarInformacion() {
            Console.WriteLine($"Nodos:               {nodos.Count}");
            Console.WriteLine($"Sensitividad:        {SProm()}");
            Console.WriteLine($"Conectividad:        {K()}");
            Console.WriteLine($"Probabilidad 1's FB: {probabilidadBooleana()}");
            Console.WriteLine($"Error:               {error}");
            Console.WriteLine($"Pasos implementados:  {tareaLocal.Count() - 1}");
        }

        // Exporta la información básica de la red
        public void exportarBasico() {
            Console.WriteLine("***InicioNodos***");
            for (int i = 0; i < nodos.Count; i++) {
                Console.Write($"{i}:");
                var n = nodos[i];
                foreach (var r in n.entradas) {
                    Console.Write($"{r} 1|");
                }
                Console.Write(":");
                foreach (var valor in n.funciónBooleana) Console.Write(valor);
                Console.WriteLine();
            }
            Console.WriteLine("***FinNodos***");
        }
    }
}
