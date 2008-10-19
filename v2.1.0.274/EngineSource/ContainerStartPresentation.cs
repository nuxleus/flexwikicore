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
    [ExposedClass("ContainerStartPresentation", "Presents the start of a <div> or <span> container")]
    public class ContainerStartPresentation : FlexWiki.PresentationPrimitive
    {
		
		public const string Div = "div";
		public const string Span = "span";
		private string _id;
		private string _style;
		private string _type;


		public ContainerStartPresentation(string type, string id, string style)
        {
            Init(type, id, style);
        }
		
		public ContainerStartPresentation(string type, string id)
        {
            Init(type, id, string.Empty);
        }
		
		public ContainerStartPresentation(string type)
        {
            Init(type, string.Empty, string.Empty);
        }
		

		public string ContainerElement
        {
            get
            {
                return _type;
            }
        }
		
		public string Id
        {
            get
            {
                return _id;
            }
        }
		
		public string Style
        {
            get
            {
                return _style;
            }
        }
		

		public override void OutputTo(WikiOutput output)
        {
            output.ContainerStart(ContainerElement, Id, Style);
        }
		
		private void Init(string type, string id, string style)
        {
            // Check for allowed container elements.
            if ((false == string.Equals(
                Div, type, StringComparison.CurrentCultureIgnoreCase)) &&
                (false == string.Equals(
                Span, type, StringComparison.CurrentCultureIgnoreCase)))
            {
                throw new FlexWikiException("Invalid 'type' parameter. Must be 'span' or 'div'.");
            }
            _type = type;
            _id = id;
            _style = style;
        }


    }
}
