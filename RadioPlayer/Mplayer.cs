using System;
using System.Diagnostics;
using System.IO;
using System.Timers;
using System.Threading.Tasks;
using RadioLogger;

namespace RadioPlayer
{
	public class Mplayer
	{
		Process mplayer_process;
		StreamWriter mplayer_input;
		const float delta_volume = 0.1f;
		float target_volume = 0f;
		float mplayer_volume = 0f;
		Timer refresherTimer;
		bool mplayer_paused = false;
		bool events_running = false;
		bool still_alive = false;
		int ticks_since_last_message = 0;
		const int ticks_to_death = 8;
		TimeSpan mplayer_position;
		TimeSpan mplayer_length;
		//AudioMetaData mplayer_metadata;

		~Mplayer() {
			Dispose();
		}

		public void Dispose() {
			refresherTimer.Stop();
			try {
				Volume = 0f;
				try {
					mplayer_input.WriteLine("stop");
				} catch {
				}
				mplayer_process.Kill();
			} catch {
				//Nothing to do here, sometimes it has already collected the garbage and sometimes not
			}
			refresherTimer.Dispose();
		}

		public Mplayer(string path) {
			Task.Run(() => {
				string arguments = "-slave -quiet \"" + path + "\"";
				Console.WriteLine("CMD: mplayer " + arguments);

				mplayer_process = new Process();
				mplayer_process.StartInfo.FileName = "mplayer";
				mplayer_process.StartInfo.Arguments = arguments; //"/home/apexys/Music/test.mp3";
				mplayer_process.StartInfo.UseShellExecute = false;
				mplayer_process.StartInfo.RedirectStandardInput = true;
				mplayer_process.StartInfo.RedirectStandardOutput = true;
				mplayer_process.StartInfo.RedirectStandardError = true;
				mplayer_process.StartInfo.CreateNoWindow = true;
				mplayer_process.OutputDataReceived += HandleOutputDataReceived;
				mplayer_process.ErrorDataReceived += HandleErrorDataReceived;
				mplayer_process.EnableRaisingEvents = true;
				// this call can throw an error
				mplayer_process.Start();

				mplayer_process.BeginErrorReadLine();
				mplayer_process.BeginOutputReadLine();
				mplayer_input = mplayer_process.StandardInput;
				mplayer_input.AutoFlush = true;

				refresherTimer = new Timer(250);
				refresherTimer.Elapsed += HandleRefresherTimerElapsed;
				refresherTimer.Start();

				mplayer_position = new TimeSpan(0, 0, 0);
				mplayer_length = new TimeSpan(0, 0, 0);

				//mplayer_metadata = new AudioMetaData();
				//mplayer_metadata.Filename = path;

				refreshValues();
				Pause();

				still_alive = true;


			});

			// TODO: handle inside timer
			int i;
			for (i = 0; !events_running && i<20; i++) {
				System.Threading.Thread.Sleep(100);
			}

			Volume = 0f;

			if (i == 20) {
				Logger.LogError("MPlayer didn't respond to message");
			}

		}

		void HandleRefresherTimerElapsed(object sender, ElapsedEventArgs e) {
			if (!mplayer_paused) {
				refreshValues();
				ticks_since_last_message ++;
				if (ticks_since_last_message > ticks_to_death) {
					still_alive = false;
				}
			}
		}

		void refreshValues() {
			try {
				mplayer_input.WriteLine("get_property volume");
				mplayer_input.WriteLine("get_time_pos");
				mplayer_input.WriteLine("get_time_length");
			} catch {
			}
		}

		void HandleErrorDataReceived(object sender, DataReceivedEventArgs e) {
			if (e.Data.Trim() != "") {
				Logger.LogError("MPlayer: " + e.Data);
			}
		}

		void HandleOutputDataReceived(object sender, DataReceivedEventArgs e) {
			if (e.Data != null) {
				events_running = true;
				// reset timeout
				ticks_since_last_message = 0;

				if (e.Data.StartsWith("ANS_volume=")) {
					mplayer_volume = float.Parse(e.Data.Substring("ANS_volume=".Length).Trim());
					if (Math.Abs(target_volume - mplayer_volume) > delta_volume) {
						setVolume(target_volume);
					}
				}

				if (e.Data.StartsWith("ANS_TIME_POSITION=")) {
					float seconds_position = float.Parse(e.Data.Substring("ANS_TIME_POSITION=".Length).Trim());
					mplayer_position = TimeSpan.FromSeconds(Convert.ToDouble(seconds_position));
				}

				if (e.Data.StartsWith("ANS_LENGTH=")) {
					float seconds_length = float.Parse(e.Data.Substring("ANS_LENGTH=".Length).Trim());
					mplayer_length = TimeSpan.FromSeconds(Convert.ToDouble(seconds_length));
				}


				/*
				if (e.Data.Trim().ToLower().StartsWith("title: ")) {
					string title = e.Data.Trim().Substring("Title: ".Length).Trim();
					mplayer_metadata.Title = title;
				}

				if (e.Data.Trim().ToLower().StartsWith("album: ")) {
					string album = e.Data.Trim().Substring("Album: ".Length).Trim();
					mplayer_metadata.Album = album;
				}

				if (e.Data.Trim().ToLower().StartsWith("artist: ")) {
					string artist = e.Data.Trim().Substring("Artist: ".Length).Trim();
					mplayer_metadata.Artist = artist;
				}*/
			}
		}

		void TogglePause() {
			mplayer_paused = !mplayer_paused;
			try {
				mplayer_input.WriteLine("pause");
				mplayer_input.Flush();
			} catch {
			}
		}

		public void Play() {
			if (mplayer_paused == true) {
				TogglePause();
				refresherTimer.Start();
			}
		}

		public void Pause() {
			if (mplayer_paused == false) {
				refresherTimer.Stop();
				TogglePause();
			}

		}

		/*public AudioMetaData Metadata {
			get {
				return mplayer_metadata.Clone();
			}
		}*/

		/// <summary>
		/// Gets or sets the volume.
		/// </summary>
		/// <value>
		/// The volume, a float between 0 and 100.
		/// </value>
		public float Volume {
			get {
				return target_volume;
			}

			set {
				target_volume = value;
				setVolume(target_volume);
			}
		}

		void setVolume(float volume) {
			//Ask mplayer to change the volume
			try {
				mplayer_input.WriteLine("set_property volume " + volume.ToString());
				mplayer_input.Flush();
			} catch {
			}
		}

		public TimeSpan Length {
			get {
				return mplayer_length;
			}
		}

		public TimeSpan Position {
			get {
				return mplayer_position;
			}
		}

		public bool StillAlive {
			get {
				return still_alive || (!mplayer_process.HasExited);
			}
		}

		public static string getVersion() {
			try {
				ProcessStartInfo mplayer_version_info = new ProcessStartInfo("mplayer");
				mplayer_version_info.RedirectStandardInput = true;
				mplayer_version_info.RedirectStandardOutput = true;
				mplayer_version_info.UseShellExecute = false;
				Process mplayer_version_process = Process.Start(mplayer_version_info);
				return mplayer_version_process.StandardOutput.ReadLine();
			} catch {
				return "";
			}
		}
	}
}

