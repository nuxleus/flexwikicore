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
    [ExposedClass("ContainerEndPresentation", "Presents the end of a <div> container")]
    public class ContainerEndPresentation : FlexWiki.PresentationPrimitive
    {
		
		private string _type;


		public ContainerEndPresentation(string type)
        {
            // Check for allowed container elements.
            if ((false == string.Equals(ContainerStartPresentation.Div, 
                type, StringComparison.CurrentCultureIgnoreCase)) &&
                (false == string.Equals(ContainerStartPresentation.Span, 
                type, StringComparison.CurrentCultureIgnoreCase)))
            {
                throw new FlexWikiException("Invalid 'type' parameter. Must be 'span' or 'div'.");
            }
            _type = type;
        }
		

		public string ContainterElement
        {
            get
            {
                return _type;
            }
        }
		

		public override void OutputTo(WikiOutput output)
        {
            output.ContainerEnd(_type);
        }
		

    }
}
