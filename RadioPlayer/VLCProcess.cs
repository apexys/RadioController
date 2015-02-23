using System;
using System.Diagnostics;
using System.IO;
using Configuration;

namespace RadioPlayer
{
	public class VLCProcess : ISoundObject
	{
		Process process;
		StreamReader sout;
		StreamWriter sin;

		TimeSpan position;
		TimeSpan length;

		AudioMetaData metadata;

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

			// this call can throw an error
			process.Start();

			sin = process.StandardInput;
			sin.AutoFlush = true;

			sout = process.StandardOutput;

			position = new TimeSpan(0, 0, 0);
			length = new TimeSpan(0, 0, 0);

			metadata = new AudioMetaData();


		}
	}
}

