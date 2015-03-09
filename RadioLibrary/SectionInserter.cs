using System;

namespace RadioLibrary
{
	public class SectionInserter : AMediaFileInserter
	{
		IMediaFileProvider media;
		SectionTimer timer;
		bool previousState = false;
		bool needInsertioen = false;

		public SectionInserter (IMediaFileProvider provider, IMediaFileProvider media, SectionTimer timer) :
					base(provider)
		{
			this.media = media;
			this.timer = timer;
		}

		#region implemented abstract members of AMediaFileInserter
		protected override MediaFile getInsertedMediaFile ()
		{
			if (timer.Active) {
				needInsertioen = false;
				return media.nextMediaFile();
			} else {
				return null;
			}
		}

		public override bool interject ()
		{
			if (timer.Active != previousState) {
				needInsertioen = previousState = timer.Active;
				return true;
			}
			return needInsertioen || base.interject();

		}
		#endregion
	}
}

