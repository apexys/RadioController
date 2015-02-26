using System;
using System.Threading;
using RadioLibrary;

namespace RadioPlayer
{
	public class VLCSoundObject : ISoundObject
	{
		MediaFile mf;
		VLCProcess vlcp;
		bool playing;
		TimeSpan duration;
		float volume;

		public VLCSoundObject(MediaFile mf, VLCProcess vlcp) {
			int i, d = 0;
			this.mf = mf;
			this.vlcp = vlcp;
			vlcp.setFile(mf);
			playing = false;
			for (i=0; i<50; i++) {
				d = vlcp.getLength();
				if (d > 0) {
					break;
				}
				if (d < 0) {
					throw new ArgumentException("Sound not loaded");
				}
				Thread.Sleep(20);
			}
			duration = TimeSpan.FromSeconds(Convert.ToDouble(d));
			volume = Convert.ToSingle(vlcp.getVolume());
		}
		#region ISoundObject implementation
		public float Volume {
			get {
				return volume;
			}
			set {
				if (value != volume) {
					volume = value;
					vlcp.setVolume(Convert.ToInt32(value));
				}
			}
		}

		public bool Playing {
			get {
				return playing;
			}
			set {
				RadioLogger.Logger.LogGood("Playing = " + value.ToString());
				if (value != playing) {
					if (value) {
						vlcp.play();
						playing = true;
					} else {
						vlcp.pause();
						playing = false;
					}
				}
			}
		}

		public TimeSpan Position {
			get {
				return TimeSpan.FromSeconds(Convert.ToDouble(vlcp.getPosition()));
			}
			set {
				vlcp.setPosition(Convert.ToInt32(value.TotalSeconds));
			}
		}

		public TimeSpan Duration {
			get {
				return duration;
			}
		}

		public string Title {
			get {
				return mf.Name;
			}
		}
		#endregion
	}
}

