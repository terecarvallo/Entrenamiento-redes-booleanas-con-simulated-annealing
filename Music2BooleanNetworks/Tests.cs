using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Music2BooleanNetworks {
	public static class Tests {

		public static void t01redBooleana () {
			var gk = new Gk("t01redBooleana.txt");

			var net = new Red();
			net.construir(2, 3, 20, 0.2f);

			var estadoInicial = "00000000000000000000";

			for (int i = 0; i < 100; i++) {
				Console.WriteLine(estadoInicial);
				//estadoInicial = net.actualizarEstado(estadoInicial);
			}

			gk.StopAndOpen("test.txt");
		}

		public static void t02LeerArchivoCSV () {
			var gk = new Gk("t02LeerArchivoCSV.txt");

			var contenido = Gk.LeerArchivo("BinSeqAllOct01.csv");

			var matriz = new List<List<string>>();
			foreach (var s in contenido) {
				var renglón = s.Split(',');
				matriz.Add(renglón.ToList());
			}

			var notas = extraerNotas(matriz);
			var tarea = extraerTarea(matriz, notas);


			foreach (var s in notas) {
				Console.Write(s + " ");
			}
			Console.WriteLine();

			foreach (var s in tarea) {
				Console.WriteLine(s);
			}

			gk.StopAndOpen("t02LeerArchivoCSV.txt");
		}

		/// <summary>
		/// Extraer las notas que están activas.
		/// </summary>
		/// <returns>Las notas de la canción.</returns>
		/// <param name="matriz">Matriz.</param>
		static List<int> extraerNotas (List<List<string>> matriz) {
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


		static List<string> extraerTarea (List<List<string>> matriz, List<int> listaNotas) {
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


		public static void t03Excepción () {
			var FileName = "t03Excepción";
			var gk = new Gk($"{FileName}.txt");

			var s1 = "0000000000";
			var s2 = "01000100010";

			var dif = distanciaHamming(s1, s2);

			Console.WriteLine($"s1: {s1}");
			Console.WriteLine($"s2: {s2}");
			Console.WriteLine($"Diferencia : {dif}");


			gk.StopAndOpen($"{FileName}.txt");
		}

		static int distanciaHamming (string s1, string s2) {
			// Las cadenas deben tener la misma longitud
			if (s1.Length != s2.Length) {
				var lsp = "\r\n============**********================\r\n";
				var método = "Red.distanciaHamming";
				var problema = "Cadenas con diferente longitud";
				throw new ArgumentException($"{lsp}{método}: {problema}.{lsp}");
			}

			int diferencias = 0;
			for (int i = 0; i < s1.Length; i++) {
				if (s1[i] != s2[i]) {
					diferencias++;
				}
			}
			return diferencias;
		}

		public static void t04InterpretaciónDeLaRed () {
			var FileName = MethodBase.GetCurrentMethod().Name + ".txt";
			var gk = new Gk(FileName);

			var melodía = "BinSeqAllOct01.csv";
			var net = new Red();
			net.construir(1, 2, 50, 0.2f);
			net.conectividadForzada = 2f;
			net.asiganarTarea(melodía);
			net.evaluarDesempeño();
			net.crearSonido();
			Extra.ImpLinea("Melodía original");
			Red.convertirCadenasASonido(net.tareaGlobal, net.notas);



			gk.StopAndOpen(FileName);
		}


		public static void t04evluciónDeRedes () {
			var FileName = MethodBase.GetCurrentMethod().Name + ".txt";
			var gk = new Gk(FileName);
			var archivo = "BinSeqAllOct01.csv";

			var net = new Red();
			net.construir(2, 2, 50, 0.2f);
			net.asiganarTarea(archivo);
			net.medirErrorEnLaInterpretacion();

			for (int i = 0; i < 20000; i++) {
				var net2 = net.clonar();
				net2.mutar();
				net2.medirErrorEnLaInterpretacion();
				if (net2.error <= net.error) {
					net = net2;
				}
				if (net.error == 0) {
					net.evaluarDesempeño();
					net.crearSonido();
					net.incrementarTareaLocal(1);
					net.medirErrorEnLaInterpretacion();
				}
			}

			net.evaluarDesempeño();
			net.crearSonido();
			Extra.ImpLinea("Melodía original");
			Red.convertirCadenasASonido(net.tareaGlobal, net.notas);

			gk.StopAndOpen(FileName);
		}


	}
}