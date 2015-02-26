using System;
using RadioLibrary;
using RadioPlayer;

namespace RadioController
{
	public class BasicController : AController
	{
		int sample = 0;

		public BasicController(IMixer mixer, ISoundObjectProvider sounds) : base(mixer, sounds) {
		}

		protected override bool startNextNow() {
			return sample > 0 && (getCurrentSound().Position.TotalSeconds > sample);
		}
	}
}

