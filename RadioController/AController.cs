using System;
using RadioLibrary;
using RadioPlayer;
using RadioLogger;
using System.Timers;
using Configuration;

namespace RadioController
{
	public abstract class AController : IDisposable, IController
	{

		protected IMixer mixer;
		ISoundObjectProvider soundProvider;
		ISoundObject currentSO;
		Timer controller_timer;
		TimeSpan lastPosition;
		int lastTimeEqualCounter;
		int maxLastTimeEqualCounter;
		int SecondsSpentFading;

		~AController() {
			Stop();
		}

		public void Dispose() {
			Stop();
		}

		public AController(IMixer mixer, ISoundObjectProvider soundProvider) {
			this.mixer = mixer;
			this.soundProvider = soundProvider;

			if (mixer == null || soundProvider == null) {
				throw new ArgumentNullException();
			}

			SecondsSpentFading = Settings.getInt("mixer.fadetime", 3);
			maxLastTimeEqualCounter = Settings.getInt("controller.positionTimeout", 6);

			controller_timer = new Timer(Settings.getInt("controller.tickrate", 500));
			controller_timer.AutoReset = false;
			controller_timer.Elapsed += HandleClockEvent;
		}

		public void Start() {
			controller_timer.Start();
			if (currentSO != null) {
				currentSO.Volume = 100;
				currentSO.Playing = true;
			}
		}

		public void Stop() {
			controller_timer.Stop();
			if (currentSO != null) {
				currentSO.Playing = false;
			}
		}

		public void Pause() {
			controller_timer.Stop();
			if (currentSO != null) {
				currentSO.Playing = false;
			}
		}

		public void fadeOut() {
			controller_timer.Start();
			if (currentSO != null) {
				mixer.fadeTo(currentSO, 0, SecondsSpentFading);
				System.Threading.Tasks.Task.Run(async () => {
					await System.Threading.Tasks.Task.Delay(SecondsSpentFading * 1000);
					Stop();
				});
			}
		}

		public void fadeIn() {
			controller_timer.Start();
			if (currentSO != null) {
				currentSO.Volume = 0;
				currentSO.Playing = true;
				mixer.fadeTo(currentSO, 100, SecondsSpentFading);
			}
		}

		public void Skip() {
			setSound(soundProvider.nextSound());
		}

		protected ISoundObject getCurrentSound() {
			return currentSO;
		}

		protected void setSound(ISoundObject sound) {
			if (currentSO != null ) {
				ISoundObject tmp = currentSO;
				if (currentSO.Type == EMediaType.FullVolume) {
					System.Threading.Tasks.Task.Run(async () => {
						await System.Threading.Tasks.Task.Delay(SecondsSpentFading * 1000);
						tmp.Playing = false;
					});
				} else {
					mixer.fadeTo(currentSO, 0f, SecondsSpentFading);
				}
			}

			currentSO = sound;
			lastTimeEqualCounter = 0;
			lastPosition = TimeSpan.FromSeconds(0);

			if (currentSO != null) {
				if (currentSO.Type == EMediaType.FullVolume) {
					currentSO.Volume = 100;
					currentSO.Playing = true;
				} else {
					currentSO.Volume = 0;
					currentSO.Playing = true;
					mixer.fadeTo(currentSO, 100f, SecondsSpentFading);
				}
			}
		}

		void HandleClockEvent(object sender, ElapsedEventArgs e) {
			TimeSpan position = TimeSpan.FromSeconds(0);
			// Just to be sure the Stream doesn't get killed by this
			try {
				// check if we want to start the next sound
				if (currentSO != null) {
					position = currentSO.Position;
					if (position > lastPosition) {
						lastTimeEqualCounter = 0;
						lastPosition = position;
					} else {
						lastTimeEqualCounter++;
					}
				}
				
				if ((currentSO == null)
				    || currentSO.Ended
					|| (maxLastTimeEqualCounter > 0 && lastTimeEqualCounter >= maxLastTimeEqualCounter)
					|| (currentSO.DuratiopnKnown && (currentSO.Duration.Subtract(position) <= TimeSpan.FromSeconds(SecondsSpentFading)))
					|| startNextNow()) {

					setSound(soundProvider.nextSound());
				}
			} catch (Exception ex) {
				Logger.LogException(ex);
			}
			controller_timer.Start();

		}

		protected abstract bool startNextNow();
	}
}

