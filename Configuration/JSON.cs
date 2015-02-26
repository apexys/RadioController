using System;
using System.Text;
using System.Collections.Generic;

namespace Configuration
{
	//TODO: Vielleicht durch http://visitmix.com/writings/the-rise-of-json ersetzen
	public class JSON
	{
		public JSON() {
		}

		public delegate T inputConverter<T>(string str, ref int pos);

		public static string write(string[] arr) {
			StringBuilder res = new StringBuilder();
			int i, j, len;
			string str;
			bool first = true;

			res.Append("[ ");
			for (i=0; i<arr.Length; i++) {
				if (arr[i] == null) {
					continue;
				}

				if (!first) {
					first = false;
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

		public static string write(int[] arr) {
			StringBuilder res = new StringBuilder();
			int i;

			res.Append("[ ");
			for (i=0; i<arr.Length; i++) {
				if (i != 0) {
					res.Append(", ");
				}
				res.Append(arr[i]);
			}
			res.Append(" ]");

			return res.ToString();
		}


		// reading
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

		public static int readInt(string str) {
			int pos = 0;
			return readInt(str, ref pos);
		}
		public static int readInt(string str, ref int pos) {
			int number = 0;
			int len = str.Length;

			// skip space
			while (pos < len && (str[pos] == ' ' || str[pos] == '\t')) {
				pos++;
			}

			if (pos > len || str[pos] < '0' || str[pos] > '9') {
				throw new InvalidCastException("not a JSON int '"+str[pos]+"'("+pos+")");
			}

			while (pos < len && str[pos] >= '0' && str[pos] <= '9') {
				number *= 10;
				number += str[pos] - '0';
				pos++;
			}
			return number;
		}
		
		public static T[] readArray<T>(string str, inputConverter<T> reader) {
			int pos = 0;
			return readArray<T>(str, ref pos, reader);
		}
		public static T[] readArray<T>(string str, ref int pos, inputConverter<T> reader) {
			int len = str.Length;
			List<T> res;

			// skip space
			while (pos < len && (str[pos] == ' ' || str[pos] == '\t')) {
				pos++;
			}

			if (pos < len && str[pos] == '[') {
				res = new List<T>();
				pos++;

				while (true) {

					res.Add(reader(str, ref pos));

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
					pos++;
				}
			} else {
				throw new InvalidCastException("incorrect JSON string array");
			}
		}
	}
}

