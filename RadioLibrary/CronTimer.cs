using System;

namespace RadioLibrary
{
	public class CronTimer
	{
		string minute;
		string hour;
		string day;
		string month;
		string weekDay;
		DateTime nextAction;

		public CronTimer(string input) {
			string[] para = System.Text.RegularExpressions.Regex.Split(input, @"\s+");
			if (para.Length < 5) {
				throw new InvalidOperationException("Configurtion does not contain 5 Parameters for Cron: " + input);
			}
			minute = para[0];
			hour = para[1];
			day = para[2];
			month = para[3];
			weekDay = para[4];

			nextAction = DateTime.Now.AddMinutes(1);
			calcNextActionTime();
		}

		public CronTimer(string minute, string hour, string day, string month, string weekDay) {
			this.minute = minute;
			this.hour = hour;
			this.day = day;
			this.month = month;
			this.weekDay = weekDay;
			
			nextAction = DateTime.Now.AddMinutes(1);
			calcNextActionTime();
		}

		void calcNextActionTime() {
			bool reset = false;
			bool found;

			do {

				// Month
				for(int i = 0; i<11; i++) {
					if (matchesNumber(month, nextAction.Month)) {
						break;
					}
					if (reset) {
						nextAction = nextAction.AddMonths(1);
					} else {
						nextAction = new DateTime(nextAction.Year, nextAction.Month, 1)
							.AddMonths(1);
						reset = true;
					}
				}

				// Day
				for(int i = 0; i<30; i++) {
					if (matchesNumber(day, nextAction.Day)) {
						break;
					}
					if (reset) {
						nextAction = nextAction.AddDays(1);
					} else {
						nextAction = new DateTime(nextAction.Year, nextAction.Month, nextAction.Day)
							.AddDays(1);
						reset = true;
					}
				}

				// Hour
				for(int i = 0; i<59; i++) {
					if (matchesNumber(hour, nextAction.Hour)) {
						break;
					}
					if (reset) {
						nextAction = nextAction.AddHours(1);
					} else {
						nextAction = new DateTime(nextAction.Year, nextAction.Month, nextAction.Day, nextAction.Hour, 0, 0)
							.AddHours(1);
						reset = true;
					}
				}

				// Minutes
				for(int i = 0; i<59; i++) {
					if (matchesNumber(minute, nextAction.Minute)) {
						break;
					}
					if (reset) {
						nextAction = nextAction.AddMinutes(1);
					} else {
						nextAction = new DateTime(nextAction.Year, nextAction.Month, nextAction.Day, nextAction.Hour, nextAction.Minute, 0)
							.AddMinutes(1);
						reset = true;
					}
				}

				found = matchesWeekDay(weekDay, nextAction.DayOfWeek);
				if(!found) {
					nextAction = new DateTime(nextAction.Year, nextAction.Month, nextAction.Day)
						.AddDays(1);
				} else {
					found  = matchesNumber(month, nextAction.Month)
							&& matchesNumber(day, nextAction.Day)
							&& matchesNumber(hour, nextAction.Hour)
							&& matchesNumber(minute, nextAction.Minute);
				}
				reset = false;
			} while (!found);

		}

		bool matchesWeekDay(string reference, DayOfWeek dayOfWeek) {
			int day1, day2, day;
			if ("" == reference) {
				throw new InvalidOperationException("Empty config is not allowed for Cron configuration");
			}
			if ("*" == reference) {
				return true;
			}

			switch (dayOfWeek) {
			default:
			case DayOfWeek.Sunday:
				day = 0;
				break;
			case DayOfWeek.Monday:
				day = 1;
				break;
			case DayOfWeek.Tuesday:
				day = 2;
				break;
			case DayOfWeek.Wednesday:
				day = 3;
				break;
			case DayOfWeek.Thursday:
				day = 4;
				break;
			case DayOfWeek.Friday:
				day = 5;
				break;
			case DayOfWeek.Saturday:
				day = 6;
				break;
			}

			string[] split = reference.Split(',');
			foreach (string part in split) {
				string[] range = part.Split('-');
				switch (range.Length) {
				case 2:
					day1 = toDayOfWeek(range[0]);
					day2 = toDayOfWeek(range[1]);
					if (day2 == 0) {
						day2 = 7;
					}
					if (day1 <= day && day <= day2) {
						return true;
					}
					break;
				case 1:
					if (toDayOfWeek(range[0]) == day) {
						return true;
					}
					break;
				default:
					throw new InvalidCastException("\"" + split + "\" is not a range");
				}
			}
			return false;
		}

		int toDayOfWeek(string day) {
			switch (day.ToLower()) {
			case "0":
			case "sun":
			case "so":
			case "7":
				return 0;
			case "1":
			case "mon":
			case "mo":
				return 1;
			case "2":
			case "tue":
			case "di":
				return 2;
			case "3":
			case "wed":
			case "mi":
				return 3;
			case "4":
			case "thu":
			case "do":
				return 4;
			case "5":
			case "fri":
			case "fr":
				return 5;
			case "6":
			case "sat":
			case "sa":
				return 6;
			default:
				throw new InvalidCastException("\"" +day + "\" is not a Day of the Week");
			}
		}

		bool matchesNumber(string reference, int number) {

			if ("" == reference) {
				throw new InvalidOperationException("Empty config is not allowed for Cron configuration");
			}
			if ("*" == reference) {
				return true;
			}

			string[] split = reference.Split(',');
			foreach (string part in split) {
				string[] range = part.Split('-');
				switch (range.Length) {
				case 1:
					if (Convert.ToInt32(range[0]) == number) {
						return true;
					}
					break;
				case 2:
					if (Convert.ToInt32(range[0]) <= number && Convert.ToInt32(range[1]) >= number) {
						return true;
					}
					break;
				default:
					throw new InvalidCastException("\"" + split + "\" is not a range");
				}
			}
			return false;
		}
		
		public void next() {
			//Console.WriteLine("prev: " + nextAction);
			nextAction = nextAction.AddMinutes(1);
			//Console.WriteLine("next: " + nextAction);
			calcNextActionTime();
		}
		public void update() {
			if (nextAction < DateTime.Now) {
				nextAction = DateTime.Now.AddMinutes(1);
				calcNextActionTime();
			}
		}

		public DateTime NextEvent {
			get {
				update();
				return nextAction;
			}
		}
	}
}

