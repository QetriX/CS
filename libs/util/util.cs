namespace com.qetrix.libs {

/* Copyright (c) 2015 QetriX. Licensed under MIT License, see /LICENSE.txt file.
 * QetriX Utils Class
 */

using System;
using System.Text;
using System.Runtime.InteropServices;

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

	public static String crKey(String text)
	{
		String stFormD = text.Normalize(NormalizationForm.FormD);
		int len = stFormD.Length;
		StringBuilder sb = new StringBuilder();
		for (int i = 0; i < len; i++) {
			System.Globalization.UnicodeCategory uc = System.Globalization.CharUnicodeInfo.GetUnicodeCategory(stFormD[i]);
			if (uc != System.Globalization.UnicodeCategory.NonSpacingMark) sb.Append(stFormD[i]);
		}
		String output = (sb.ToString().Normalize(NormalizationForm.FormC)).Trim().ToLower();
		output = output.Replace(":", "-").Replace("=", "-").Replace("(", "-").Replace(")", "-").Replace("{", "-").Replace("}", "-").Replace("!", "-").Replace(";", "-").Replace(" ", "-").Replace("+", "-").Replace("&", "-").Replace("_", "-").Replace("/", "-").Replace("@", "-at-").Replace("–", "-").Replace(".", "-").Replace(",", "-").Replace("?", "-");
		output = output.Replace("\\", "").Replace("\"", "").Replace("°", "").Replace("„", "").Replace("“", "").Replace("`", "").Replace("ʻ", "").Replace("*", "").Replace("¨", "").Replace("™", "").Replace("®", "").Replace("§", "");
		output = output.Replace("---", "-").Replace("--", "-").Replace("--", "-");
		if (output.StartsWith("-", StringComparison.InvariantCultureIgnoreCase)) output = output.Substring(1, output.Length - 1);
		if (output.EndsWith("-", StringComparison.InvariantCultureIgnoreCase)) output = output.Substring(0, output.Length - 1);
		return output;
	}
	#endregion

	[DllImport("kernel32")]
	static extern bool AllocConsole();
	public static String log(String str)
	{
		Console.WriteLine(str);
		return "";
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
