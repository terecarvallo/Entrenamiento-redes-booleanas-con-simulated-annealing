namespace Music2BooleanNetworks {
	class MainClass {

		public static void Main () {

			var archivoImportar = "BinSeqAllOct01.csv";
			var conectividad = 2f;
			var númeroDeNodos = 200;
			var generaciones = 100000;

			Evolucion.EvoluciónDeUnaRed(archivoImportar, conectividad, númeroDeNodos, generaciones);
		}
	}
}