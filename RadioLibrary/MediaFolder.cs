using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using RadioLogger;

namespace RadioLibrary
{
	public class MediaFolder
	{
		Random rnd;
		List<int> lastPlayed;
		public string[] paths;
		EMediaType defaultType;

		public string[] Paths {
			get {
				return paths;
			}
		}

		List<MediaFile> files;
		const string allowedInputs = ".mp3.mp4.flac.m4a.aac";

		public void Refresh() {
			string path;
			lastPlayed = new List<int>();
			files = new List<MediaFile>();

			for (int i = 0; i<paths.Length; i++) {
				path = paths[i];

				if (Directory.Exists(path)) {
					//Get Media Infos
					DirectoryInfo di = new DirectoryInfo(path);
					foreach (FileInfo file in di.GetFiles()) {
						if (file.Name.Contains(".") && allowedInputs.Contains(file.Extension)) {
							files.Add(new MediaFile(file.FullName, defaultType));
							Logger.LogDebug("Added " + file.Name);
						}
					}
				}
			}

			Logger.LogInformation("Loaded " + files.Count.ToString() + " items from " + Configuration.JSON.write(paths));
		}

		public MediaFile pickRandomFile() {

			int next;
			do {
				next = rnd.Next(0, files.Count);
			} while(lastPlayed.Contains(next));


			lastPlayed.Add(next);

			if (lastPlayed.Count > (files.Count / 4)) {
				lastPlayed.RemoveAt(0);
			}
			return files[next];
		}

		public MediaFolder(string[] folderPaths) : this(folderPaths, EMediaType.Normal) {
		}

		public MediaFolder(string[] folderPaths, EMediaType type) {
			this.paths = folderPaths;
			defaultType = type;
			rnd = new Random(DateTime.Now.Millisecond);
			Refresh();
		}
	}
}

