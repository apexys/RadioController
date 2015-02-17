using System;

namespace RadioController
{
	public class Globals
	{
		static Random R;
		public static Random Random{
			get{
				if(R == null){
					R = new Random(DateTime.Now.Millisecond); 
				}
				return R;
			}
		}

		public static string escapeSpacesInPath(string path){
			return path.Replace(" ","\\ ").Replace("\\\\","\\");
		}

		public const float FADE_TIME = 5;
		public const float VOLUME_LOW = 0f;
		public const float VOLUME_HIGH = 100f;


		static Mixer mixer;
		public static Mixer Mixer {
			get {
				if (mixer == null) {
					mixer = new Mixer();
				}
				return mixer;
			}
		}
	}
}

