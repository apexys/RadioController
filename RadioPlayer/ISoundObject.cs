using System;

namespace RadioPlayer
{
	public interface ISoundObject
	{
		float Volume {get; set;}
		bool Playing {get; set;}
		float Position {get; set;}
		float Duration {get;}
		string Title {get;}
	}
}

