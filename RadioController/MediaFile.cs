using System;
using System.Threading;
using System.IO;

namespace RadioController
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
		AudioMetaData MetaData{
			get{
				return metaData.Clone ();
			}
		}

		public MediaFile (string path)
		{
			this.path = path;

			name = new FileInfo (path).Name;

			Mplayer tempmp = new Mplayer (path);
			//tempmp.Play ();
			int i = 0;
			while (tempmp.Metadata.Title == null && i  < 10) {
				Thread.Sleep (100);
				i++;
			}
			metaData = tempmp.Metadata;
			tempmp.Dispose();
		}
	}
}

