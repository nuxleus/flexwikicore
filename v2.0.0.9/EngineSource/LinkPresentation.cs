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
using FlexWiki.Formatting;

namespace FlexWiki
{
	/// <summary>
	/// 
	/// </summary>
	[ExposedClass("LinkPresentation", "Presents a hyperlink")]
	public class LinkPresentation : PresentationPrimitive
	{
		public LinkPresentation(string content, string URL, string tip)
		{
			Init(content, URL, tip, null);
		}
		public LinkPresentation(string content, string URL, string tip, string attributes)
		{
			Init(content, URL, tip, attributes);
		}
		private void Init(string content, string URL, string tip, string attributes)
		{
			_Tip = tip;
			_URL = URL;
			_Content = content;
			_attributes = attributes;
		}


		private string _Tip;
		private string _URL;
		private string  _Content;

		private string Tip
		{
			get
			{
				return _Tip;
			}
		}

		private string URL
		{
			get
			{
				return _URL;
			}
		}

		private string Content
		{
			get
			{
				return _Content;
			}
		}

		public override void OutputTo(WikiOutput output)
		{
			output.WriteLink(URL, Tip, Content, Attributes);
		}

	}
}
