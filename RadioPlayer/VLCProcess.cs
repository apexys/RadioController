using System;
using System.Diagnostics;
using System.IO;
using Configuration;
using RadioLogger;
using RadioLibrary;
using System.Threading;

namespace RadioPlayer
{
	public class VLCProcess
	{
		Process process;
		StreamReader sout;
		StreamWriter sin;

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

			process.Start();

			sin = process.StandardInput;
			sin.AutoFlush = true;

			sout = process.StandardOutput;

			// this call can throw an error

			String version = sout.ReadLine();
			string dump = sout.ReadLine ();

			Logger.LogInformation ("VLC version " + version);
		}

		public void setFile(MediaFile mf){
			sin.WriteLine ("clear");
			sin.WriteLine ("enqueue " + mf.Path); 
			sin.WriteLine ("goto 0");//GOTO considered harmful :P
			sin.WriteLine ("pause");
		}

		public void play(){
			sin.WriteLine ("play");
		}

		public void pause(){
			sin.WriteLine ("pause");
		}

		public int getPosition(){
			sin.WriteLine ("get_time");
			Thread.Sleep (30);
			debugStream (sout);
			return Convert.ToInt32 (sout.ReadLine ().Trim());
		}

		public int getLength(){
			sin.WriteLine ("get_length");
			Thread.Sleep (30);
			debugStream (sout);
			string s = sout.ReadLine ().Trim();
			return Convert.ToInt32 (s);
		}

		public void setVolume(int volume){
			sin.WriteLine ("volume " + volume.ToString ());
		}

		public int getVolume(){
			sout.DiscardBufferedData ();
			sin.WriteLine ("volume");
			Thread.Sleep (30);
			return Convert.ToInt32 (sout.ReadLine ().Trim ());
		}

		void debugStream(StreamReader s){
			s.Peek ();
		}



	}
}

