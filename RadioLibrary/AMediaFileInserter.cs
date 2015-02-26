using System;

namespace RadioLibrary
{
	public abstract class AMediaFileInserter : IMediaFileProvider
	{
		IMediaFileProvider provider;

		public AMediaFileInserter(IMediaFileProvider provider) {
			this.provider = provider;
		}

		protected abstract MediaFile getInsertedMediaFile();

		#region IMediaFileProvider implementation
		public MediaFile nextMediaFile() {
			MediaFile f = getInsertedMediaFile();
			if (f == null) {
				return provider.nextMediaFile();
			}
			return f;
		}
		#endregion
	}
}

