using System;
using System.Collections.Generic;
using System.Timers;
using System.IO;

namespace RadioController
{
	public class Controller
	{
		// TODO: Config for Fading time
		const int SecondsSpentFading = 3;
		bool islive;

		/*
		bool isLive {
			get {
				return islive;
			}
			set {
				islive = value;
			}
		}
		*/

		public enum ControllerAction
		{
			Idle,
			Song,
			Jingle,
			News,
			Live
		}

		string ControllerActionToString (ControllerAction action)
		{
			switch (action) {
			case ControllerAction.Idle:
				return "Idle";
			case ControllerAction.Jingle:
				return "Jingle";
			case ControllerAction.Live:
				return "Live";
			case ControllerAction.News:
				return "News";
			case ControllerAction.Song:
				return "Song";
			default:
				return "Unknown";
			}
		}

		string controller_state;

		public string ControllerState {
			get {
				return controller_state;
			}
		}

		Mplayer currentElement;
		Mplayer lastElement;

		ControllerAction currentState;
		ControllerAction nextState;

		MediaFolder songs;
		MediaFolder jingles;
		MediaFolder news;

		TimedTrigger jingleTrigger;
		TimedTrigger newsTrigger;

		Timer controller_timer;

		Mixer mixer;

		string liveURL;
		HTTPServer toggleServer;

		public Controller (string songFolder, string jingleFolder, string newsFolder, TimedTrigger jingleTrigger, TimedTrigger newsTrigger, string liveURL) {
			//Basic state
			changeState("Initializing");

			mixer = new Mixer();

			controller_timer = new Timer(500);
			controller_timer.Elapsed += HandleClockEvent;

			this.songs   = new MediaFolder(songFolder);
			this.jingles = new MediaFolder(jingleFolder);
			this.news    = new MediaFolder(newsFolder);

			this.newsTrigger   = newsTrigger;
			this.jingleTrigger = jingleTrigger;

			this.liveURL = liveURL;

			// TODO: Config for Port
			toggleServer = new HTTPServer(3124);

			changeState("Idle");
		}

		void changeState (string state) {
			controller_state = state;
			Logger.LogInformation("Controller changed state to: " + state);
		}

		public void Start() {
			currentState = ControllerAction.Idle;
			nextState = ControllerAction.Idle;
			controller_timer.Start();
			changeState("Running");
		}

		public void Stop() {
			controller_timer.Stop();
			changeState("Idle");
		}

		public void Rescan() {
			songs.Refresh();
			jingles.Refresh();
			news.Refresh();
		}

		public void Skip() {
			currentState = ControllerAction.Idle;
			// stop the current element
			if (currentElement != null) {
				currentElement.Pause();
				currentElement.Dispose();
				currentElement = null;
			}
		}

		void HandleClockEvent(object sender, ElapsedEventArgs e) {
			if (toggleServer.State == true && currentState != ControllerAction.Live) {
				//Go Live
				islive = true;
				currentState = ControllerAction.Live;
				nextState = ControllerAction.Live;

				lastElement = currentElement;
				currentElement = new Mplayer(liveURL);
				//Fading
				currentElement.Play();
				if (lastElement != null) {
					mixer.FadeTo(lastElement, 0, SecondsSpentFading);
				}
				mixer.FadeTo(currentElement, 100, SecondsSpentFading);
			} else if (toggleServer.State == false && currentState == ControllerAction.Live) {
				//Go Dead
				islive = false;
				currentState = ControllerAction.Idle;
				nextState = ControllerAction.Idle;
			}

			newsTrigger.checkTriggers();
			jingleTrigger.checkTriggers();

			if (nextState == ControllerAction.Idle) {
				//Chose next Action
				if (newsTrigger.PreviousTriggerChanged && currentState != ControllerAction.News) {
					nextState = ControllerAction.News;
				} else {
					if (jingleTrigger.PreviousTriggerChanged) {
						nextState = ControllerAction.Jingle;
					} else {
						nextState = ControllerAction.Song;
					}
				}
				Logger.LogGood("Next action: " + ControllerActionToString(nextState));
			}

			if (currentState == ControllerAction.Idle) {
				currentState = nextState;
				nextState = ControllerAction.Idle;
				Logger.LogGood("Current action: " + ControllerActionToString(currentState));
			}

			if (!islive) {
				if (currentElement == null
				    	|| (!currentElement.StillAlive)
				    	|| currentElement.Length.Subtract(currentElement.Position) <= TimeSpan.FromSeconds(SecondsSpentFading)) {

					//Switch focus, fade down last element, fade up next element
					if (lastElement != null) {
						lastElement.Dispose();
					}

					// select current Element to fade out
					lastElement = currentElement;
					if (lastElement != null) {
						mixer.FadeTo(lastElement, 0, SecondsSpentFading);
					}

					Logger.LogNormal("now: Fading");

					//Create new element
					switch (currentState) {
					case ControllerAction.Jingle:
						currentElement = new Mplayer(jingles.pickRandomFile().Path);
						break;
					case ControllerAction.News:
						MediaFile nextNews = news.pickRandomFile();
						currentElement = new Mplayer(nextNews.Path);
						break;
					case ControllerAction.Song:
						currentElement = new Mplayer(songs.pickRandomFile().Path);
						break;
					case ControllerAction.Idle:
						Logger.LogError("Controller Action was Idle");
						break;
					}

					//Fading in
					currentElement.Play();
					mixer.FadeTo(currentElement, 100, SecondsSpentFading);

				}
			}
		}
	}
}

