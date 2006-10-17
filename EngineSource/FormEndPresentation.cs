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
	/// 
	[ExposedClass("FormEndPresentation", "Presents the end of a form")]
	public class FormEndPresentation : FlexWiki.PresentationPrimitive
	{
		public FormEndPresentation()
		{
		}

		public override void OutputTo(WikiOutput output)
		{
			output.FormEnd();
		}

	}
}
