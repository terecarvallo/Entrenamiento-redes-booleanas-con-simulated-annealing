using System;
using System.Collections.Generic;

/// <summary>
/// Representa un nodo en una red booleana.
/// </summary>
namespace Music2BooleanNetworks {
    public class Nodo {

        /// <summary>
        /// Generador de números aleatorios.
        /// </summary>
        // ThreadLocal Random (puede ser compartido si es estático)
        private static ThreadLocal<Random> rnd = new ThreadLocal<Random>(() => new Random(GetSeed()));

        /// <summary>
        /// Lista de entradas del nodo.
        /// </summary>
        public List<int> entradas = new List<int>();

        /// <summary>
        /// Lista de salidas del nodo.
        /// </summary>
        public List<int> salidas = new List<int>();

        /// <summary>
        /// Función booleana del nodo.
        /// </summary>
        public int[] funciónBooleana;

        /// <summary>
        /// Conectividad del nodo.
        /// </summary>
        public int conectividad;

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

        /// <summary>
        /// Constructor de la clase Nodo.
        /// </summary>
        public Nodo() {
            entradas = new List<int>();
            salidas = new List<int>();
            funciónBooleana = new int[0];
            conectividad = 0;
        }

        /// <summary>
        /// Asigna entradas al nodo.
        /// </summary>
        /// <param name="conectividad">Conectividad.</param>
        /// <param name="numeroDeNodos">Número de nodos.</param>
        public void asignarEntradas(int conectividad, int numeroDeNodos)
        {
            while (entradas.Count < conectividad)
            {
                var nuevaEntrada = rnd.Value.Next(numeroDeNodos);
                if (!entradas.Contains(nuevaEntrada))
                {
                    entradas.Add(nuevaEntrada);
                }
            }
            this.conectividad = conectividad;
        }

        /// <summary>
        /// Construye la función booleana del nodo.
        /// </summary>
        public void crearFunciónBooleana()
        {
            int numeroDeConfiguraciones = 1 << conectividad; // Equivalente a 2^conectividad
            funciónBooleana = new int[numeroDeConfiguraciones];
            for (int i = 0; i < numeroDeConfiguraciones; i++)
            {
                funciónBooleana[i] = rnd.Value.Next(2);
            }
        }

        /// <summary>
        /// Clona el nodo.
        /// </summary>
        /// <returns>Una versión clonada del nodo.</returns>
        public Nodo clonar() {
            var n = new Nodo();
            n.entradas = new List<int>(this.entradas);
            n.salidas = new List<int>(this.salidas);
            n.conectividad = this.conectividad;
            n.funciónBooleana = (int[])this.funciónBooleana.Clone();
            return n;
        }

        /// <summary>
        /// Añade una nueva entrada al nodo.
        /// </summary>
        /// <param name="entrada">Entrada.</param>
        public void añadirEntrada(int entrada) {
            if (!entradas.Contains(entrada)) {
                entradas.Add(entrada);
                conectividad = entradas.Count;
                // Actualizar la función booleana
                int numConfigsAntiguo = funciónBooleana.Length;
                int numConfigsNuevo = numConfigsAntiguo * 2;
                int[] nuevaFunciónBooleana = new int[numConfigsNuevo];

                for (int i = 0; i < numConfigsAntiguo; i++) {
                    // Mantener la salida anterior cuando la nueva entrada es '0'
                    nuevaFunciónBooleana[i * 2] = funciónBooleana[i];
                    // Asignar nueva salida aleatoria cuando la nueva entrada es '1'
                    nuevaFunciónBooleana[i * 2 + 1] = rnd.Value.Next(2);
                }
                funciónBooleana = nuevaFunciónBooleana;
            }
        }

        /// <summary>
        /// Cambia el tipo de regulación de la entrada.
        /// </summary>
        /// <param name="entrada">Entrada.</param>
        public void cambiarFunciónBooleana(int entrada) {
            int indiceEntrada = entradas.IndexOf(entrada);
            if (indiceEntrada == -1) {
                throw new ArgumentException("La entrada especificada no existe en el nodo.");
            }

            int numConfiguraciones = funciónBooleana.Length;
            int paso = 1 << (conectividad - indiceEntrada - 1);

            for (int i = 0; i < numConfiguraciones; i++) {
                if (((i / paso) % 2) == 1) { // Si el bit en indiceEntrada es '1'
                    funciónBooleana[i] = rnd.Value.Next(2);
                }
            }
        }

        /// <summary>
        /// Elimina una entrada del nodo.
        /// </summary>
        /// <param name="entrada">Entrada.</param>
        public void eliminarEntrada(int entrada) {
            int indiceEntrada = entradas.IndexOf(entrada);
            if (indiceEntrada == -1) {
                throw new ArgumentException("La entrada especificada no existe en el nodo.");
            }

            int numConfiguracionesAntiguo = funciónBooleana.Length;
            int numConfiguracionesNuevo = numConfiguracionesAntiguo / 2;
            int[] nuevaFuncionBooleana = new int[numConfiguracionesNuevo];

            for (int i = 0; i < numConfiguracionesNuevo; i++) {
                int indiceAntiguo1 = insertarBit(i, indiceEntrada, 0);
                int indiceAntiguo2 = insertarBit(i, indiceEntrada, 1);

                // Promediar o elegir una de las salidas anteriores
                // En este caso, elegimos la salida cuando el bit eliminado es '0'
                nuevaFuncionBooleana[i] = funciónBooleana[indiceAntiguo1];
            }

            funciónBooleana = nuevaFuncionBooleana;
            entradas.RemoveAt(indiceEntrada);
            conectividad = entradas.Count;
        }

        /// <summary>
        /// Inserta un bit en una posición específica en un entero.
        /// </summary>
        /// <param name="valor">Valor original.</param>
        /// <param name="posicion">Posición donde insertar el bit.</param>
        /// <param name="bit">Bit a insertar (0 o 1).</param>
        /// <returns>Nuevo entero con el bit insertado.</returns>
        private int insertarBit(int valor, int posicion, int bit) {
            int mascaraIzquierda = (valor >> posicion) << (posicion + 1);
            int mascaraDerecha = valor & ((1 << posicion) - 1);
            return mascaraIzquierda | (bit << posicion) | mascaraDerecha;
        }

        /// <summary>
        /// Calcula la probabilidad de que la función booleana sea '1'.
        /// </summary>
        /// <returns>Probabilidad de salida '1'.</returns>
        public float probabilidadBooleana() {
            int suma = 0;
            foreach (int salida in funciónBooleana) {
                suma += salida;
            }
            return (float)suma / funciónBooleana.Length;
        }
    }
}
