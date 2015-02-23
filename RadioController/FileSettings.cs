using System;
using System.Diagnostics;
using System.IO;
using System.Collections.Generic;

namespace RadioController.Configuration
{
	public class FileSettings : BasicSettings
	{
		public Dictionary<string, string> config = new Dictionary<string, string>();

		public FileSettings(string file) {
			read(file);
		}

		public void read(string configFile) {
			string basis = "";
			try {
				using (StreamReader fi = new StreamReader(configFile)) {
					while (!fi.EndOfStream) {
						string line;
						string[] parts;

						line = fi.ReadLine();
						while (!fi.EndOfStream && line != "" && line[line.Length - 1] == '\\') {
							line = line.Substring(0, line.Length - 1) + fi.ReadLine().TrimStart();
						}
						line = line.Trim();

						if (line != "") {
							// new basis
							if (line[0] == '[' && line[line.Length - 1] == ']') {
								basis = line.Substring(1, line.Length - 2) + ".";
							} else {
								// or a assignment
								parts = line.Split(new char[] { '=' }, 2);
								if (parts.Length == 2) {
									config[basis + parts[0].Trim()] = parts[1].Trim();
								} else {
									Logger.LogWarning("Config: invalid entry \"" + line + "\" " + parts.Length.ToString());
								}
							}
						}
					}
				}
			} catch (Exception ex) {
				Logger.LogError("Could not read the config file \"" + configFile + "\" "+ex.Message + ex.StackTrace.ToString());
			}
		}

		public override string getString(string s, string init) {
			string str;
			try {
				str = config[s];
			} catch {
				config.Add(s, init);
				str = init;
			}
			return str;
		}

		public override void setString(string s, string value) {
			config.Remove(s);
			config.Add(s, value);
		}
	}
}
