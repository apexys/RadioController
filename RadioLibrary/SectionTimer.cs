using System;

namespace RadioLibrary
{
	public class SectionTimer
	{
		CronTimer start;
		CronTimer end;

		public SectionTimer (CronTimer start, CronTimer end) {
			this.start = start;
			this.end = end;
		}

		public bool Active {
			get {
				return start.NextEvent > end.NextEvent;
			}
		}
	}
}

