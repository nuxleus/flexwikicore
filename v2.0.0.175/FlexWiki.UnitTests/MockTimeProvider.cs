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

namespace FlexWiki.UnitTests
{
	/// <summary>
	/// Provides an implementation of <see cref="ITimeProvider"/> for use during 
	/// unit tests. 
	/// </summary>
	public class MockTimeProvider : ITimeProvider
	{
		public MockTimeProvider(TimeSpan interval)
		{
      _interval = interval; 
    }

    private TimeSpan _interval; 
    private DateTime _now = new DateTime(2004, 10, 28, 14, 10, 59); 

    public DateTime Now
    {
      get
      {
        _now += _interval; 
        return _now; 
      }
    }

  }
}
