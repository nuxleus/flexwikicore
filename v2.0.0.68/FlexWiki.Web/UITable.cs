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
using System.Collections;

namespace FlexWiki.Web
{
	/// <summary>
	/// Summary description for UITable.
	/// </summary>
	public class UITable
	{
		public UITable()
		{
		}

		private ArrayList _Columns = new ArrayList();
		public IList Columns
		{
			get
			{
				return _Columns;
			}
		}

		public void AddColumn(UIColumn col)
		{
			Columns.Add(col);
		}

	}
}
