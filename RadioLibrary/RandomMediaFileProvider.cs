using System;

namespace RadioLibrary
{
	public class RandomMediaFileProvider : IMediaFileProvider
	{
		MediaFolder mediaFolder;
		public RandomMediaFileProvider(MediaFolder mediaFolder) {
			this.mediaFolder = mediaFolder;
			if (mediaFolder == null) {
				throw new ArgumentNullException();
			}
		}

		#region IMediaFileProvider implementation
		public MediaFile nextMediaFile() {
			return mediaFolder.pickRandomFile();
		}

		public bool interject() {
			return false;
		}
		#endregion
	}
}

