using System;

namespace RadioController.Configuration
{
	public interface ISettings
	{
		void setString(string s, string value);

		string getString(string s, string init);
		string[] getStrings(string s, string[] init);
		bool getBool(string s, bool init);
		int getInt(string s, int i);
		int[] getInts(string s, int[] i);
		float getFloat(string s, float f);
	}
}

