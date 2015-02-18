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

		public enum ControllerAction {
			Idle,
			Song,
			Jingle,
			News,
			Live
		}

		~Controller() {
			Stop();
		}

		public void Dispose() {
			Stop();
		}

		string ControllerActionToString (ControllerAction action) {
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

		MediaFolder songs;
		MediaFolder jingles;
		MediaFolder news;

		TimedTrigger jingleTrigger;
		TimedTrigger newsTrigger;

		Timer controller_timer;

		Mixer mixer;

		string liveURL;
		HTTPServer toggleServer;
		
		bool doFade = true;
		int sample = 12;

		/// <summary>
		/// Switches between Fading between elements or simply cutting over
		/// </summary>
		/// <value><c>true</c> if it should fade; otherwise, <c>false</c>.</value>
		public bool DoFade {
			get {
				return doFade;
			}
			set {
				doFade = value;
			}
		}

		/// <summary>
		/// If set to a value than zero the controller goes into debug mode and switches over to the next element after the specified delay in seconds
		/// </summary>
		/// <value>Time to fade after or zero</value>
		public int Sample {
			get {
				return sample;
			}
			set {
				sample = value;
			}
		}


		public Controller (string songFolder, string jingleFolder, string newsFolder, TimedTrigger jingleTrigger, TimedTrigger newsTrigger, string liveURL) {
			//Basic state
			changeState("Initializing");

			mixer = new Mixer();

			controller_timer = new Timer(500);
			controller_timer.AutoReset = false;
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
			controller_timer.Start();
			if (currentElement != null) {
				currentElement.Play();
			}
			changeState("Running");
		}

		public void Stop() {
			controller_timer.Stop();
			if (currentElement != null) {
				currentElement.Dispose();
				currentElement = null;
			}
			changeState("Idle");
		}

		public void Pause() {
			if (currentElement != null) {
				currentElement.Pause();
			}
			changeState("Paused");
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
			Logger.LogInformation("Track Skipped");
		}

		void HandleClockEvent(object sender, ElapsedEventArgs e) {
			if (toggleServer.State == true && currentState != ControllerAction.Live) {
				//Go Live
				islive = true;
				currentState = ControllerAction.Live;
	
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
			}

			newsTrigger.checkTriggers();
			jingleTrigger.checkTriggers();

			if (!islive) {
				if (	(currentElement == null)
				    	|| (!currentElement.StillAlive)
				    	|| (doFade && (currentElement.Length.Subtract(currentElement.Position) <= TimeSpan.FromSeconds(SecondsSpentFading)))
				    	|| (sample > 0 && (currentElement.Position.Seconds > sample)) ) {

					if (currentElement == null) {
						Logger.LogNormal("Currentelement was NULL");
					} else
					if (!currentElement.StillAlive) {
						Logger.LogNormal("Currentelement died");
					} else
					if (doFade && currentElement.Length.Subtract (currentElement.Position) <= TimeSpan.FromSeconds (SecondsSpentFading)) {
						Logger.LogNormal("Start Fading");
					} else
					if (sample > 0 && currentElement.Position.Seconds > sample) {
						Logger.LogNormal("Sample Timeout");
					}


					//Chose next Action
					if (newsTrigger.PreviousTriggerChanged && currentState != ControllerAction.News) {
						currentState = ControllerAction.News;
					} else {
						if (jingleTrigger.PreviousTriggerChanged) {
							currentState = ControllerAction.Jingle;
						} else {
							currentState = ControllerAction.Song;
						}
					}
					Logger.LogGood("Next action: " + ControllerActionToString(currentState));

					//Switch focus, fade down last element, fade up next element
					if (lastElement != null) {
						lastElement.Dispose();
					}

					// select current Element to fade out
					lastElement = currentElement;
					if (lastElement != null) {
						if (doFade) {
							mixer.FadeTo (lastElement, 0f, SecondsSpentFading);
						} else {
							lastElement.Volume = 0f;
							lastElement.Dispose();
							lastElement = null;
						}
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
						MediaFile mf = songs.pickRandomFile ();
						Logger.LogNormal ("Now playing: " + mf.MetaData.ToString ());
						currentElement = new Mplayer(mf.Path);
						break;
					case ControllerAction.Idle:
						Logger.LogError("Controller Action was Idle");
						break;
					}

					//Fading in
					currentElement.Play();
					if (doFade) {
						mixer.FadeTo(currentElement, 100f, SecondsSpentFading);
					} else {
						currentElement.Volume = 100f;
					}
				}
			}
			controller_timer.Start ();
		}
	}
}

