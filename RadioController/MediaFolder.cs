using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;

namespace RadioController
{
	public class MediaFolder
	{
		Random r;
		int lastIndex;
		public string path;

		public string Path {
			get {
				return path;
			}
		}

		List<MediaFile> files;
		const string allowedInputs = ".mp3.mp4.flac.m4a.aac";

		public void Refresh ()
		{
			files = new List<MediaFile> ();
			lastIndex = 0;
			//Get Media Infos
			DirectoryInfo di = new DirectoryInfo (path);
			foreach (FileInfo file in di.GetFiles()) {
				if (file.Name.Contains (".") && allowedInputs.Contains (file.Extension)) {
					files.Add (new MediaFile (file.FullName));
					Logger.LogDebug ("Added " + file.Name);
				}
			}

			Logger.LogInformation ("Loaded " + files.Count.ToString () + " items from " + path);
		}

		public MediaFile pickRandomFile ()
		{
			if (files.Count == 1) {
				return files [0];
			} else {
				int temp;
				do {
					temp = r.Next (0, files.Count);
				} while(temp == lastIndex);
				lastIndex = temp;
				return files [temp];
			}
		}

		public MediaFolder (string folderPath)
		{
			this.path = folderPath;
			r = new Random (DateTime.Now.Millisecond);
			Refresh ();
		}
	}
}

