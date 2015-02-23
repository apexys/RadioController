using System;
using System.Diagnostics;
using System.Threading;
using System.IO;
using System.Collections.Generic;
using RadioController.Configuration;
using RadioPlayer;
using RadioLogger;

namespace RadioController
{
	class MainClass
	{
		public static void Main(string[] args) {

			Settings.setSettings(new FileSettings("../../../Settings.conf"));
			Settings.setString("hello.123", "hi");

			/*
			foreach (KeyValuePair<string, string> pair in ((FileSettings)(Settings.getSettings())).config) {
				Logger.LogGood(pair.Key + " => " + pair.Value);
			}
			Logger.LogGood("int: " + Settings.getInt("vlc.start", 2).ToString());
			*/

			string[] songs = Settings.getStrings("media.songs", new string[] {"hi"});
			string[] jingles = Settings.getStrings("media.jingles", new string[] {"hi"});
			string[] news = Settings.getStrings("media.news", new string[] {"hi"});


			//1. Try to find a version of mplayer
			Logger.LogNormal("Looking for mplayer...");
			string mplayer_version = Mplayer.getVersion();
			if (mplayer_version != "") {
				Logger.LogGood(mplayer_version);
			} else {
				Logger.LogError("No mplayer found!");
			}
			Logger.LogLine();
			Logger.LogDebug("Environment: " + Environment.CurrentDirectory);

			//2. Create a Controller
			TimedTrigger jinglett = new TimedTrigger();
			jinglett.addMinuteTrigger(0);
			jinglett.addMinuteTrigger(15);
			jinglett.addMinuteTrigger(30);
			jinglett.addMinuteTrigger(45);

			TimedTrigger newstt = new TimedTrigger();
			newstt.addMinuteTrigger(0);
			newstt.addMinuteTrigger(30);

			//string basepath = "/home/apexys/content"; // @"/home/streamer/radiosoftware/content";

			Controller ctrl = new Controller(
				songs[0],//basepath + Path.DirectorySeparatorChar + "songs",
				jingles[0],//basepath + Path.DirectorySeparatorChar + "jingles",
				news[0],//basepath + Path.DirectorySeparatorChar + "news",
				jinglett,
				newstt,
				"http://134.245.206.50:8080/");
			ctrl.Start();

			//3. Run for your life!
			Console.Write("Everything running, type ");
			Console.ForegroundColor = ConsoleColor.Green;
			Console.Write("exit");
			Console.ResetColor();
			Console.WriteLine(" to quit.");

			interact(ctrl);
		}

		static void interact(Controller ctrl) {
			while (true) {
				string[] input = Console.ReadLine().Trim().Split(' ');
				if (input.Length > 0) {
					switch (input[0].ToLower()) {
					case "exit":
						Console.WriteLine("Terminting program");
						//TODO: Make sure all mplayers DIE here
						ctrl.Dispose();
						Environment.Exit(0);
						break;
					case "s":
					case "stop":
						ctrl.Stop();
						break;
					case "p":
					case "start":
					case "play":
						ctrl.Start();
						break;
					case "h":
					case "pause":
						ctrl.Pause();
						break;
					case "fi":
					case "fadein":
						ctrl.fadeIn();
						break;
					case "fo":
					case "fadeout":
						ctrl.fadeOut();
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
}
