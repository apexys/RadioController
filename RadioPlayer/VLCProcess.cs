using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using Configuration;
using RadioLogger;
using RadioLibrary;
using System.Threading;
using System.Threading.Tasks;

namespace RadioPlayer
{
	public class VLCProcess : IDisposable
	{
		Process process;
		StreamReader sout;
		StreamWriter sin;
		bool kill = false;
		float volumeAdjustment = 1f;
		public StreamWriter debugStream;

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
			process.Exited += exited;

			process.Start();

			sin = process.StandardInput;
			sin.AutoFlush = true;

			sout = process.StandardOutput;

			sin = new DebugWrite(sin.BaseStream);
			sin.AutoFlush = true;
			debugStream = new StreamWriter(Console.OpenStandardOutput());


			//debugProcess();
			// this call can throw an error

			string data = getData();
			string version = data.Substring(0, data.IndexOfAny(new char[] {'\n', '\r'}));

			Logger.LogInformation("VLC version " + version);
		}

		~VLCProcess () {
			Dispose();
		}

		public void Dispose() {
			Logger.LogInformation("VLCProcess got Disposed");
			kill = true;
			try {
				sin.WriteLine("shutdown");
				Task.Run(async () => {
					await Task.Delay(1000);
					if (!process.HasExited) {
						Logger.LogWarning("VLCProcess didn't stop properly");
						process.Kill();
					}
				});
			} catch {
			}
		}

		public void setFile(MediaFile mf) {
			sin.WriteLine(
				"clear\n" +
				"enqueue " + mf.Path);
			dropData();
			dropData();
		}

		public void play() {
			sin.WriteLine("play");
			dropData();
		}

		public void pause() {
			sin.WriteLine("pause");
			dropData();
		}

		public int getPosition() {
			sin.WriteLine("get_time");
			string s = getData();
			if (s == "") {
				return 0;
			}
			return int.Parse(s);
		}

		public int getLength() {
			sin.WriteLine("get_length");
			string s = getData();
			if (s == "") {
				return 0;
			}
			return int.Parse(s);
		}

		public void setVolume(int volume) {
			sin.WriteLine("volume " + ((int)(volume * volumeAdjustment)).ToString());
			dropData();
		}

		public int getVolume() {
			sin.WriteLine("volume");
			return (int)(int.Parse(getData()) / volumeAdjustment);
		}

		void debugStreamPeek(StreamReader s) {
			s.Peek();
		}

		public void debugProcess() {
			string str;
			string[] sp;
			bool run = true;

			//Task.Run(() => { while (true) {Logger.LogGood("Data: "+getData());} } );

			while (run) {
				str = Console.ReadLine();
				sp = str.Split(new char[] { ' ' }, 2);
				sp[0] = sp[0].ToLower();

				switch (sp[0]) {
				case "volume":
					if (sp.Length > 1) {
						sin.WriteLine("volume " + int.Parse(sp[1]).ToString());
					} else {
						sin.WriteLine("volume");
						Console.WriteLine(getData());
					}
					break;
				case "seek":
				case "goto":
					if (sp.Length > 1) {
						sin.WriteLine(sp[0] + " " + int.Parse(sp[1]).ToString());
						dropData();
						sin.WriteLine("get_time");
						Console.WriteLine(getData());
						Thread.Sleep(500);
						sin.WriteLine("get_time");
						Console.WriteLine(getData());

					}
					break;
				case "add":
					if (sp.Length > 1) {
						sin.WriteLine("enqueue " + new FileInfo(Settings.getStrings("media.songs", new string[] { "./" })[0] + sp[1]).FullName);
					}
					dropData();
					break;
				case "get_time":
					Logger.LogInformation("Position: " + getPosition().ToString());
					break;
				case "get_length":
				case "play":
				case "pause":
				case "shutdown":
				case "clear":
				case "status":
				case "playlist":
					sin.WriteLine(sp[0]);
					Console.WriteLine(getData());
					break;
				case "exit":
					run = false;
					break;
				}

			}
		}

		string getData() {
			string s;
			int i = 0;
			char c;

			StringBuilder sb = new StringBuilder();

			while (!sout.EndOfStream && i<2) {
				c = (char)sout.Read();
				if (c == '>') {
					i = 1;
				} else if (i == 1 && c == ' ') {
					break;
				}
				sb.Append(c);
			}

			sb.Remove(sb.Length - 1, 1);
			s = sb.ToString();
			
			if (debugStream != null) {
				debugStream.WriteLine(s.Replace("\033", "ESC"));
			}

			return s.Trim(new char[] { '\r', '\n', ' ', '\t' });
		}

		void dropData() {
			int i = 0;
			char c;

			while (!sout.EndOfStream && i<2) {
				c = (char)sout.Read();
				if (c == '>') {
					i = 1;
				} else if (i == 1 && c == ' ') {
					break;
				}
			}
		}

		void exited(Object sender, EventArgs args) {
			if (!kill) {
				Logger.LogError("VLC exited unexpected");
			}
		}

		class DebugWrite : StreamWriter
		{

			public DebugWrite(Stream stream) : base(stream) {
			}

			public override void WriteLine(string s) {
				if (s != "get_time") {
					Logger.LogInformation("VLC got: \"" + s + "\"");
				}
				base.WriteLine(s);
			}
		}
	}
}

