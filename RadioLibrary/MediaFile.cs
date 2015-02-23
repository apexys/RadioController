using System;
using System.Threading;
using System.IO;

namespace RadioLibrary
{
	public class MediaFile
	{
		string path;
		public string Path{
			get{
				return path;
			}
		}

		string name;
		public string Name{
			get{
				return name;
			}
		}

		AudioMetaData metaData;
		public AudioMetaData MetaData{
			get{
				return metaData.Clone ();
			}
		}

		public MediaFile (string path)
		{
			this.path = path;

			name = new FileInfo (path).Name;

			metaData = MediaInfoWrapper.getMetadata (path);
		}
	}
}

