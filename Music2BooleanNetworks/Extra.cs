using System;
using System.Collections.Generic;
using System.Linq;

namespace Music2BooleanNetworks {
	public class Extra {

		private static int seedCounter = Environment.TickCount;
        private static readonly object seedLock = new object();

        private static int GetSeed()
        {
            lock (seedLock)
            {
                return seedCounter++;
            }
        }

        // ThreadLocal Random
        private static ThreadLocal<Random> rnd = new ThreadLocal<Random>(() => new Random(GetSeed()));

		/// <summary>
		/// Convertir número en forma binaria con determinada longitud.
		/// </summary>
		/// <returns>Número binario.</returns>
		/// <param name="númeroDecimal">Numero decimal.</param>
		/// <param name="longitudTotal">Longitud total del número en binario.</param>
		public static string númeroBinario (int númeroDecimal, int longitudTotal) {
			var binario = Convert.ToString(númeroDecimal, 2);
			return new string('0', longitudTotal - binario.Length) + binario;
		}

		/// <summary>
		/// Cadena sin el caracter situado en la posición que elijas.
		/// </summary>
		/// <param name="s">Cadena original.</param>
		/// <param name="idx">Índice del caracter que se quiere quitar.</param>
		/// <returns></returns>
		public static string cortarCadena (string s, int idx) {
			return s.Substring(0, idx) + s.Substring(idx + 1);
		}

		/// <summary>
		/// Lista con elementos desordenados.
		/// </summary>
		/// <returns>Lista desordenada.</returns>
		/// <param name="n">N.</param>
		public static List<int> listaDesordenada(int n)
        {
            var lO = Enumerable.Range(0, n).ToList();
            var lD = new List<int>();
            for (int i = 0; i < n; i++)
            {
                int z = lO[rnd.Value.Next(lO.Count)];
                lD.Add(z);
                lO.Remove(z);
            }
            return lD;
        }

		/// <summary>
		/// Elegir un número aleatorio, siempre y cuando no sea alguno 
		/// de los que están en una lista determinada.
		/// </summary>
		/// <returns>El número aleatorio deseado.</returns>
		/// <param name="mínimo">Mínimo.</param>
		/// <param name="máximo">Máximo.</param>
		/// <param name="evitar">Evitar.</param>
		public static int númeroAleatorioConExcepciones(int mínimo, int máximo, List<int> evitar)
        {
            var número = rnd.Value.Next(mínimo, máximo);
            while (evitar.Contains(número))
            {
                número = rnd.Value.Next(mínimo, máximo);
            }
            return número;
        }

		/// <summary>
		/// Elegir un elemento aleatorio de una lista.
		/// </summary>
		/// <returns>Elemento elegido de forma aleatoria.</returns>
		/// <param name="elegidos">Lista de elementos.</param>
		public static int elementoAleatorio(List<int> elegidos)
        {
            var indice = rnd.Value.Next(elegidos.Count);
            return elegidos[indice];
        }

		/// <summary>
		/// Escribir texto entre dos líneas horizontales
		/// para dar formato de título. 
		/// </summary>
		/// <param name="str"></param>
		public static void ImpLinea (string str) {
			Console.WriteLine(new string('=', str.Length + 6));
			Console.WriteLine("   " + str);
			Console.WriteLine(new string('=', str.Length + 6));
		}
	}
}