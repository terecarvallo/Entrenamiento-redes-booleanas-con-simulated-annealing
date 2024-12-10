using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using static System.Console;

namespace Music2BooleanNetworks {
	class Gk {
		public TextWriter tmp;
		public StreamWriter sw;

		public Gk (string FileName, bool Append = false) {
			var Ruth = FileName.Split('/');
			for (int i = 0; i < Ruth.Length - 1; i++) {
				string Ruth2 = "./";
				for (int j = 0; j < i; j++) { Ruth2 += Ruth[j] + "//"; }
				try { Directory.CreateDirectory(Ruth2 + Ruth[i]); } catch (Exception e) { WriteLine(e); }
			}
			if (!Append) {
				if (File.Exists(FileName)) {
					try { File.Delete(FileName); } catch (IOException e) {
						WriteLine(e.Message);
						return;
					}
				}
			}
			var fs = new FileStream(FileName, FileMode.Append);
			tmp = Out;
			sw = new StreamWriter(fs);
			SetOut(sw);
		}

		/// <summary>
		/// Comenzar a escribir en un archivo de texto.
		/// </summary>
		/// <param name="FileName">File name. Nombre del archivo de texto.</param>
		/// <param name="Append">If set to <c>true</c> Añadir en lugar de sobreescribir.</param>
		public void StartWriting (string FileName, bool Append = false) {
			var Ruth = FileName.Split('/');
			for (int i = 0; i < Ruth.Length - 1; i++) {
				string Ruth2 = "./";
				for (int j = 0; j < i; j++) { Ruth2 += Ruth[j] + "//"; }
				try { Directory.CreateDirectory(Ruth2 + Ruth[i]); } catch (Exception e) { WriteLine(e); }
			}
			if (!Append) {
				if (File.Exists(FileName)) {
					try { File.Delete(FileName); } catch (IOException e) {
						WriteLine(e.Message);
						return;
					}
				}
			}
			var fs = new FileStream(FileName, FileMode.Append);
			tmp = Out;
			sw = new StreamWriter(fs);
			SetOut(sw);
		}

		/// <summary>
		/// Parar de escribir en un archivo de texto.
		/// </summary>
		public void StopWriting () {
			SetOut(tmp);
			sw.Close();
		}

		/// <summary>
		/// Abrir archivo de texto.
		/// </summary>
		/// <param name="FileName">File name.</param>
		public void OpenFile (string FileName) {
			var name = FileName.Split('/');
			string FileToOpen = name[0];
			for (int i = 1; i < name.Length; i++) { FileToOpen += "\\" + name[i]; }
			Process.Start(FileToOpen);
		}

		/// <summary>
		/// Parar de escribir y abrir archivo de texto.
		/// </summary>
		/// <param name="FileName">File name.</param>
		public void StopAndOpen(string FileName)
        {
            StopWriting();

            // Utilizar el comando 'open' en macOS
            var process = new Process();
            process.StartInfo.FileName = "open";
            process.StartInfo.Arguments = FileName;
            process.StartInfo.UseShellExecute = false;
            process.StartInfo.CreateNoWindow = true;
            process.Start();
        }


		/// <summary>
		/// Almacenar y exportar en una lista el contenido
		/// de de un archivo de texto.
		/// </summary>
		/// <param name="FileName">Nombre del archivo.</param>
		/// <returns></returns>
		public static List<string> LeerArchivo (string FileName) {
			var Contenido = new List<string>();
			var reader = new StreamReader(File.OpenRead(FileName));
			while (!reader.EndOfStream) {
				var line = reader.ReadLine();
				Contenido.Add(line);
			}
			reader.Close();
			return Contenido;
		}
	}
}