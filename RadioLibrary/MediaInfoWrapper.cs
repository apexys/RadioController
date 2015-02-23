using System;
using System.Diagnostics;

namespace RadioLibrary
{
	public class MediaInfoWrapper
	{
		/// <summary>
		/// Gets metadata for a file using the ´mediainfo´ program
		/// </summary>
		/// <returns>The metadata of the file</returns>
		/// <param name="path">Path of the mediafile</param>
		public static AudioMetaData getMetadata(string path){
			Process process = new Process();
			process.StartInfo.FileName = "mediainfo";
			process.StartInfo.Arguments = "\"" + path + "\"";
			process.StartInfo.UseShellExecute = false;
			process.StartInfo.RedirectStandardOutput = true;
			process.StartInfo.CreateNoWindow = true;

			process.Start ();

			process.WaitForExit ();

			string output = process.StandardOutput.ReadToEnd ();

			AudioMetaData result = new AudioMetaData ();

			foreach(string line in output.Split ('\n')){
				if (line.Contains(":")) {
					string[] parts = line.Split (':');
					string property = parts [0].Trim ();
					string content = parts[1].Trim();
					switch (property) {
					case "Track name":
						result.Title = content;
						break;
					case "Album":
						result.Album = content;
						break;
					case "Performer":
						result.Artist = content;
						break;
					case "Complete name":
						result.Filename = content;
						break;
					default:
						break;
					}
				}
			}

			return result;
		}
	}
}

