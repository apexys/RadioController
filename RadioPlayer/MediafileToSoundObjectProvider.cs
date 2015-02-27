using System;
using RadioLibrary;
using RadioLogger;

namespace RadioPlayer
{
	public class MediafileToSoundObjectProvider : ISoundObjectProvider
	{
		IMediaFileProvider provider;
		IMixer mixer;

		public MediafileToSoundObjectProvider(IMixer mixer, IMediaFileProvider provider) {
			this.mixer = mixer;
			this.provider = provider;
			if (mixer == null || provider == null) {
				throw new ArgumentNullException();
			}

		}

		#region ISoundObjectProvider implementation
		public ISoundObject nextSound() {
			int i;
			for (i=0; i<10; i++) {
				try {
					return mixer.createSound(provider.nextMediaFile());
				} catch (Exception ex) {
					Logger.LogException(ex);
				}
			}
			return null;
		}

		public bool interject() {
			return provider.interject();
		}
		#endregion
	}
}

