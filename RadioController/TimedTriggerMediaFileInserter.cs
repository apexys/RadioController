using System;
using RadioLibrary;

namespace RadioController
{
	public class TimedTriggerMediaFileInserter : AMediaFileInserter
	{
		IMediaFileProvider provider;
		TimedTrigger trigger;

		public TimedTriggerMediaFileInserter(IMediaFileProvider normalProvider, TimedTrigger trigger, IMediaFileProvider insertProvider) :
			base(normalProvider) {
			this.trigger = trigger;
			provider = insertProvider;

			if (trigger == null || insertProvider == null) {
				throw new ArgumentNullException();
			}
		}

		#region implemented abstract members of AMediaFileInserter

		protected override MediaFile getInsertedMediaFile() {
			if (trigger.PreviousTriggerChanged) {
				MediaFile f = provider.nextMediaFile();
				RadioLogger.Logger.LogGood("Trigger insertion occured "+f.ToString());
				return f;
			}
			return null;
		}

		#endregion
	}
}

