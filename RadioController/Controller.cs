using System;
using System.Collections.Generic;
using System.Timers;
using System.IO;
using RadioPlayer;
using RadioLogger;
using RadioLibrary;

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

		~Controller() {
			Stop();
		}

		public void Dispose() {
			Stop();
		}

		string ControllerActionToString(ControllerAction action) {
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

		ISoundObject currentElement;

		ControllerAction currentState;
		ControllerAction lastState;

		MediaFolder songs;
		MediaFolder jingles;
		MediaFolder news;

		TimedTrigger jingleTrigger;
		TimedTrigger newsTrigger;
		Timer controller_timer;

		IMixer mixer;
		string liveURL;
		HTTPServer toggleServer;

		int demoLoop = -1;
		bool doFade = true;
		int sample = 0;

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

		public Controller(string songFolder, string jingleFolder, string newsFolder, TimedTrigger jingleTrigger, TimedTrigger newsTrigger, string liveURL) {
			//Basic state
			changeState("Initializing");

			mixer = new VLCMixer();

			controller_timer = new Timer(500);
			controller_timer.AutoReset = false;
			controller_timer.Elapsed += HandleClockEvent;

			this.songs = new MediaFolder(songFolder);
			this.jingles = new MediaFolder(jingleFolder);
			this.news = new MediaFolder(newsFolder);

			this.newsTrigger = newsTrigger;
			this.jingleTrigger = jingleTrigger;

			this.liveURL = liveURL;

			// TODO: Config for Port
			toggleServer = new HTTPServer(3124);

			changeState("Idle");
		}

		void changeState(string state) {
			controller_state = state;
			Logger.LogInformation("Controller changed state to: " + state);
		}

		public void Start() {
			currentState = ControllerAction.Idle;
			controller_timer.Start();
			if (currentElement != null) {
				currentElement.Playing = true;
				currentElement.Volume = 100f;
			}
			changeState("Running");
		}

		public void Stop() {
			controller_timer.Stop();
			changeState("Idle");
		}

		public void Pause() {
			if (currentElement != null) {
				currentElement.Playing = false;
			}
			changeState("Paused");
		}

		public void fadeOut() {
			if (currentElement != null) {
				// TODO: fix new tracks fading in, wjile fading out
				mixer.fadeTo (currentElement, 0f, SecondsSpentFading);
			}
		}

		public void fadeIn() {
			if (currentElement != null) {
				currentElement.Playing = true;
				mixer.fadeTo(currentElement, 100f, SecondsSpentFading);
			}
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
				currentElement.Playing = false;
			}
			Logger.LogInformation("Track Skipped");
		}

		void setMPlayer(ISoundObject player) {
			
			if (currentElement != null) {
				if (doFade) {
					if (lastState != ControllerAction.Jingle) {
						mixer.fadeTo(currentElement, 0f, SecondsSpentFading);
					}
				} 
			}

			currentElement = player;

			if (currentElement != null) {
				currentElement.Playing = true;
				if (doFade && currentState != ControllerAction.Jingle) {
					mixer.fadeTo(currentElement, 100f, SecondsSpentFading);
				} else {
					currentElement.Volume = 100f;
				}
			}
		}

		void HandleClockEvent(object sender, ElapsedEventArgs e) {
			ISoundObject newPlayer = null;

			// Do we want to go live
			if (toggleServer.State == true && currentState != ControllerAction.Live) {
				//Go Live
				islive = true;
				currentState = ControllerAction.Live;

				MediaFile streammf = new MediaFile (liveURL);

				setMPlayer(mixer.createSound(streammf));

			} else if (toggleServer.State == false && currentState == ControllerAction.Live) {
				//Go Dead
				islive = false;
				currentState = ControllerAction.Idle;
				
				setMPlayer(null);
			}

			newsTrigger.checkTriggers();
			jingleTrigger.checkTriggers();

			if (!islive) {
				if ((currentElement == null)
					|| (doFade && (currentElement.Duration.Subtract(currentElement.Position) <= TimeSpan.FromSeconds(SecondsSpentFading)))
					|| (sample > 0 && (currentElement.Position.Seconds > sample))) {

					lastState = currentState;

					//Chose next Action
					if (demoLoop < 0) {
						if (newsTrigger.PreviousTriggerChanged && currentState != ControllerAction.News) {
							currentState = ControllerAction.News;
						} else {
							if (jingleTrigger.PreviousTriggerChanged) {
								currentState = ControllerAction.Jingle;
							} else {
								currentState = ControllerAction.Song;
							}
						}
					} else {
						// a basic Demo Loop
						demoLoop++;
						if (demoLoop % 3 == 0) {
							if (demoLoop == 9) {
								currentState = ControllerAction.News;
								demoLoop = 0;
							} else {
								currentState = ControllerAction.Jingle;
							}
						} else {
							currentState = ControllerAction.Song;
						}
					}

					Logger.LogGood("Next action: " + ControllerActionToString(currentState));

					//Create new element
					switch (currentState) {
					case ControllerAction.Jingle:
						newPlayer = mixer.createSound(jingles.pickRandomFile());
						break;
					case ControllerAction.News:
						MediaFile nextNews = news.pickRandomFile();
						newPlayer = mixer.createSound(nextNews);
						break;
					case ControllerAction.Song:
						MediaFile mf = songs.pickRandomFile();
						Logger.LogNormal("Now playing: " + mf.MetaData.ToString());
						newPlayer = mixer.createSound(mf);
						break;
					case ControllerAction.Idle:
						Logger.LogError("Controller Action was Idle");
						break;
					}

					if (newPlayer != null) {
						setMPlayer(newPlayer);
					}
				}
			}
			controller_timer.Start();
		}
	}
}

