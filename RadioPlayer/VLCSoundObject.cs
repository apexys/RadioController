using System;
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
		public VLCSoundObject (MediaFile mf, VLCProcess vlcp)
		{
			this.mf = mf;
			this.vlcp = vlcp;
			vlcp.setFile (mf);
			playing = false;
			duration = TimeSpan.FromSeconds(Convert.ToDouble(vlcp.getLength()));
			volume = Convert.ToSingle (vlcp.getVolume ());
		}

		#region ISoundObject implementation

		public float Volume {
			get {
				return volume;
			}
			set {
				if (value != volume) {
					volume = value;
					vlcp.setVolume (Convert.ToInt32 (value));
				}
			}
		}

		public bool Playing {
			get {
				return playing;
			}
			set {
				if (value != playing) {
					if (value) {
						vlcp.play ();
						playing = true;
					} else {
						vlcp.pause ();
						playing = false;
					}
				}
			}
		}

		public TimeSpan Position {
			get {
				return TimeSpan.FromSeconds(Convert.ToDouble (vlcp.getPosition ()));
			}
			set {
				throw new NotImplementedException ();
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

