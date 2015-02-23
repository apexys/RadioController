using System;

namespace RadioLibrary
{
	public class AudioMetaData
	{
		public string Artist;
		public string Album;
		public string Title;
		public string Filename;

		public AudioMetaData(){
			Artist = "";
			Album = "";
			Title = "";
			Filename = "";
		}

		public AudioMetaData(string artist, string album, string title, string filename){
			this.Artist = artist;
			this.Album = album;
			this.Title = title;
			this.Filename = filename;
		}

		public override string ToString(){
			if (Title.Trim () != "") {
				return Artist + " - " + Album + " - " + Title;
			} else {
				return Filename;
			}
		}

		public AudioMetaData Clone() {
			return new AudioMetaData(this.Artist, this.Album, this.Title, this.Filename);
		}
	}
}

