using System;
using System.Diagnostics;
using System.IO;
using System.Collections.Generic;
using RadioLogger;

namespace Configuration
{
	public class FileSettings : BasicSettings
	{
		public Dictionary<string, string> config = new Dictionary<string, string>();

		public FileSettings(string file) {
			read(file);
		}

		public void read(string configFile) {
			string basis = "";
			bool blockComment = false;
			try {
				using (StreamReader fi = new StreamReader(configFile)) {
					while (!fi.EndOfStream) {
						string line;
						string trimed;
						string[] parts;

						line = fi.ReadLine();
						trimed = line.Trim();

						// filter block komments
						if (blockComment) {
							blockComment = !trimed.EndsWith("*/");
							continue;
						}
						if (trimed.StartsWith("/*")) {
							blockComment = true;
							continue;
						}

						// skip comments and empty lines
						if (trimed.StartsWith("//") || trimed == "") {
							continue;
						}

						// append next line on line break
						while (!fi.EndOfStream && line != "" && line[line.Length - 1] == '\\') {
							line = line.Substring(0, line.Length - 1) + fi.ReadLine().TrimStart();
						}
						line = line.Trim();

						if (line == "") {
							continue;
						}

						// get line information
						if (line[0] == '#') {
							// instruction
							if (line.Length>9 && line.StartsWith("#include") && (line[8] == ' ' || line[8] == '\t')) {
								read(new FileInfo(configFile).Directory.FullName + "/" + line.Substring(9).Trim());
							}
						} else if (line[0] == '[' && line[line.Length - 1] == ']') {
							// new basis
							basis = line.Substring(1, line.Length - 2).Trim() + ".";
							if (basis == ".") {
								basis = "";
							}
						} else {
							// a assignment
							parts = line.Split(new char[] { '=' }, 2);
							if (parts.Length == 2) {
								config[basis + parts[0].Trim()] = parts[1].Trim();
							} else {
								Logger.LogWarning("Config: invalid entry \"" + line + "\" " + parts.Length.ToString());
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

