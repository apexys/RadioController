using System;

namespace RadioController
{
	public interface IPlayerConnection
	{
		float Volume {get; set;}
		bool Playing {get; set;}
		float Position {get; set;}
		float Duration {get;}
		string Title {get;}
	}
}

