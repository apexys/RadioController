using System;
using System.Diagnostics;
using System.IO;
using System.Timers;

namespace RadioController
{
	public class Mplayer
	{
		Process mplayer_process;
		StreamWriter mplayer_input;
		float mplayer_volume = 0f;
		Timer refresherTimer;

		bool mplayer_paused = false;

		bool events_running = false;

		bool still_alive = false;
		int ticks_since_last_message = 0;
		const int ticks_to_death = 8;

		TimeSpan mplayer_position;
		TimeSpan mplayer_length;

		AudioMetaData mplayer_metadata;

		~Mplayer(){
			refresherTimer.Stop();
			try{
				mplayer_input.WriteLine("stop");
				mplayer_process.Kill ();
			}catch{
				;//Nothing to do here, sometimes it has already collected the garbage and sometimes not
			}
		}

		public void Dispose(){
			refresherTimer.Stop();
			try{
				mplayer_input.WriteLine("stop");
				mplayer_process.Kill ();
			}catch{
				;//Nothing to do here, sometimes it has already collected the garbage and sometimes not
			}
		}

		public Mplayer (string path)
		{
			string arguments = "-slave -quiet " + "\"" + path + "\"";
			Console.WriteLine ("CMD: mplayer " + arguments);

			mplayer_process = new Process ();
			mplayer_process.StartInfo.FileName = "mplayer";
			mplayer_process.StartInfo.Arguments = arguments;//"/home/apexys/Music/test.mp3";
			mplayer_process.StartInfo.UseShellExecute = false;
			mplayer_process.StartInfo.RedirectStandardInput = true;
			mplayer_process.StartInfo.RedirectStandardOutput = true;
			mplayer_process.StartInfo.RedirectStandardError = true;
			mplayer_process.StartInfo.CreateNoWindow = true;
			mplayer_process.OutputDataReceived += HandleOutputDataReceived;
			mplayer_process.ErrorDataReceived += HandleErrorDataReceived;
			mplayer_process.EnableRaisingEvents = true;
			mplayer_process.Start ();

			mplayer_process.BeginErrorReadLine ();
			mplayer_process.BeginOutputReadLine ();
			mplayer_input = mplayer_process.StandardInput;
			mplayer_input.AutoFlush = true;

			refresherTimer = new Timer (250);
			refresherTimer.Elapsed += HandleRefresherTimerElapsed;
			refresherTimer.Start ();

			mplayer_position = new TimeSpan (0, 0, 0);
			mplayer_length = new TimeSpan (0, 0, 0);

			mplayer_metadata = new AudioMetaData();

			refreshValues ();
			Pause ();

			still_alive = true;

			while (! events_running) {
				System.Threading.Thread.Sleep(100);
			}
		}

		void HandleRefresherTimerElapsed (object sender, ElapsedEventArgs e)
		{
			if (! mplayer_paused) {
				refreshValues();
				ticks_since_last_message ++;
				if (ticks_since_last_message > ticks_to_death) {
					still_alive = false;
					Logger.LogError ("Mplayer died!");
				}
			}
		}

		void refreshValues(){
			mplayer_input.WriteLine("get_property volume");
			mplayer_input.WriteLine("get_time_pos");
			mplayer_input.WriteLine("get_time_length");
		}

		void HandleErrorDataReceived (object sender, DataReceivedEventArgs e)
		{
			if (e.Data.Trim () != "") {
				Logger.LogError (e.Data);
			}
		}

		void HandleOutputDataReceived (object sender, DataReceivedEventArgs e)
		{
			if (e.Data != null) {
				events_running = true;
				ticks_since_last_message = 0;
				if (e.Data.StartsWith ("ANS_volume=")) {
					mplayer_volume = float.Parse (e.Data.Substring ("ANS_volume=".Length).Trim ());
				}

				if (e.Data.StartsWith ("ANS_TIME_POSITION=")) {
					float seconds_position = float.Parse (e.Data.Substring ("ANS_TIME_POSITION=".Length).Trim ());
					mplayer_position = TimeSpan.FromSeconds (Convert.ToDouble (seconds_position));
				}

				if (e.Data.StartsWith ("ANS_LENGTH=")) {
					float seconds_length = float.Parse (e.Data.Substring ("ANS_LENGTH=".Length).Trim ());
					mplayer_length = TimeSpan.FromSeconds (Convert.ToDouble (seconds_length));
				}

				if(e.Data.Trim().ToLower().StartsWith("title: ")){
					string title = e.Data.Trim().Substring("Title: ".Length).Trim();
					mplayer_metadata.Title = title;
				}

				if(e.Data.Trim().ToLower().StartsWith("album: ")){
					string album = e.Data.Trim().Substring("Album: ".Length).Trim();
					mplayer_metadata.Album = album;
				}

				if(e.Data.Trim().ToLower().StartsWith("artist: ")){
					string artist = e.Data.Trim().Substring("Artist: ".Length).Trim();
					mplayer_metadata.Artist = artist;
				}

			}

			/*
			Console.ForegroundColor = ConsoleColor.Cyan;
			Console.WriteLine (e.Data + "|||");
			Console.ResetColor ();*/


		}

		public void TogglePause(){
			mplayer_paused = !mplayer_paused;
			mplayer_input.WriteLine("pause");
			mplayer_input.Flush();
		}

		public void Play ()
		{

			if (mplayer_paused == true) {
				mplayer_input.WriteLine("pause");
				mplayer_paused = false;
				refresherTimer.Start();
			}
		}

		public void Pause()
		{
			if (mplayer_paused == false) {
				refresherTimer.Stop();
				mplayer_input.WriteLine("pause");
				mplayer_paused = true;
			}

		}

		public AudioMetaData Metadata {
			get {
				return mplayer_metadata.Clone();
			}
		}

		/// <summary>
		/// Gets or sets the volume.
		/// </summary>
		/// <value>
		/// The volume, a float between 0 and 100.
		/// </value>
		public float Volume {
			get {
				return mplayer_volume;
			}

			set {
				//Ask mplayer to change the volume
				mplayer_input.WriteLine("set_property volume " + value);
				mplayer_input.Flush();
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

		public bool StillAlive{
			get{
				return still_alive || (!mplayer_process.HasExited);
			}
		}

		public static string getVersion()
		{
			try {
				ProcessStartInfo mplayer_version_info = new ProcessStartInfo ("mplayer");
				mplayer_version_info.RedirectStandardInput = true;
				mplayer_version_info.RedirectStandardOutput = true;
				mplayer_version_info.UseShellExecute = false;
				Process mplayer_version_process = Process.Start (mplayer_version_info);
				return mplayer_version_process.StandardOutput.ReadLine ();
			} catch {
				return "";
			}
		}
	}
}

