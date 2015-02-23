using System;

namespace RadioController.Configuration
{
	public class Settings
	{
		static ISettings conf = null;

		// changing the configuration
		public static void setSettings(ISettings conf) {
			Settings.conf = conf;
		}
		public static ISettings getSettings() {
			return conf;
		}

		// setting the values
		public static void setString(string s, string value) {
			if (conf != null) {
				conf.setString(s, value);
			}
		}

		// getting the values
		public static string getString(string s, string init) {
			if (conf == null) {
				return init;
			} else {
				return conf.getString(s, init);
			}
		}
		public static string[] getStrings(string s, string[] init) {
			if (conf == null) {
				return init;
			} else {
				return conf.getStrings(s, init);
			}
		}
		public static bool getBool(string s, bool init) {
			if (conf == null) {
				return init;
			} else {
				return conf.getBool(s, init);
			}
		}
		public static int getInt(string s, int init) {
			if (conf == null) {
				return init;
			} else {
				return conf.getInt(s, init);
			}
		}
		public static int[] getInts(string s, int[] init) {
			if (conf == null) {
				return init;
			} else {
				return conf.getInts(s, init);
			}
		}
		public static float getFloat(string s, float init) {
			if (conf == null) {
				return init;
			} else {
				return conf.getFloat(s, init);
			}
		}
	}
}

