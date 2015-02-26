using System;
using System.Collections.Generic;

namespace RadioController
{
	public class TimedTrigger
	{
		List<int> MinuteTriggers;
		int lastTrigger;
		int lastMinute;
		bool previousTriggerChanged;

		public TimedTrigger()
		{
			MinuteTriggers = new List<int>();
			lastTrigger = -1;
			lastMinute = 0;
			previousTriggerChanged = false;
		}

		public bool PreviousTriggerChanged {
			get {
				bool temp = previousTriggerChanged;
				previousTriggerChanged = false;
				return temp;
			}
		}

		public void addMinuteTrigger(int minute) {
			if (minute < 0 || minute > 59) {
				throw new ArgumentOutOfRangeException("minute has to be larger or equal to zero and lower than 60");
			} else {
				MinuteTriggers.Add(minute);
				MinuteTriggers.Sort();
			}
		}

		public void checkTriggers() {
			int minute = DateTime.Now.Minute;
			if (lastMinute != minute) {
				for (int i = 0; i < MinuteTriggers.Count; i++) {
					// TODO: redo this with a Queue and checking the whole timespan passed
					if (MinuteTriggers[i] == minute) {
						lastTrigger = MinuteTriggers[i];
						previousTriggerChanged = true;
						Console.WriteLine("Trigger");
						break;
					}
				}
				lastMinute = minute;
			}
		}

		public int getLastTrigger() {
			return lastTrigger;
		}
	}
}

