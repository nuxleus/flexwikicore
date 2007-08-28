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

namespace FlexWiki.Web
{
  internal class FormValues
  {
    private string _contact; 
    private string _description; 
    private string _namespace; 
    private string _title; 

    public string Contact
    {
      get { return _contact; }
      set { _contact = value; }
    }

    public string Description
    {
      get { return _description; }
      set { _description = value; }
    }

    public string Namespace
    {
      get { return _namespace; }
      set { _namespace = value; }
    }

    public string Title
    {
      get { return _title; }
      set { _title = value; }
    }
  }
}
