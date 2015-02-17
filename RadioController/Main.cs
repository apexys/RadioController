using System;
using System.Diagnostics;
using System.Threading;
using System.IO;

namespace RadioController
{
	class MainClass
	{
		public static void Main (string[] args)
		{
			//1. Try to find a version of mplayer
			Logger.LogNormal ("Looking for mplayer...");
			string mplayer_version = Mplayer.getVersion ();
			if (mplayer_version != "") {
				Logger.LogGood (mplayer_version);
			} else {
				Logger.LogError ("No mplayer found!");
			}
			Logger.LogLine ();
			Logger.LogDebug ("Environment: " + Environment.CurrentDirectory);

			//2. Create a Controller
			TimedTrigger jinglett = new TimedTrigger ();
			jinglett.addMinuteTrigger(0);
			jinglett.addMinuteTrigger(15);
			jinglett.addMinuteTrigger(30);
			jinglett.addMinuteTrigger(45);

			TimedTrigger newstt = new TimedTrigger ();
			newstt.addMinuteTrigger(0);
			newstt.addMinuteTrigger(30);

			string basepath = "/home/apexys/content"; // @"/home/streamer/radiosoftware/content";

			Controller ctrl = new Controller(
						basepath + Path.DirectorySeparatorChar + "songs",
				        basepath + Path.DirectorySeparatorChar + "jingles",
				        basepath + Path.DirectorySeparatorChar + "news",
				        jinglett,
				        newstt,
				        "http://134.245.206.50:8080/");
			ctrl.Start ();

			//3. Run for your life!
			Console.Write ("Everything running, type ");
			Console.ForegroundColor = ConsoleColor.Green;
			Console.Write ("exit");
			Console.ResetColor ();
			Console.WriteLine(" to quit.");

			interact(ctrl);
		}
 

		static void interact(Controller ctrl){
			while (true) {
				string input = Console.ReadLine().Trim();
				switch (input) {
				case "exit":
					Console.WriteLine("Terminting program");
					//TODO: Make sure all mplayers DIE here
					Environment.Exit(0);
					break;
				
				case "rescan":
					Logger.LogNormal("Rescanning media collection");
					// Retraverse Meida Directory
					ctrl.Rescan();
					Logger.LogNormal("Scan done");
					Logger.LogLine();
					break;
				case "skip":
					Logger.LogNormal("Skipping current action");
					ctrl.Skip();
					break;
				default:
					break; //Nothing to do here
				}
			}
		}


	}
}
