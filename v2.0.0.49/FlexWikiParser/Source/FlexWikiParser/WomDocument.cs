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
using System.Xml;
using System.IO;

namespace FlexWiki
{
    public class WomDocument : WomElement
    {
        // Constructors 

        public WomDocument()
        {
        }

        public WomDocument(params object[] contents)
        {
            Add(contents);
        }

        public WomDocument(WomDocument other)
            : base(other)
        {
        }

        // Properties

        public WomElement Root
        {
            get
            {
                if (ElementList.Count > 0)
                {
                    return ElementList[0];
                }
                return null;
            }
        }

        // Methods

        public static WomDocument Load(string fileName)
        {
            using (XmlTextReader reader = new XmlTextReader(fileName))
            {
                return WomDocument.Load(reader);
            }
        }

        public static WomDocument Load(TextReader reader)
        {
            XmlTextReader xmlReader = new XmlTextReader(reader);
            return WomDocument.Load(xmlReader);
        }

        public static new WomDocument Load(XmlReader reader)
        {
            if (reader == null)
            {
                throw new ArgumentNullException("reader");
            }
            if (reader.NodeType == XmlNodeType.None)
            {
                reader.Read();
            }
            WomDocument doc = new WomDocument();
            doc.ReadContentFrom(reader);
            if (!reader.EOF || doc.Root == null)
            {
                throw new InvalidOperationException();
            }
            return doc;
        }

        public static WomDocument Parse(string value)
        {
            XmlTextReader reader = new XmlTextReader(new StringReader(value));
            return WomDocument.Load(reader);
        }

        public override void ReadContentFrom(XmlReader reader)
        {
            if (reader == null)
            {
                throw new ArgumentNullException("reader");
            }
            RemoveContent();
            bool process = true;
            while (process)
            {
                switch (reader.NodeType)
                {
                    case XmlNodeType.Element:
                        AddElement(new WomElement(reader));
                        break;

                    case XmlNodeType.ProcessingInstruction:
                    case XmlNodeType.Comment:
                    case XmlNodeType.DocumentType:
                    case XmlNodeType.Whitespace:
                    case XmlNodeType.XmlDeclaration:
                        break; // Ignore these elements

                    default:
                        process = false; // cannot take these elements as content
                        continue;
                }
                reader.Read();
            }
        }

        public void Save(string fileName)
        {
            using (XmlTextWriter writer = new XmlTextWriter(fileName, null))
            {
                Save(writer);
            }
        }

        public void Save(TextWriter writer)
        {
            using (XmlTextWriter xmlWriter = new XmlTextWriter(writer))
            {
                Save(xmlWriter);
            }
        }

        public void Save(XmlWriter writer)
        {
            if (writer == null)
            {
                throw new ArgumentNullException("writer");
            }
            writer.WriteStartDocument();
            WriteContentTo(writer);
            writer.WriteEndDocument();
        }

        public override void WriteTo(XmlWriter writer)
        {
            WriteContentTo(writer);
        }
    }
}
