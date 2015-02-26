using System;
using RadioLibrary;

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
				} catch {
					;
				}
			}
			return null;
		}
		#endregion
	}
}

