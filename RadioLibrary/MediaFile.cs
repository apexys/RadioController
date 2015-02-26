using System;
using System.Threading;
using System.IO;

namespace RadioLibrary
{
	public class MediaFile
	{
		string path;

		public string Path {
			get {
				return path;
			}
		}

		string name;

		public string Name {
			get {
				return name;
			}
		}

		public enum MediaType
		{
			Normal,
			Jingle
		}

		private MediaType type;

		public MediaType Type {
			get {
				return type;
			}
		}

		AudioMetaData metaData;

		public AudioMetaData MetaData {
			get {
				return metaData.Clone();
			}
		}

		public MediaFile(string path, MediaType type) :
			this(path, MediaType.Normal) {
		}

		public MediaFile(string path, MediaType type) {
			this.path = path;
			this.type = type;

			name = new FileInfo(path).Name;

			metaData = MediaInfoWrapper.getMetadata(path);
		}
	}
}

