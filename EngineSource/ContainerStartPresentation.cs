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
    [ExposedClass("ContainerStartPresentation", "Presents the start of a <div> container")]
    public class ContainerStartPresentation : FlexWiki.PresentationPrimitive
    {
        public ContainerStartPresentation(string id, string style)
        {
            Init(id, style);
        }
        public ContainerStartPresentation(string id)
        {
            Init(id, string.Empty);
        }
        public ContainerStartPresentation()
        {
            Init(string.Empty, string.Empty);
        }
        private void Init(string id, string style)
        {
            _id = id;
            _style = style;
        }

        private string _id;
        public string Id
        {
            get
            {
                return _id;
            }
        }

        private string _style;
        public string Style
        {
            get
            {
                return _style;
            }
        }

        public override void OutputTo(WikiOutput output)
        {
            output.ContainerStart(Id, Style);
        }
    }
}
