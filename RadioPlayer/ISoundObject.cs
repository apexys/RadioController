using System;
using RadioLibrary;

namespace RadioPlayer
{
	public interface ISoundObject
	{
		float Volume {get; set;}
		bool Playing {get; set;}
		TimeSpan Position {get; set;}
		TimeSpan Duration {get;}
		bool DuratiopnKnown {get;}
		bool Ended {get;}
		string Title {get;}
		EMediaType Type {get;}
	}
}

