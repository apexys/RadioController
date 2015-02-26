using RadioPlayer;
using System;
using System.Timers;

namespace RadioController
{
	public interface IController
	{
		void Start();
		void Stop();
		void Pause();
		void fadeOut();
		void fadeIn();
		void Skip();

		//void setPlayer(ISoundObject player);
		//void HandleClockEvent(object sender, ElapsedEventArgs e);
	}
}

