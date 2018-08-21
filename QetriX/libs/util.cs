namespace com.qetrix.libs
{
	using System;
	using System.Collections;
	using System.Collections.Generic;
	using System.Linq;
	using System.Reflection;
	using System.Runtime.InteropServices;
	using System.Text;
	using System.Text.RegularExpressions;

	public static class Util
	{
		#region String functions
		public static String urlEncode(String inString)
		{
			StringBuilder sb = new StringBuilder();
			int limit = 32000;
			int loops = inString.Length / limit;

			// EscapeDataString is limited, for larger Strings we have to break it down into smaller increments
			for (int i = 0; i <= loops; i++) {
				if (i < loops) sb.Append(Uri.EscapeDataString(inString.Substring(limit * i, limit)));
				else sb.Append(Uri.EscapeDataString(inString.Substring(limit * i)));
			}

			return sb.ToString().Replace(" ", "+");
		}

		public static String urlDecode(String text)
		{
			text = text.Replace("+", " ");
			return System.Uri.UnescapeDataString(text);
		}

		public static string array_shift(ref List<string> arr)
		{
			var str = arr[0];
			arr.RemoveAt(0);
			return str;
		}

		public static List<string> getQueRow(string str, string delimiter, Dictionary<string, string> data)
		{
			var matches = System.Text.RegularExpressions.Regex.Matches("/([^" + delimiter + ":]+)(:[^" + delimiter + "]*)?\\" + delimiter + "/", str.Trim() + delimiter);
			if (matches.Count == 0) return null;

			var arr = new List<string>();
			foreach (var match in matches) {
				var val = Util.processVars(match.ToString(), data);
			}
			return arr;
		}

		public static String crKey(String text)
		{
			String stFormD = Util.normalizeString(text);
			int len = stFormD.Length;
			StringBuilder sb = new StringBuilder();
			for (int i = 0; i < len; i++) {
				System.Globalization.UnicodeCategory uc = System.Globalization.CharUnicodeInfo.GetUnicodeCategory(stFormD[i]);
				if (uc != System.Globalization.UnicodeCategory.NonSpacingMark) sb.Append(stFormD[i]);
			}
			String output = Util.normalizeString(sb.ToString()).Trim().ToLower();
			output = output.Replace(":", "-").Replace("=", "-").Replace("(", "-").Replace(")", "-").Replace("{", "-").Replace("}", "-").Replace("!", "-").Replace(";", "-").Replace(" ", "-").Replace("+", "-").Replace("&", "-").Replace("_", "-").Replace("/", "-").Replace("@", "-at-").Replace("–", "-").Replace(".", "-").Replace(",", "-").Replace("?", "-");
			output = output.Replace("\\", "").Replace("\"", "").Replace("°", "").Replace("„", "").Replace("“", "").Replace("`", "").Replace("ʻ", "").Replace("*", "").Replace("¨", "").Replace("™", "").Replace("®", "").Replace("§", "");
			output = output.Replace("---", "-").Replace("--", "-").Replace("--", "-");
			if (output.StartsWith("-", StringComparison.OrdinalIgnoreCase)) output = output.Substring(1, output.Length - 1);
			if (output.EndsWith("-", StringComparison.OrdinalIgnoreCase)) output = output.Substring(0, output.Length - 1);
			return output;
		}

		public static String crKeyRev(String text)
		{
			var s = text.Replace("-", " ");
			s = Regex.Replace(s, @"(^\w)|(\s\w)", m => m.Value.ToUpper());
			s = Regex.Replace(s, @"\s.\s", m => m.Value.ToLower());
			return s;
		}

		private static String normalizeString(String value)
		{
			byte[] tempBytes = Encoding.GetEncoding("ISO-8859-8").GetBytes(value);
			return Encoding.UTF8.GetString(tempBytes, 0, tempBytes.Length);
		}
		#endregion

		/// <summary>
		/// processVars
		/// </summary>
		/// <param name="str">String with %vars%</param>
		/// <param name="data">Data for vars, keys must match with vars</param>
		/// <returns>Processed string</returns>
		public static string processVars(string str, Dictionary<string, string> data)
		{
			/// If no variables or no data, return the original string (no point of parsing it)
			if (!str.Contains("%") || data.Count == 0) return str;

			var xx = str.Split('%');
			var strx = "";
			foreach (var x in xx) strx += data.ContainsKey(x) ? data[x] : x;
			return strx;
		}

		/** Ceil up to nearest ten, hundread... */
		public static int ceil(float value, int precision)
		{
			var pow = Math.Pow(10, precision);
			return (int)((Math.Ceiling(pow * value) + Math.Ceiling(pow * value - Math.Ceiling(pow * value))) / pow);
		}

		public static bool isActionPath(string value)
		{
			return value.Contains("/") && !value.Contains("\t");
		}

		public static String log(String str)
		{
			return "";
		}

		public static string formatDateTime(string dtstr)
		{
			return Util.formatDateTime(dtstr, "%3\\$d.%2\\$d.%1\\$d %4\\$02d:%5\\$02d");
		}


		public static string formatNumber(float num, int maxdec)
		{
			return "";
		}

		public static string formatNumber(string num)
		{
			return "";
		}

		public static int parseTime(string time)
		{
			var t = time.Replace(" ", "").Split(':');
			return (int.Parse(t[0]) * 3600) + (int.Parse(t[1]) * 60) + (t.Length > 2 ? int.Parse(t[2]) : 0);
		}

		public static string formatDateTime(string dtstr, string format)
		{
			/*
			* 201100000000 = year 2011 (2011)
			* 201100010000 = 1st quarter of 2011 (I/2011)
			* 201110000000 = October 2011 (=> 201110) (10/2010)
			* 201101000000 = January 2011 (01/2010)
			* 201101100000 = January 10, 2011 (10.1.2011)
			 */
			if (dtstr == "") return "";
			return "";
		}

		public static object convert(object data, string fromFormat, string toFormat, string toType, Dict args)
		{
			Type myType = QPage.getInstance().loadConverter(fromFormat, toFormat, toType);

			try {
				MethodInfo method = myType.GetMethod("convert");
				if (method == null) throw new Exception("Function convert not found in converter " + fromFormat);
				var myInstance = Activator.CreateInstance(myType);
				return method.Invoke(myInstance, new object[] { data, args });
			} catch (Exception ex) {
				return "Exception: " + ex.InnerException.Message + " at " + ex.InnerException.Source + "\n" + String.Join("\n", ex.InnerException.StackTrace);
			}
		}
	}

	public class Dict : IEnumerable<KeyValuePair<string, string>>
	{
		private Dictionary<string, string> _data = new Dictionary<string, string>();

		public Dict()
		{
		}

		public string get(string key)
		{
			return get(key, "");
		}

		public string get(string key, string valueIfNotFound)
		{
			key = key.ToLower();
			return this._data.ContainsKey(key) ? this._data[key] : valueIfNotFound;
		}

		public Dict set(string key, string value)
		{
			key = key.ToLower();
			if (this._data.ContainsKey(key)) this._data[key] = value;
			else this._data.Add(key, value);
			return this;
		}

		public Dict set(Dict data)
		{
			this._data.Concat(data.toArray()).ToDictionary(d => d.Key, d => d.Value);
			return this;
		}

		public Dict set(Dictionary<string, string> data)
		{
			foreach (var item in data) if (_data.ContainsKey(item.Key)) _data[item.Key] = item.Value; else _data.Add(item.Key, item.Value);
			return this;
		}

		public bool has(string key)
		{
			key = key.ToLower();
			return this._data.ContainsKey(key) && this._data[key] != "";
		}

		public Dict del(string key)
		{
			_data.Remove(key.ToLower());
			return this;
		}

		public Dictionary<string, string> toArray()
		{
			return this._data;
		}

		public IEnumerator<KeyValuePair<string, string>> GetEnumerator()
		{
			return _data.GetEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}
	}

	/// <summary>
	/// Java-like NULL integers without using "int?" (nullable int).
	/// Usage: Integer i = 77;
	/// </summary>
	public class Integer
	{
		private int Value;

		private Integer(int val)
		{
			this.Value = val;
		}

		public static implicit operator Integer(int value)
		{
			return new Integer(value);
		}

		public static implicit operator int(Integer i)
		{
			return i.Value;
		}
	}
}
