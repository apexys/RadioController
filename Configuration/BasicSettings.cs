using System;
using System.Collections.Generic;

namespace Configuration
{
	public abstract class BasicSettings : ISettings
	{
		public BasicSettings() {
		}


		public abstract string getString(string s, string init);
		public abstract void setString(string s, string value);

		public bool getBool(string s, bool init) {
			switch (getString(s, init ? "true" : "false")) {
				case "true":
				case "yes":
				case "y":
				case "on":
				case "wahr":
				case "ja":
				case "an":
					return true;
				case "false":
				case "no":
				case "n":
				case "off":
				case "falsch":
				case "nein":
				case "aus":
					return false;
				default:
					setString(s, init ? "true" : "false");
					return init;
			}
		}

		public int getInt(string s, int i) {
			string str = getString(s, i.ToString());
			try {
				i = int.Parse(str);
			} catch {
				setString(s, i.ToString());
			}
			return i;
		}

		public float getFloat(string s, float f)  {
			string str = getString(s, f.ToString());
			try {
				f = float.Parse(str);
			} catch {
				setString(s, f.ToString());
			}
			return f;
		}

		// Arrays
		public string[] getStrings(string s, string[] init) {
			try {
				return JSON.readArray<string>(getString(s, JSON.write(init)), JSON.readString);
			} catch {}
			return init;
		}
		public int[] getInts(string s, int[] init) {
			try {
				return JSON.readArray<int>(getString(s, JSON.write(init)), JSON.readInt);
			} catch {}
			return init;
		}
	}
}

