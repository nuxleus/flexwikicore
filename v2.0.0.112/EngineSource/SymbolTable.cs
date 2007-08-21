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

namespace FlexWiki
{
	/// <summary>
	/// 
	/// </summary>
	public class SymbolTable : IValueSource
	{
    private Hashtable _Table = new Hashtable();
    
    public SymbolTable()
		{
		}


		public IBELObject ValueOf(string symbol, ArrayList args, ExecutionContext ctx)
		{
			IBELObject answer = (IBELObject)_Table[symbol];
			if (answer == null)
				return null;		// not found
			if (args != null && args.Count > 0)
				throw new ExecutionException(ctx.CurrentLocation, symbol + " is a temporary variable, not a function; no arguments allowed");
			return answer;
		}

//		public bool Contains(string symbol)
//		{
//			return _Table.Contains(symbol);
//		}

		public void Set(string symbol, IBELObject val)
		{
			_Table[symbol] = val;
		}

		public void Delete(string symbol)
		{
			_Table.Remove(symbol);
		}
		
	}
}
