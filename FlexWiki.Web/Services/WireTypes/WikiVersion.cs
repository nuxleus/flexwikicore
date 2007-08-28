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

namespace FlexWiki.Web.Services.WireTypes
{
  public class WikiVersion
  {
    private int major; 
    private int minor;
    private int build;
    private int revision; 

    public int Major
    {
      get { return major; }
      set { major = value; }
    }

    public int Minor
    {
      get { return minor; }
      set { minor = value; }
    }

    public int Build
    {
      get { return build; }
      set { build = value; }
    }

    public int Revision
    {
      get { return revision; }
      set { revision = value; }
    }
  }
}
