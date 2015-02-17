using System;
using System.IO;
using System.Xml.Serialization;

namespace RadioController
{
	public class Show
	{
		string show_name;
		DateTime show_time;
		TimeSpan show_duration;
		string show_path;

		public string Name{
			get{
				return show_name;
			}
		}

		public DateTime Time {
			get {
				return show_time;
			}
		}

		public TimeSpan Duration {
			get {
				return show_duration;
			}
		}

		public string Path {
			get {
				return Path;
			}
		}

		public Show(string name, DateTime time, TimeSpan duration, string path){
			show_name = name;
			show_time = time;
			show_path = path;
			show_duration = duration;
		}

		public void Save(string path){
			XmlSerializer xser =  new XmlSerializer(this.GetType());
			FileStream fstr = new FileStream(path, FileMode.OpenOrCreate);
			xser.Serialize(fstr,this);
			fstr.Flush();
			fstr.Close();
		}

		public static Show Load(string path){
			XmlSerializer xser =  new XmlSerializer(typeof(Show));
			FileStream fstr = new FileStream(path, FileMode.Open);
			Show temp = (Show) xser.Deserialize(fstr);
			fstr.Close();
			return temp;
		}

	}
}

