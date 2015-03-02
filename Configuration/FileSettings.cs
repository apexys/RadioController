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
			Dictionary<string, int> vars;
			int lineNumber = 0;
			try {
				if (!File.Exists(configFile)) {
					return;
				}
				using (StreamReader fi = new StreamReader(configFile)) {
					vars = new Dictionary<string, int>();
					while (!fi.EndOfStream) {
						string line;
						string trimed;
						string varName;
						string[] parts;
						int i, j, varVal;

						lineNumber++;
						
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
							if (line.Length > 9 && line.StartsWith("#include") && (line[8] == ' ' || line[8] == '\t')) {
								read(new FileInfo(configFile).Directory.FullName + "/" + line.Substring(9).Trim());
							} else {
								Logger.LogWarning("[Config] [Reading] unknown operation: \"" + line + "\" at line: " + lineNumber);
							}
						} else if (line[0] == '$') {
							// var
							if (line.EndsWith("++")) {
								varName = line.Substring(1, line.Length - 3).Trim();
								if (-1 == varName.IndexOf(" ")) {
									varVal = 0;
									if (vars.TryGetValue(varName, out varVal)) {
										vars[varName] = varVal + 1;
									} else {
										Logger.LogWarning("[Config] [Reading] variable not set: \"" + varName + "\" at line: " + lineNumber);
									}
								} else {
									Logger.LogWarning("[Config] [Reading] not a variable: \"" + varName + "\" at line: " + lineNumber);
								}
							} else if (-1 != line.IndexOf("=")) {
								parts = line.Split(new char[] { '=' }, 2);
								if (parts.Length == 2) {
									parts[0] = parts[0].Substring(1).Trim();
									try {
										vars[parts[0]] = Convert.ToInt32(parts[1]);
									} catch {
										Logger.LogError("[Config] [Reading] not a number: \"" + parts[1] + "\" at line: " + lineNumber);
									}
								} else {
									Logger.LogWarning("[Config] [Reading] unknown operation: \"" + line + "\" at line: " + lineNumber);
								}
							} else {
								Logger.LogWarning("[Config] [Reading] unknown operation: \"" + line + "\" at line: " + lineNumber);
							}
						} else if (line[0] == '[' && line[line.Length - 1] == ']') {
							// new basis
							basis = line.Substring(1, line.Length - 2).Trim() + ".";
							if (basis == ".") {
								basis = "";
							}
						} else {
							// a assignment
							while (-1 != (i = line.LastIndexOf("$"))) {
								j = line.IndexOfAny(new char [] { ']', '=' }, i);
								if (j != -1) {
									varName = line.Substring(i + 1, j - i - 1).Trim();
								} else {
									varName = line.Substring(i + 1).Trim();
								}

								if (i>=2 && line[i-1]=='+' && line[i-2]=='+') {
									// pre increment the value
									varVal = 0;
									i -= 2;
									if (vars.TryGetValue(varName, out varVal)) {
										vars[varName] = varVal + 1;
									} else {
										Logger.LogWarning("[Config] [Reading] variable not set: \"" + varName + "\" at line: " + lineNumber);
										break;
									}
								}

								if (varName.EndsWith("++")) {
									if (-1 == varName.IndexOf(" ")) {
										varName = varName.Substring(0, varName.Length - 2).Trim();

										// First evaluate the variable
										varVal = 0;
										if (vars.TryGetValue(varName, out varVal)) {
											if(j!=-1) {
												line = line.Substring(0, i) + varVal.ToString() + line.Substring(j);
											} else {
												line = line.Substring(0, i) + varVal.ToString();
											}
										} else {
											Logger.LogWarning("[Config] [Reading] variable not set: \"" + varName + "\" at line: " + lineNumber);
											break;
										}

										// post increment the value
										varVal = 0;
										if (vars.TryGetValue(varName, out varVal)) {
											vars[varName] = varVal + 1;
										} else {
											Logger.LogWarning("[Config] [Reading] variable not set: \"" + varName + "\" at line: " + lineNumber);
											break;
										}
									} else {
										Logger.LogWarning("[Config] [Reading] not a variable: \"" + varName + "\" at line: " + lineNumber);
										break;
									}
								} else {
									// just evaluate the value
									varVal = 0;
									if (vars.TryGetValue(varName, out varVal)) {
										if(j!=-1) {
											line = line.Substring(0, i) + varVal.ToString() + line.Substring(j);
										} else {
											line = line.Substring(0, i) + varVal.ToString();
										}
									} else {
										Logger.LogWarning("[Config] [Reading] variable not set: \"" + varName + "\" at line: " + lineNumber);
										break;
									}
								}
							}
							if (i == -1) {
								parts = line.Split(new char[] { '=' }, 2);
								if (parts.Length == 2) {
									config[basis + parts[0].Trim()] = parts[1].Trim();
								} else {
									Logger.LogWarning("[Config] [Reading] invalid entry \"" + line + "\" at line: " + lineNumber);
								}
							}
						}
					}
				}
			} catch (Exception ex) {
				Logger.LogException(ex);
				Logger.LogError("[Config] [Reading] Could not fully read the config file \"" + configFile + "\" last line: " + lineNumber + " Message: " + ex.ToString());
			}
		}

		public override string getString(string s, string init) {
			string str;
			try {
				str = config[s];
			} catch {
				Logger.LogInformation("Missing config option for \"" + s + "\"");
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

