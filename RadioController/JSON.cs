using System;
using System.Text;
using System.Collections.Generic;

namespace RadioController
{
	public class JSON
	{
		public JSON() {
		}

		public static string write(string[] arr) {
			StringBuilder res = new StringBuilder();
			int i, j, len;
			string str;

			res.Append("[ ");
			for (i=0; i<arr.Length; i++) {
				if (arr[i] == null) {
					continue;
				}

				if (i != 0) {
					res.Append(", ");
				}
				res.Append('"');
				str = arr[i];
				len = str.Length;
				for (j=0; j<len; j++) {
					switch (str[j]) {
					case '\n':
						res.Append("\\n");
						break;
					case '\r':
						res.Append("\\r");
						break;
					case '\t':
						res.Append("\\t");
						break;
					case '"':
						res.Append("\\\"");
						break;
					case '\\':
						res.Append("\\\\");
						break;
					default:
						res.Append(str[j]);
						break;
					}
				}
				res.Append('"');

			}
			res.Append(" ]");

			return res.ToString();
		}


		public static string readString(string str) {
			int pos = 0;
			return readString(str, ref pos);
		}
		public static string readString(string str, ref int pos) {
			StringBuilder res;
			int len = str.Length;
			bool escape = false;

			res = new StringBuilder();

			// skip space
			while (pos < len && (str[pos] == ' ' || str[pos] == '\t')) {
				pos++;
			}
			// end?
			if (pos < len) {
				// correct start of the string?
				if (str[pos] != '"') {
					throw new InvalidCastException("incorrect JSON string");
				}
				pos++;

				while (pos < len && (escape || str[pos] != '"')) {
					if (escape) {
						switch (str[pos]) {
						case 'n':
							res.Append('\n');
							break;
						case 'r':
							res.Append('\r');
							break;
						case 't':
							res.Append('\t');
							break;
						default:
							res.Append(str[pos]);
							break;
						}
						escape = false;
					} else {
						switch (str[pos]) {
						case '\\':
							escape = true;
							break;
						default:
							res.Append(str[pos]);
							break;
						}
					}
					pos++;
				}
				// correct end of the string? (pev. loop ends at string end or '"')
				if (pos >= len) {
					throw new InvalidCastException("incorrect JSON string array");
				}
				pos++;
				return res.ToString();
			}
			throw new InvalidCastException("no JSON string found");
		}
		
		public static string[] readStringArray(string str) {
			int pos = 0;
			return readStringArray(str, ref pos);
		}
		public static string[] readStringArray(string str, ref int pos) {
			int len = str.Length;
			List<string> res;

			// skip space
			while (pos < len && (str[pos] == ' ' || str[pos] == '\t')) {
				pos++;
			}

			if (pos < len && str[pos] == '[') {
				res = new List<string>();
				pos++;

				while (true) {

					res.Add(readString(str, ref pos));

					// skip space
					while (pos < len && (str[pos] == ' ' || str[pos] == '\t')) {
						pos++;
					}

					// check correckt string seperator
					if (pos >= len || (str[pos] != ',' && str[pos] != ']')) {
						throw new InvalidCastException("incorrect JSON string array");
					}
					if (str[pos] == ']') {
						return res.ToArray();
					}
				}
			} else {
				throw new InvalidCastException("incorrect JSON string array");
			}
		}
	}
}

