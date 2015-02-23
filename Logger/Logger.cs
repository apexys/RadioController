using System;
using System.IO;
using System.Threading.Tasks;

namespace RadioLogger
{
	public class Logger
	{
		public static bool logDebugMessages = true;
		public static bool logToFile = true;
		public static string logFileName = "Radiocontroller.log";
		public static StreamWriter strw = null;

		static void writeLine(string line){
			if (logToFile) {
				try{
				if (strw == null) {
					if (!File.Exists (Environment.CurrentDirectory + Path.PathSeparator + logFileName)) {
						File.WriteAllText (Environment.CurrentDirectory + Path.PathSeparator + logFileName,"");
						Console.WriteLine ("Created logfile at " + Environment.CurrentDirectory + Path.PathSeparator + logFileName);
					}
					strw = new StreamWriter (File.Open (Environment.CurrentDirectory + Path.PathSeparator + logFileName, FileMode.Append));
					strw.AutoFlush = true;
					Console.WriteLine ("Continuing logfile at " + Environment.CurrentDirectory + Path.PathSeparator + logFileName);
				}
				strw.WriteLine (line);
				}catch{
					;
				}
			}
			System.Diagnostics.Debug.WriteLine (line);
		}

		~Logger(){
			logToFile = false;
			strw.Flush ();
			strw.Close ();
		}

		public static string getTimeString(){
			return "["+ DateTime.Now.Year.ToString() + '-' + DateTime.Now.Month.ToString() + '-' + DateTime.Now.Day.ToString() + " " + DateTime.Now.Hour.ToString () + ":" + DateTime.Now.Minute.ToString () + ":" + DateTime.Now.Second.ToString () + "]  ";
		}

		public static void LogLine(){
			Console.WriteLine();
		}

		public static void LogNormal(string message){
			Console.ResetColor();
			Console.WriteLine(getTimeString() + message);
			writeLine ('N' + message);
		}

		public static void LogGood(string message){
			Console.ForegroundColor = ConsoleColor.DarkGreen;
			Console.WriteLine(getTimeString() + message);
			Console.ResetColor();
			writeLine ('G' + message);
		}

		public static void LogInformation(string message){
			Console.ForegroundColor = ConsoleColor.Cyan;
			Console.WriteLine(getTimeString() + message);
			Console.ResetColor();
			writeLine ('I' + message);
		}

		public static void LogWarning(string message){
			Console.ForegroundColor = ConsoleColor.Yellow;
			Console.WriteLine(getTimeString() + message);
			Console.ResetColor();
			writeLine ('W' + message);
		}

		public static void LogError(string message){
			Console.ForegroundColor = ConsoleColor.Red;
			Console.WriteLine(getTimeString() + message);
			Console.ResetColor();
			writeLine ('E' + message);
		}

		public static void LogDebug (string message) {
			if (logDebugMessages) {
				Console.ForegroundColor = ConsoleColor.DarkGray;
				Console.WriteLine(getTimeString() + message);
				Console.ResetColor();
				writeLine ('D' + message);
			}
		}
	}
}

