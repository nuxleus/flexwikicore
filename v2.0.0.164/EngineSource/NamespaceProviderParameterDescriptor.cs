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
	/// Summary description for NamespaceProviderParameterDescriptor.
	/// </summary>
	public class NamespaceProviderParameterDescriptor
	{
    private IList _Choices;
    private string _DefaultValue;
    private string _Description;
    private string _ID;
    private bool _IsPersistent;
    private string _Title;
    
    public NamespaceProviderParameterDescriptor(string id, string title, string description, string defaultValue, bool isPersistent)
		{
			_ID = id;
			_Title = title;
			_Description = description;
			_DefaultValue = defaultValue;
			_IsPersistent = isPersistent;
		}

		public bool IsPersistent
		{
			get
			{
				return _IsPersistent;
			}
		}


		public string ID
		{
			get
			{
				return _ID;
			}
		}

		public string Title
		{
			get
			{
				return _Title;
			}
		}

		public string Description
		{
			get
			{
				return _Description;
			}
		}

		public string DefaultValue
		{
			get
			{
				return _DefaultValue;
			}
		}

		public IList Choices
		{
			get
			{
				if (_Choices == null)
					return _Choices;
				return ArrayList.ReadOnly(_Choices);
			}
		}

		public void AddChoice(string choice)
		{
			if (_Choices == null)
				_Choices = new ArrayList();
			_Choices.Add(choice);
		}


	}
}
