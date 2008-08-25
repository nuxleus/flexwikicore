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
using System.Collections.Generic;
using System.Text;
using System.Xml.Serialization;

namespace FlexWiki.Web
{
	public class AlternateStylesheetConfiguration
	{
		private string _href;
		private string _title;
		private bool _explicitTitle;

		/// <summary>
		/// HREF to linked stylesheet
		/// </summary>
		[XmlAttribute]
		public string Href
		{
			get { return _href; }
			set 
			{ 
				_href = value;
				// reset the "auto" title
				if (!_explicitTitle) _title = null;
			}
		}

		/// <summary>
		/// Title of alternate stylesheet
		/// </summary>
		[XmlAttribute]
		public string Title
		{
			get 
			{ 
				if (_title != null) return _title;
				// try to create a title based on the css file name
				if (_href == null) return null;
				int epos = _href.LastIndexOf(".");
				if (epos == -1) epos = _href.Length - 1;
				int spos = _href.LastIndexOf("/");
				if (spos == -1) spos = 0;
				else spos = spos +1; // eat "/";
				_title = _href.Substring(spos, epos - spos);
				return _title;
			}
			set 
			{ 
				_title = value;
				_explicitTitle = true;
			}
		}

	}
}
