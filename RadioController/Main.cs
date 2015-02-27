using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using System.IO;
using System.Collections.Generic;
using Configuration;
using RadioPlayer;
using RadioLibrary;
using RadioLogger;

namespace RadioController
{
	class MainClass
	{
		static MediaFolder songs;
		static MediaFolder jingles;
		static MediaFolder news;

		public static void Main(string[] args) {

			Settings.setSettings(new FileSettings("../../../Settings.conf"));

			/*
			foreach (KeyValuePair<string, string> pair in ((FileSettings)(Settings.getSettings())).config) {
				Logger.LogGood(pair.Key + " => " + pair.Value);
			}
			Logger.LogGood("int: " + Settings.getInt("vlc.start", 2).ToString());
			//*/

			FileInfo fi = new FileInfo("../");
			Console.WriteLine(fi.Directory + " -/- " + fi.Directory.Name + " -/- " + fi.Directory.FullName);


			string[] songFolders = Settings.getStrings("media.songs", new string[] { });
			string[] jingleFolders = Settings.getStrings("media.jingles", new string[] { });
			string[] newsFolders = Settings.getStrings("media.news", new string[] { });

			//2. Create a Controller
			TimedTrigger jinglett = new TimedTrigger();
			int[] trigger = Settings.getInts("trigger.jingles", new int[] { 0, 15, 30, 45 });
			for (int t = 0; t < trigger.Length; t++) {
				jinglett.addMinuteTrigger(trigger[t]);
			}
			
			TimedTrigger newstt = new TimedTrigger();
			trigger = Settings.getInts("trigger.news", new int[] { 0, 30 });
			for (int t = 0; t < trigger.Length; t++) {
				newstt.addMinuteTrigger(trigger[t]);
			}

			Task.Run(async () => {
				while (true) {
					jinglett.checkTriggers();
					newstt.checkTriggers();
					await Task.Delay(500);
				}
			});

			VLCMixer mixer = new VLCMixer();
			songs = new MediaFolder(songFolders);
			jingles = new MediaFolder(jingleFolders, EMediaType.FullVolume);
			news = new MediaFolder(newsFolders);

			IMediaFileProvider provider = new RandomMediaFileProvider(songs);
			provider = new TimedTriggerMediaFileInserter(provider, jinglett, new RandomMediaFileProvider(jingles));
			provider = new TimedTriggerMediaFileInserter(provider, newstt, new RandomMediaFileProvider(news));

			IController ctrl = new BasicController(mixer, new MediafileToSoundObjectProvider(mixer, provider));
			ctrl.Start();

			//3. Run for your life!
			Console.Write("Everything running, type ");
			Console.ForegroundColor = ConsoleColor.Green;
			Console.Write("exit");
			Console.ResetColor();
			Console.WriteLine(" to quit.");

			while(true) {
				try {
					interact(ctrl);
				} catch (Exception ex) {
					Logger.LogException(ex);
				}
			}
		}

		static void interact(IController ctrl) {
			while (true) {
				string[] input = Console.ReadLine().Trim().Split(' ');
				if (input.Length > 0) {
					switch (input[0].ToLower()) {
					case "?":
					case "help":
						Console.WriteLine(
							"The following commands are currently available\n" +
							"s(top)\n" +
							"p(lay)/start\n" +
							"h/pause\n" +
							"f(ade)i(n)\n" +
							"f(ade)o(ut)\n" +
							"rescan\n" +
							"skip\n" +
							"vlc <id>");
						break;
					case "exit":
						Console.WriteLine("Do you really want to exit this programm? y/n: ");
						if (Console.ReadLine() == "y") {
							Console.WriteLine("Terminting program");
							//TODO: Make sure all mplayers DIE here
							//ctrl.Dispose();
							Environment.Exit(0);
						}
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
						songs.Refresh();
						jingles.Refresh();
						news.Refresh();
					///ctrl.Rescan();
					//Logger.LogNormal("Scan done");
					//Logger.LogLine();
						break;
					case "skip":
						Logger.LogNormal("Skipping current action");
						ctrl.Skip();
						break;
					case "vlc":
						VLCMixer.mixer.getCurrentPlayers()[int.Parse(input[1])].debugProcess();
						break;
					default:
						break; //Nothing to do here
					}
				}
			}
		}
	}
}
