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
using System.Xml;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Globalization;

namespace FlexWiki
{
    public class WomElement
    {
        // Fields

        internal object _content;
        internal int _length;
        internal string _name;
        internal WomElement _parent;
        internal WomPropertyCollection _properties;
        internal int _start;

        //Constructors

        public WomElement(string name)
        {
            if (name == null)
            {
                throw new ArgumentNullException("name");
            }
            this._name = WomNameTable.Instance.Add(name);
        }

        public WomElement(string name, params object[] contents)
            : this(name)
        {
            Add(contents);
        }

        public WomElement(WomElement other)
        {
            if (other == null)
            {
                throw new ArgumentNullException("other");
            }

            this._name = other._name;

            WomPropertyCollection attrs = other._properties;
            if (attrs != null)
            {
                this._properties = new WomPropertyCollection(attrs);
            }

            if (other._content is string)
            {
                this._content = other._content;
            }
            else
            {
                WomElementCollection otherElementList = other._content as WomElementCollection;
                if (otherElementList != null)
                {
                    WomElementCollection elementList = ElementList;
                    for (int i = 0; i < otherElementList.Count; i++)
                    {
                        elementList.Add(new WomElement(otherElementList[i]));
                    }
                }
            }
        }

        public WomElement(XmlReader reader)
        {
            if (reader == null)
            {
                throw new ArgumentNullException("reader");
            }
            if (reader.NodeType != XmlNodeType.Element)
            {
                throw new InvalidOperationException();
            }
            bool sameNameTable = (reader.NameTable == WomNameTable.Instance);
            if (sameNameTable)
            {
                this._name = reader.LocalName;
            }
            else
            {
                this._name = WomNameTable.Instance.Add(reader.LocalName);
            }

            if (reader.HasAttributes)
            {
                this._properties = new WomPropertyCollection(reader.AttributeCount);
                for (int i = 0; i < reader.AttributeCount; i++)
                {
                    reader.MoveToAttribute(i);
                    this._properties.Add(new WomProperty(reader.LocalName, reader.Value));
                }
                reader.MoveToElement();
            }

            if (!reader.IsEmptyElement)
            {
                reader.Read();
                ReadElementContentFrom(reader);
                if (reader.NodeType != XmlNodeType.EndElement)
                {
                    throw new InvalidOperationException();
                }
                if (_content == null)
                {
                    _content = String.Empty;
                }
            }
        }

        internal WomElement()
        {
        }

        // Properties

        public WomElementCollection ElementList
        {
            get
            {
                WomElementCollection list = _content as WomElementCollection;
                if (list == null)
                {
                    list = new WomElementCollection(this);
                    _content = list;
                }
                return list;
            }
        }

        public WomElement FirstChild
        {
            get
            {
                WomElementCollection list = _content as WomElementCollection;
                if (list != null && list.Count > 0)
                {
                    return list[0];
                }
                return null;
            }
        }

        public bool HasElements
        {
            get
            {
                WomElementCollection list = _content as WomElementCollection;
                return (list != null && list.Count > 0);
            }
        }

        public bool HasProperties
        {
            get { return (this._properties != null && _properties.Count > 0); }
        }

        public int Index
        {
            get
            {
                if (_parent != null)
                {
                    return _parent.ElementList.IndexOf(this);
                }
                return -1;
            }
        }

        public bool IsTextElement
        {
            get { return (_name == "_text"); }
        }

        public WomElement LastChild
        {
            get
            {
                WomElementCollection list = _content as WomElementCollection;
                if (list != null && list.Count > 0)
                {
                    return list[list.Count - 1];
                }
                return null;
            }
        }

        public int Length
        {
            get { return this._length; }
            set { this._length = value; }
        }

        public string Name
        {
            get { return this._name; }
        }

        public WomElement Parent
        {
            get { return this._parent; }
        }

        public WomPropertyCollection Properties
        {
            get
            {
                if (this._properties == null)
                {
                    this._properties = new WomPropertyCollection();
                }
                return this._properties;
            }
        }

        public int Start
        {
            get { return this._start; }
            set { this._start = value; }
        }

        public string Xml
        {
            get
            {
                StringWriter textWriter = new StringWriter(CultureInfo.CurrentCulture);
                using (XmlTextWriter xmlWriter = new XmlTextWriter(textWriter))
                {
                    WriteTo(xmlWriter);
                }
                return textWriter.ToString();
            }
        }

        // Methods

        public void Add(params object[] contents)
        {
            if (contents == null)
            {
                throw new ArgumentNullException("contents");
            }
            for (int i = 0; i < contents.Length; i++)
            {
                object item = contents[i];
                if (item is string)
                {
                    WomElement textElement = new WomElement("_text");
                    textElement._content = item;
                    ElementList.Add(textElement);
                    continue;
                }
                WomElement element = item as WomElement;
                if (element != null)
                {
                    ElementList.Add(element);
                    continue;
                }
                WomProperty attribute = item as WomProperty;
                if (attribute != null)
                {
                    Properties.Add(attribute);
                    continue;
                }
                throw new ArgumentException(
                    String.Format(CultureInfo.CurrentCulture, "Wrong argument type as {0} position.", i));
            }
        }

