using System;

namespace RadioController
{
	public class AudioMetaData
	{
		public string Artist;
		public string Album;
		public string Title;

		public AudioMetaData(){
			Artist = "";
			Album = "";
			Title = "";
		}

		public AudioMetaData(string artist, string album, string title){
			this.Artist = artist;
			this.Album = album;
			this.Title = title;
		}

		public override string ToString(){
			return Artist + " - " + Album + " - " + Title;
		}

		public AudioMetaData Clone(){
			return new AudioMetaData(this.Artist, this.Album, this.Title);
		}
	}
}

