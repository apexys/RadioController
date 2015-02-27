using System;

namespace RadioLibrary
{
	public class StreamInserter : AMediaFileInserter
	{
		HTTPServer htserver;
		string targetAddress;
		bool previousState = false;
		public StreamInserter (IMediaFileProvider provider, int HTTPPort, string targetAddress) :
			base(provider)
		{
			htserver = new HTTPServer (HTTPPort);
			this.targetAddress = targetAddress;
		}

		#region implemented abstract members of AMediaFileInserter

		protected override MediaFile getInsertedMediaFile ()
		{
			if (htserver.State == true) {//true if live
				return new MediaFile (targetAddress);
			} else {
				return null;
			}
		}

		public override bool interject() {
			if (htserver.State != previousState) {
				previousState = htserver.State;
					return true;
			}
			return base.interject();

		}

		#endregion
	}
}

