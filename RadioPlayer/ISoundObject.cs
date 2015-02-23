using System;

namespace RadioPlayer
{
	public interface ISoundObject
	{
		float Volume {get; set;}
		bool Playing {get; set;}
		TimeSpan Position {get; set;}
		TimeSpan Duration {get;}
		string Title {get;}
	}
}

