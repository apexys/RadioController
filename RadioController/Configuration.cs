using System;
using System.IO;
using System.Collections.Generic;

namespace RadioController
{
	public class Configuration
	{
		public static Dictionary<string, string> config = new Dictionary<string, string>();

		public static void read (string configFile) {
			string basis = "";
			try {
				using (StreamReader fi = new StreamReader(configFile)) {
					while (!fi.EndOfStream) {
						string line;
						string[] parts;

						line = fi.ReadLine().Trim();
						if (line != "") {
							// new basis
							if (line[0] == '[' && line[line.Length - 1] == ']') {
								basis = line.Substring(1, line.Length - 2) + ".";
							} else {
								// or a assignment
								parts = line.Split(new char[] {'='}, 2);
								if (parts.Length == 2) {
									config[basis + parts[0].Trim()] = parts[1].Trim();
								} else {
									Logger.LogWarning("Config: invalid entry \"" + line + "\" " +parts.Length.ToString());
								}
							}
						}
					}
				}
			} catch {
				Logger.LogError("Could not read the config file \"" + configFile + "\"");
			}
		}

		public static string getConfig(string s) {
			return config[s];
		}
	}
}