        public override bool Equals(object obj)
        {
            WomElement other = obj as WomElement;
            if (other == null)
            {
                return false;
            }
            if ((object)this._name != (object)other._name)
            {
                return false;
            }
            if (_properties != null)
            {
                if (!this._properties.Equals(other._properties))
                {
                    return false;
                }
            }
            else if (other._properties != null && other._properties.Count > 0)
            {
                return false;
            }
            if (this._content != null)
            {
                if (!this._content.Equals(other._content))
                {
                    return false;
                }
            }
            else if (other._content != null
                && other._content is WomElementCollection && (other._content as WomElementCollection).Count > 0)
            {
                return false;
            }
            return true;
        }

        public override int GetHashCode()
        {
            int hashCode = _name.GetHashCode();
            if (_properties != null)
            {
                hashCode ^= _properties.GetHashCode();
            }
            if (_content != null)
            {
                hashCode ^= _content.GetHashCode();
            }
            return hashCode;
        }

        public static WomElement Load(XmlReader reader)
        {
            if (reader == null)
            {
                throw new ArgumentNullException("reader");
            }
            reader.MoveToContent();
            WomElement element = new WomElement(reader);
            reader.Read();
            reader.MoveToContent();
            if (!reader.EOF)
            {
                throw new InvalidOperationException();
            }
            return element;
        }

        public virtual void ReadContentFrom(XmlReader reader)
        {
            RemoveContent();
            ReadElementContentFrom(reader);
        }

        public void RemoveContent()
        {
            WomElementCollection elements = _content as WomElementCollection;
            if (elements != null)
            {
                elements.Clear();
            }
            _content = null;
        }

        public override string ToString()
        {
            return _name + ": " + base.ToString();
        }

        public void WriteContentTo(XmlWriter writer)
        {
            if (writer == null)
            {
                throw new ArgumentNullException("writer");
            }
            string text = _content as string;
            if (text != null)
            {
                writer.WriteString(text);
            }
            else
            {
                WomElementCollection elementList = _content as WomElementCollection;
                if (elementList != null)
                {
                    elementList.WriteTo(writer);
                }
            }
        }

        public virtual void WriteTo(XmlWriter writer)
        {
            if (writer == null)
            {
                throw new ArgumentNullException("writer");
            }
            if (_name == "_text")
            {
                writer.WriteString(_content as string);
            }
            else
            {
                writer.WriteStartElement(_name);
                if (_properties != null)
                {
                    _properties.WriteTo(writer);
                }
                WomElementCollection elementList = _content as WomElementCollection;
                if (elementList != null)
                {
                    elementList.WriteTo(writer);
                }
                writer.WriteEndElement();
            }
        }

        internal void AddElement(WomElement element)
        {
            if (element._parent != null)
            {
                element = new WomElement(element);
            }
            else
            {
                WomElement ancestorOrSelf = this;
                while (ancestorOrSelf != null)
                {
                    if (element == ancestorOrSelf)
                    {
                        throw new ArgumentException();
                    }
                    ancestorOrSelf = ancestorOrSelf._parent;
                }
            }
            //TODO:
            //string text2 = this.content as string;
            //if (text2 != null)
            //{
            //    if (text2.Length == 0)
            //    {
            //        this.content = null;
            //    }
            //    else
            //    {
            //        XText text3 = new XText(text2);
            //        text3.parent = this;
            //        text3.next = text3;
            //        this.content = text3;
            //    }
            //}
            ElementList.Add(element);
        }

        internal string GetText(string text)
        {
            //if (ElementList.Count == 0)
            //{
            return text.Substring(_start, _length);
            //}
            //throw new Exception("The method or operation is not implemented.");
        }

        private void AddString(string value)
        {
            WomElement text = new WomElement("_text");
            text._content = value;
            Add(text);
        }

        private void ReadElementContentFrom(XmlReader reader)
        {
            bool process = true;
            while (process)
            {
                switch (reader.NodeType)
                {
                    case XmlNodeType.Element:
                        AddElement(new WomElement(reader));
                        break;

                    case XmlNodeType.Text:
                    case XmlNodeType.CDATA:
                    case XmlNodeType.SignificantWhitespace:
                        AddString(reader.Value);
                        break;

                    case XmlNodeType.ProcessingInstruction:
                    case XmlNodeType.Comment:
                    case XmlNodeType.Whitespace:
                        break; // Ignore these elements

                    default:
                        process = false; // cannot take these elements as content
                        continue;
                }
                reader.Read();
            }
        }
    }
}
