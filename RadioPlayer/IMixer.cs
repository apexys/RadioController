using System;
using RadioLibrary;

namespace RadioPlayer
{
	public interface IMixer
	{
		ISoundObject createSound(MediaFile file);
		void fadeTo(ISoundObject iso, float targetVolume, float time);
	}
}

