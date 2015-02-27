using System;
using System.Threading;
using RadioLibrary;
using RadioLogger;

namespace RadioPlayer
{
	public class VLCSoundObject : ISoundObject
	{
		MediaFile mf;
		VLCProcess vlcp;
		bool playing;
		TimeSpan duration;
		bool durationKnown = false;
		float volume;

		public VLCSoundObject(MediaFile mf, VLCProcess vlcp) {
			this.mf = mf;
			this.vlcp = vlcp;
			vlcp.setFile(mf);
			playing = false;
			if (!durationKnown) {
				getDuration();
			}
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

		void getDuration() {
			int d = vlcp.getLength();
			if (d < 0) {
				throw new ArgumentException("Sound not loaded");
			}
			duration = TimeSpan.FromSeconds(Convert.ToDouble(d));
			if (duration.TotalSeconds > 0) {
				durationKnown = true;
			}
		}

		public TimeSpan Duration {
			get {
				if (!durationKnown) {
					try {
						getDuration();
					} catch (Exception ex) {
						Logger.LogException(ex);
					}
				}
				return duration;
			}
		}

		public bool DuratiopnKnown {
			get {
				if (!durationKnown) {
					try {
						getDuration();
					} catch (Exception ex) {
						Logger.LogException(ex);
					}
				}
				return durationKnown;
			}
		}

		public bool Ended {
			get {
				return vlcp.getPosition() == -1;
			}
		}

		public string Title {
			get {
				return mf.Name;
			}
		}

		public EMediaType Type {
			get {
				return mf.Type;
			}
		}
		#endregion
	}
}

