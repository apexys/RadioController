using System;

namespace RadioPlayer
{
	public interface ISoundObjectProvider
	{
		ISoundObject nextSound();
		bool interject();
	}
}

