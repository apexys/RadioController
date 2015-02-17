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
		List<TriggeredCrossfadeTask> triggers;

		public Mixer ()
		{
			fades = new List<FaderTask>();
			triggers = new List<TriggeredCrossfadeTask>();

			clock = new Timer();
			clock.Interval = CLOCK_INTERVAL_MILISECONDS;
			clock.Elapsed += HandleClockEvent;
			clock.Start();
		}

		public void FadeTo (Mplayer player, float targetVolume, float seconds)
		{
			player.Play();

			FaderTask ft = new FaderTask();
			ft.player = player;
			ft.startVolume = player.Volume;
			ft.currentVolume = player.Volume;
			ft.endVolume = targetVolume;
			ft.seconds = seconds;
			ft.step = (ft.endVolume - ft.startVolume) / (ft.seconds * (1000f / ((float)CLOCK_INTERVAL_MILISECONDS)));
			fades.Add(ft);
		}

		public void TriggeredCrossfade(Mplayer player1, Mplayer player2, float seconds, float trigger_seconds){
			TriggeredCrossfadeTask tcf = new TriggeredCrossfadeTask();
			tcf.player1 = player1;
			tcf.player2 = player2;
			tcf.seconds_fade = seconds;
			tcf.trigger = TimeSpan.FromSeconds(trigger_seconds);
			triggers.Add(tcf);
		}


		void HandleClockEvent (object sender, ElapsedEventArgs e)
		{
			//Handle Fades
			List<FaderTask> fadesToRemove = new List<FaderTask> ();
		
			for (int i = 0; i < fades.Count; i++) {
				if (Math.Abs (fades [i].step) < Math.Abs (fades [i].endVolume - fades [i].currentVolume)) {
					fades [i].currentVolume += fades [i].step;
				} else {
					fades [i].currentVolume = fades [i].endVolume;
					fadesToRemove.Add (fades [i]);
				}
				fades [i].player.Volume = fades [i].currentVolume;
				Logger.LogDebug("Mixer fading: " + fades [i].currentVolume.ToString());

			}

			foreach (FaderTask finished in fadesToRemove) {
				fades.Remove (finished);
			}

/*			//Handle triggered crossfades

			List<TriggeredCrossfadeTask> triggersToRemove = new List<TriggeredCrossfadeTask> ();

			for (int i = 0; i < triggers.Count; i++) {
				Mplayer p1, p2;
				if (triggers [i].player1.Position > triggers [i].player2.Position) {
					p1 = triggers [i].player1;
					p2 = triggers [i].player2;
				} else {
					p2 = triggers [i].player1;
					p1 = triggers [i].player2;
				}

				if (p1.Length - p1.Position < triggers [i].trigger) {
					FadeTo (p1, 0, triggers [i].seconds_fade);
					FadeTo (p2, 100, triggers [i].seconds_fade);
					p2.Volume = 0;
					p2.Play();
					triggersToRemove.Add (triggers [i]);
				}
			
			}

			foreach (TriggeredCrossfadeTask trig in triggersToRemove) {
				triggers.Remove(trig);
			}*/
		}
	}

	class FaderTask{
		public Mplayer player;
		public float startVolume;
		public float endVolume;
		public float currentVolume;
		public float seconds;
		public float step;
	}

	class TriggeredCrossfadeTask{
		public Mplayer player1;
		public Mplayer player2;
		public float seconds_fade;
		public TimeSpan trigger;
	}

}

