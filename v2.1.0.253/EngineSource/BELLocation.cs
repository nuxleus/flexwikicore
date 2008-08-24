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

namespace FlexWiki
{
	/// <summary>
	/// Summary description for BELLocation.
	/// </summary>
	public class BELLocation
	{
		public BELLocation(string ctx, int line, int col)
		{
			_ContextString = ctx;
			_Line = line;
			_Column = col;
		}

		string _ContextString;
		int _Line;
		int _Column;

		public override string ToString()
		{
			return ContextString + "(" + Line + ":" + Column + ")";
		}


		public string ContextString
		{
			get
			{
				return _ContextString;
			}
		}

		public int Line
		{
			get
			{
				return _Line;
			}
		}
		
		public int Column
		{
			get
			{
				return _Column;
			}
		}

	}
}
