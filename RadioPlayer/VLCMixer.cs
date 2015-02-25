using System;
using RadioLibrary;
using System.Threading.Tasks;

namespace RadioPlayer
{
	public class VLCMixer : IMixer
	{
		VLCProcess[] vlcprocs;
		int nextProc;
		// TODO: remove
		public static VLCMixer mixer;

		public VLCMixer() {
			vlcprocs = new VLCProcess[2];
			vlcprocs[0] = new VLCProcess();
			vlcprocs[1] = new VLCProcess();
			nextProc = 0;
			// TODO: remove
			mixer = this;
		}

		#region IMixer implementation
		public ISoundObject createSound(MediaFile file) {
			RadioLogger.Logger.LogInformation("Created file for " + file.Name);
			VLCSoundObject vlcsnd = new VLCSoundObject(file, vlcprocs[nextProc]);
			nextProc ^= 1;//Toggle nextProc between zero and one
			return vlcsnd;
		}

		public VLCProcess[] getCurrentPlayers() {
			return vlcprocs;
		}

		public void fadeTo(ISoundObject iso, float targetVolume, float time) {
			int tvolume = Convert.ToInt32(targetVolume);
			int ttime = Convert.ToInt32(time) * 1000;
			int cvolume = Convert.ToInt32(iso.Volume);
			int delta = tvolume - cvolume;
			if (delta > 0) {
				int stepTime = 200;
				int stepsLeft = ttime / stepTime;
				int stepVolume = delta * stepTime / ttime;

				Task.Run(async () => {
					while (stepsLeft > 0) {
						iso.Volume += stepVolume;
						await Task.Delay(stepTime);
						stepsLeft --;
					}
					iso.Volume = targetVolume;
				});
			}
		}
		#endregion
	}
}

