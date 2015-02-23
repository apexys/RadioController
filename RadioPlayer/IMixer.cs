using System;

namespace RadioPlayer
{
	public interface IMixer
	{
		ISoundObject createSound();
		void fadeTo(ISoundObject iso);
	}
}

