using System;
using System.Diagnostics;
using System.IO;
using Configuration;
using RadioLogger;

namespace RadioPlayer
{
	public class VLCProcess : ISoundObject
	{
		Process process;
		StreamReader sout;
		StreamWriter sin;

		TimeSpan position;
		TimeSpan length;

		bool playing = false;

		string version = "";

		DateTime lastPlaying;
		DateTime lastPaused;

		//AudioMetaData metadata;

		public VLCProcess() {

			process = new Process();
			process.StartInfo.FileName = Settings.getString("vlc.cmd_prog", "cvlc");
			process.StartInfo.Arguments = Settings.getString("vlc.cmd_args", "-I rc");
			process.StartInfo.UseShellExecute = false;
			process.StartInfo.RedirectStandardInput = true;
			process.StartInfo.RedirectStandardOutput = true;
			//process.StartInfo.RedirectStandardError = true;
			process.StartInfo.CreateNoWindow = true;
			process.EnableRaisingEvents = true;



			sin = process.StandardInput;
			sin.AutoFlush = true;

			sout = process.StandardOutput;

			// this call can throw an error
			process.Start();

			version = sout.ReadLine();

			Logger.LogInformation ("VLC version " + version);

			position = new TimeSpan(0, 0, 0);
			length = new TimeSpan(0, 0, 0);

			//metadata = new AudioMetaData();


		}

		#region ISoundObject implementation

		public float Volume {
			get {
				throw new NotImplementedException ();
			}
			set {
				throw new NotImplementedException ();
			}
		}

		public bool Playing {
			get {
				return playing;
			}
			set {
				if (value != playing) {
					if (value == true) {
						sin.WriteLine ("play");

						playing = true;
					} else {
						sin.WriteLine ("pause");
						playing = false;
					}
				}
			}
		}

		public float Position {
			get {
				throw new NotImplementedException ();
			}
			set {
				throw new NotImplementedException ();
			}
		}

		public float Duration {
			get {
				throw new NotImplementedException ();
			}
		}

		public string Title {
			get {
				throw new NotImplementedException ();
			}
		}

		#endregion
	}
}

