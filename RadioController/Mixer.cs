using System;
using System.Collections.Generic;
using System.Timers;
using System.Threading.Tasks;

namespace RadioController
{
	public class Mixer
	{
		public static int CLOCK_INTERVAL_MILISECONDS = 250;

		//Timer clock;
		List<FaderTask> fades;

		public Mixer() {
			fades = new List<FaderTask>();

			/*
			clock = new Timer();
			clock.AutoReset = false;
			clock.Interval = CLOCK_INTERVAL_MILISECONDS;
			clock.Elapsed += HandleClockEvent;
			clock.Start();
			*/
			Task.Run(() => {HandleClockEvent();});
		}

		public void FadeTo(Mplayer player, float targetVolume, float seconds) {
			player.Play();

			FaderTask ft = new FaderTask();
			ft.player    = player;
			ft.endAction = null;
			ft.endVolume = targetVolume;
			ft.stepsLeft = (int) ((seconds * 1000f) / (float) CLOCK_INTERVAL_MILISECONDS);
			ft.step      = ((ft.endVolume - player.Volume) * CLOCK_INTERVAL_MILISECONDS) / (seconds * 1000f);
			fades.Add(ft);

			Logger.LogNormal("Fading "+ player.Volume.ToString() + " to " + targetVolume.ToString());
		}

		public void FadeTo(Mplayer player, float targetVolume, float seconds, Action<Mplayer> endAction) {
			player.Play();

			FaderTask ft = new FaderTask();
			ft.player    = player;
			ft.endAction = endAction;
			ft.endVolume = targetVolume;
			ft.stepsLeft = (int) ((seconds * 1000f) / (float) CLOCK_INTERVAL_MILISECONDS);
			ft.step      = ((ft.endVolume - player.Volume) * CLOCK_INTERVAL_MILISECONDS) / (seconds * 1000f);
			fades.Add(ft);

			Logger.LogNormal("Fading "+ player.Volume.ToString() + " to " + targetVolume.ToString());
		}

		//void HandleClockEvent (object sender, ElapsedEventArgs e) {
		async void HandleClockEvent () {
			//Handle Fades
			//clock.Stop();

			while (true) {
				List<FaderTask> fadesToRemove = new List<FaderTask>();

				for (int i = 0; i < fades.Count; i++) {
					if (fades[i].stepsLeft <= 1) {
						// last step to fade
						fades[i].player.Volume = fades[i].endVolume;

						fadesToRemove.Add(fades[i]);
						Logger.LogDebug("Mixer fading Done: " + fades[i].endVolume.ToString());

						if (fades[i].endAction != null) {
							fades[i].endAction(fades[i].player);
						}
					} else {
						// fade one step
						fades [i].player.Volume += fades [i].step;
						Logger.LogDebug ("Mixer fading: " + fades [i].player.Volume.ToString ());
					}

					fades [i].stepsLeft--;
				}

				foreach (FaderTask finished in fadesToRemove) {
					fades.Remove (finished);
				}
				await Task.Delay (CLOCK_INTERVAL_MILISECONDS);
			}
			//clock.Start();
		}
	}

	class FaderTask {
		public Mplayer player;
		public int stepsLeft;
		public float endVolume;
		public float step;
		public Action<Mplayer> endAction;
	}
}

