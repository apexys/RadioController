using System;
using System.Collections.Generic;
using System.IO;
using RadioPlayer;
using RadioLogger;

namespace RadioController
{
	public class RandomShow
	{
		string folderPath;

		List<string> playlist;

		Mplayer element1;

		Mplayer element2;

		int currentElement;

		public RandomShow (string path)
		{
			folderPath = path;
			playlist = new List<string>();
			playlist.Add("");
			refillPlaylist();
			playlist.RemoveAt(0);
			refillPlaylist();
			element1 = new Mplayer(playlist[0]);
			element2 = new Mplayer(playlist[0]);
			currentElement = 2;
			fadeOver();
		}

		void refillPlaylist ()
		{
			DirectoryInfo di = new DirectoryInfo (folderPath);
			List<string> refillList = new List<string> ();
			foreach (FileInfo fi in di.GetFiles()) {
				refillList.Insert (Globals.Random.Next (refillList.Count), fi.Name);
			}
			if (playlist [playlist.Count - 1] == refillList [0]) {
				refillList.Add(refillList[0]);
				refillList.RemoveAt(0);
			}
			playlist.AddRange(refillList);
			Logger.LogDebug("Playlist: ");
			for(int i = 0; i < playlist.Count; i++){
				Logger.LogDebug('[' + i.ToString() + "] " + playlist[i]);
			}
			Logger.LogLine();
			Logger.LogLine();
		}

		void fadeOver ()
		{
			if (currentElement == 1) {
				element2 = new Mplayer (playlist [0]);
				playlist.RemoveAt (0);
				Globals.Mixer.FadeTo (element1, Globals.VOLUME_LOW, Globals.FADE_TIME);
				element2.Volume = 0f;
				element2.Play ();
				Globals.Mixer.FadeTo (element2, Globals.VOLUME_HIGH, Globals.FADE_TIME);
				currentElement = 2;
			} else {
				element1 = new Mplayer (playlist [0]);
				playlist.RemoveAt (0);
				Globals.Mixer.FadeTo (element2, Globals.VOLUME_LOW, Globals.FADE_TIME);
				element1.Volume = 0f;
				element1.Play ();
				Globals.Mixer.FadeTo (element1, Globals.VOLUME_HIGH, Globals.FADE_TIME);
				currentElement = 1;
			}
		}

		public void HandleClockEvent ()
		{

		}
	}
}

