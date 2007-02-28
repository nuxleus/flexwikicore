#region License Statement
// Copyright (c) Microsoft Corporation.  All rights reserved.
//
// The use and distribution terms for this software are covered by the 
// Common Public License 1.0 (http://opensource.org/licenses/cpl.php)
// which can be found in the file CPL.TXT at the root of this distribution.
// By using this software in any fashion, you are agreeing to be bound by 
// the terms of this license.
//
// You must not remove this notice, or any other, from this software.
#endregion

using System;
using FlexWiki;

namespace FlexWiki.Formatting
{
	/// <summary>
	/// Summary description for TableCellInfo.
	/// </summary>
	public class TableCellInfo
	{
		public TableCellInfo()
		{
		}


		public AlignOption TableAlignment = AlignOption.None;
		public AlignOption CellAlignment = AlignOption.None;
		public bool HasBorder = true;
		public bool IsHighlighted = false;
		public int ColSpan = 1;
		public int RowSpan = 1;
		public bool AllowBreaks = true;
		public int CellWidth = UnspecifiedWidth;
		public int TableWidth = UnspecifiedWidth;
		public string BackgroundColor = null;

		static public int UnspecifiedWidth = -1;

		/// <summary>
		/// Parse the given options and change the options of this object as a side-effect.  Answer null if the parse goes OK or an error message if not.
		/// </summary>
		/// <param name="format"></param>
		/// <returns></returns>
		public string Parse(string format)
		{
			for (int p = 0; p < format.Length; p++)
			{
				char ch = format[p];
				string num;

				switch (ch)
				{
					case '!':
						IsHighlighted = true;
						continue;

					case '+':
						AllowBreaks = false;
						continue;
				
					case ']':
						CellAlignment = AlignOption.Right;
						continue;

					case '[':
						CellAlignment = AlignOption.Left;
						continue;

					case '^':
						CellAlignment = AlignOption.Center;
						continue;

					case 'T':
						if (p + 1 >= format.Length)
							return "Missing option for 'T' in table format";
						p++;
						char ch2 = format[p];
						switch (ch2)
						{
							case '-':
								HasBorder = false;
								continue;

							case ']':
								TableAlignment = AlignOption.Right;
								continue;

							case '[':
								TableAlignment = AlignOption.Left;
								continue;

							case '^':
								TableAlignment = AlignOption.Center;
								continue;

							case 'W':
								num = "";
								if (p + 1 >= format.Length  || !Char.IsDigit(format[p + 1]))
									return "Missing number for table width percentage option 'TW' in table format";
								p++;
								while (p < format.Length && Char.IsDigit(format[p]))
									num += format[p++];
								TableWidth = Int32.Parse(num);
								p--;
								continue;

							default:
								return "Unknown table formatting option: T" + ch2;
						}

					case 'W':
						num = "";
						if (p + 1 >= format.Length  || !Char.IsDigit(format[p + 1]))
							return "Missing number for cell width percentage option 'W' in table format";
						p++;
						while (p < format.Length && Char.IsDigit(format[p]))
							num += format[p++];
						CellWidth = Int32.Parse(num);
						p--;
						continue;
  
					case 'R':
						num = "";
						if (p + 1 >= format.Length || !Char.IsDigit(format[p + 1]))
							return "Missing number for row span option 'R' in table format";
						p++;
						while (p < format.Length && Char.IsDigit(format[p]))
							num += format[p++];
						RowSpan = Int32.Parse(num);
						p--;
						continue;

					case 'C':
						num = "";
						if (p + 1 >= format.Length || !Char.IsDigit(format[p + 1]))
							return "Missing number for column span option 'C' in table format";
						p++;
						while (p < format.Length && Char.IsDigit(format[p]))
							num += format[p++];
						ColSpan = Int32.Parse(num);
						p--;
						continue;

					case '*':
						if (p + 1 >= format.Length)
							return "Missing color for background color option '<>' in table format";
						p++;
						BackgroundColor = "";
						while (p < format.Length && format[p] != '*')
							BackgroundColor += format[p++];
						if (BackgroundColor == "")
							return "Missing color for background color option '**' in table format";
						continue;


					default:
						return "Unknown table formatting option: " + ch;
				}
			}
			return null;
		}

		
	}

}
