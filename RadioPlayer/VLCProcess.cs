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
		string id;
		Object communicationLock = new object();

		public VLCProcess(string id) {
			this.id = id;
			createProcess();
		}

		void createProcess() {
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


			// Debug:
			//sin = new DebugWrite(sin.BaseStream, id);
			//sin.AutoFlush = true;
			//debugStream = new StreamWriter(Console.OpenStandardOutput());
			//debugStream.AutoFlush = true;

			string data = getData(null);
			string version = data.Substring(0, data.IndexOfAny(new char[] { '\n', '\r' }));

			Logger.LogInformation("VLC [" + id + "] version " + version);
		}

		~VLCProcess () {
			Dispose();
		}

		public void Dispose() {
			Logger.LogInformation("VLCProcess [" + id + "] got Disposed");
			kill = true;
			try {
				sin.WriteLine("shutdown");
				Task.Run(async () => {
					await Task.Delay(1000);
					if (!process.HasExited) {
						Logger.LogWarning("VLCProcess [" + id + "] didn't stop properly");
						process.Kill();
					}
				});
			} catch {
			}
		}

		public void setFile(MediaFile mf) {
			dropData("clear");
			dropData("add " + mf.Path);
			dropData("pause");
			dropData("seek 0");
		}

		public void play() {
			dropData("play");
		}

		public void pause() {
			dropData("pause");
		}

		public int getPosition() {
			string s = getData("get_time");
			if (s == "") {
				return -1;
			}
			return Convert.ToInt32(s);
		}

		public int getLength() {
			string s = getData("get_length");
			if (s == "") {
				return -1;
			}
			return Convert.ToInt32(s);
		}

		public void setPosition(int sec) {
			dropData("seek " + sec);
		}

		public void setVolume(int volume) {
			dropData("volume " + (Convert.ToInt32(volume * volumeAdjustment)).ToString());
		}

		public int getVolume() {
			return Convert.ToInt32(Convert.ToSingle(getData("volume")) / volumeAdjustment);
		}

		void debugStreamPeek(StreamReader s) {
			s.Peek();
		}

		public void debugProcess() {
			string str;
			string[] sp;
			bool run = true;

			while (run) {
				str = Console.ReadLine();
				sp = str.Split(new char[] { ' ' }, 2);
				sp[0] = sp[0].ToLower();

				switch (sp[0]) {
				case "volumeAdjustment":
					if (sp.Length > 1) {
						try {
							volumeAdjustment = Convert.ToSingle(sp[1]);
						} catch {
						}
					} else {
						Console.WriteLine("current Adjustment: " + Math.Round(volumeAdjustment, 2).ToString());
					}
					break;
				case "volume":
					if (sp.Length > 1) {
						setVolume(Convert.ToInt32(sp[1]));
					} else {
						Console.WriteLine("volume: "+ getVolume().ToString());
					}
					break;
				case "seek":
					if (sp.Length > 1) {
						dropData(sp[0] + " " + int.Parse(sp[1]).ToString());
						Console.WriteLine(getData("get_time"));
						Thread.Sleep(100);
						Console.WriteLine(getData("get_time"));

					}
					break;
				case "add":
					if (sp.Length > 1) {
						dropData("enqueue " + new FileInfo(Settings.getString("vlc.debug.mediaBaseFolder", "./") + sp[1]).FullName);
					}
					break;
				case "get_time":
					Console.WriteLine(getPosition());
					break;
				case "get_length":
				case "play":
				case "pause":
				case "shutdown":
				case "clear":
				case "status":
				case "playlist":
					Console.WriteLine(getData(sp[0]));
					break;
				case "exit":
					run = false;
					break;
				case "?":
				case "help":
					Console.WriteLine(
						"The VLC interface supports the following commands:\n" +
						"volumeAdjustment <factor>\n" +
						"volume\n" +
						"volume <new volume>\n" +
						"seek <to second>\n" +
						"add <sound source>\n" +
						"get_time\n" +
						"get_length\n" +
						"play\n" +
						"pause\n" +
						"shutdown\n" +
						"clear\n" +
						"status\n" +
						"playlist\n" +
						"exit");
					break;
				}

			}
		}

		string getData(string command) {
			string s;
			int i = 0;
			char c;
			StringBuilder sb = new StringBuilder();

			lock (communicationLock) {
				// special handling to get the init help text
				if (command != null) {
					sin.WriteLine(command);
				}

				while (!sout.EndOfStream) {
					c = (char)sout.Read();
					if (c == '>') {
						i = 1;
					} else if (i == 1 && c == ' ') {
						i = 2;
						break;
					}
					sb.Append(c);
				}
			}
			sb.Remove(sb.Length - 1, 1);
			s = sb.ToString().Trim(new char[] { '\r', '\n', ' ', '\t' });
			
			if (debugStream != null && !kill) {
				if (i < 2 && sout.EndOfStream) {
					debugStream.WriteLine("VLC [" + id + "] <EOF> reached");
				} else {
					debugStream.WriteLine("VLC [" + id + "]" +
						(command != null ? " (" + command + ")" : "") +
						" Out: " + s.Replace("\033", "ESC"));
				}
			}
			
			return s;
		}

		void dropData(string command) {
			int i = 0;
			char c;

			lock (communicationLock) {
				sin.WriteLine(command);

				while (!sout.EndOfStream && i<2) {
					c = (char)sout.Read();
					if (c == '>') {
						i = 1;
					} else if (i == 1 && c == ' ') {
						break;
					}
				}
			}
		}

		void exited(Object sender, EventArgs args) {
			if (!kill) {
				Logger.LogError("VLC [" + id + "] exited unexpected");
				createProcess();
			}
			kill = true;
		}

		class DebugWrite : StreamWriter
		{
			string id;

			public DebugWrite(Stream stream, string id) : base(stream) {
				this.id = id;
			}

			public override void WriteLine(string s) {
				if (s != "get_time") {
					Logger.LogInformation("VLC [" + id + "] got: \"" + s + "\"");
				}
				base.WriteLine(s);
			}
		}
	}
}

