using System;
using System.Collections.Generic;
using System.Timers;

namespace RadioController
{
	public class Mixer
	{
		public static int CLOCK_INTERVAL_MILISECONDS = 250;

		Timer clock;
		List<FaderTask> fades;

		public Mixer() {
			fades = new List<FaderTask>();

			clock = new Timer();
			clock.AutoReset = false;
			clock.Interval = CLOCK_INTERVAL_MILISECONDS;
			clock.Elapsed += HandleClockEvent;
			clock.Start();
		}

		public void FadeTo(Mplayer player, float targetVolume, float seconds) {
			player.Play();
			player.Volume = 0f;
			FaderTask ft = new FaderTask();
			ft.player = player;
			ft.startVolume = player.Volume;
			ft.endVolume = targetVolume;
			ft.seconds = seconds;
			Logger.LogNormal("Fading "+ ft.startVolume.ToString() + " to " + targetVolume.ToString());
			ft.step = (ft.endVolume - ft.startVolume) / (ft.seconds * (1000f / ((float)CLOCK_INTERVAL_MILISECONDS)));
			fades.Add(ft);
		}

		void HandleClockEvent (object sender, ElapsedEventArgs e) {
			//Handle Fades
			//clock.Stop();
			List<FaderTask> fadesToRemove = new List<FaderTask>();

			for (int i = 0; i < fades.Count; i++) {
				if (Math.Abs(fades[i].step) < Math.Abs(fades[i].endVolume - fades[i].player.Volume)) {
					fades[i].player.Volume += fades[i].step;
					Logger.LogDebug("Mixer fading: " + fades[i].player.Volume.ToString());
				} else {
					fades[i].player.Volume = fades [i].endVolume;
					fadesToRemove.Add(fades[i]);
					Logger.LogDebug("Mixer fading Done: " + fades[i].player.Volume.ToString());
				}
			}

			foreach (FaderTask finished in fadesToRemove) {
				fades.Remove(finished);
			}
			clock.Start();
		}
	}

	class FaderTask {
		public Mplayer player;
		public float startVolume;
		public float endVolume;
		public float seconds;
		public float step;
	}
}

