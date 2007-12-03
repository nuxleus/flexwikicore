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
using System.Xml.XPath;
using System.Collections.Generic;
using System.Text;

namespace FlexWiki
{
    public class WomXPathNavigator : XPathNavigator
    {
        // Fields

        private WomElement _element;
        private int _elementIndex;
        private int _propertyIndex;

        // Constructors

        public WomXPathNavigator(WomElement element)
        {
            this._element = element;
            this._elementIndex = -1;
            this._propertyIndex = -1;
        }

        // Properties

        public override string BaseURI
        {
            get { return String.Empty; }
        }

        public override bool IsEmptyElement
        {
            get { return _element.HasElements; }
        }

        public override string LocalName
        {
            get
            {
                if (_propertyIndex >= 0)
                {
                    return _element.Properties.GetName(_propertyIndex);
                }
                return _element.Name;
            }
        }

        public override string Name
        {
            get { return LocalName; }
        }

        public override XmlNameTable NameTable
        {
            get { return WomNameTable.Instance; }
        }

        public override string NamespaceURI
        {
            get { return String.Empty; }
        }

        public override XPathNodeType NodeType
        {
            get
            {
                if (_propertyIndex >= 0)
                {
                    return XPathNodeType.Attribute;
                }
                if (this._element is WomDocument)
                {
                    return XPathNodeType.Root;
                }
                if (_element.IsTextElement)
                {
                    return XPathNodeType.Text;
                }
                return XPathNodeType.Element;
            }
        }

        public override string Prefix
        {
            get { return String.Empty; }
        }

        public override string Value
        {
            get
            {
                if (_propertyIndex >= 0)
                {
                    return _element.Properties[_propertyIndex];
                }
                return ""; //TODO: m_element.Value;
            }
        }

        // Methods

        public override XPathNavigator Clone()
        {
            WomXPathNavigator navigator = new WomXPathNavigator(_element);
            navigator._elementIndex = this._elementIndex;
            navigator._propertyIndex = this._propertyIndex;
            return navigator;
        }

        public override bool IsSamePosition(XPathNavigator other)
        {
            WomXPathNavigator womNavigator = other as WomXPathNavigator;
            if (womNavigator != null)
            {
                return (_element == womNavigator._element
                    && _propertyIndex == womNavigator._propertyIndex);
            }
            return false;
        }

        public override bool MoveTo(XPathNavigator other)
        {
            WomXPathNavigator womNavigator = other as WomXPathNavigator;
            if (womNavigator != null)
            {
                _element = womNavigator._element;
                _propertyIndex = womNavigator._propertyIndex;
                return true;
            }
            return false;
        }

        public override bool MoveToFirstAttribute()
        {
            if (_propertyIndex < 0)
            {
                if (_element.HasProperties)
                {
                    _propertyIndex = 0;
                    return true;
                }
            }
            return false;
        }

        public override bool MoveToFirstChild()
        {
            if (_propertyIndex < 0)
            {
                WomElement element = this._element.FirstChild;
                if (element != null)
                {
                    this._element = element;
                    this._elementIndex = 0;
                    return true;
                }
            }
            return false;
        }

        public override bool MoveToFirstNamespace(XPathNamespaceScope namespaceScope)
        {
            return false;
        }

        public override bool MoveToId(string id)
        {
            return false;
        }

        public override bool MoveToNext()
        {
            if (_propertyIndex < 0)
            {
                int index = GetElementIndex();
                if (index >= 0 && ++index < this._element._parent.ElementList.Count)
                {
                    this._element = this._element._parent.ElementList[index];
                    this._elementIndex = index;
                    return true;
                }
            }
            return false;
        }

        public override bool MoveToNextAttribute()
        {
            if (_propertyIndex >= 0)
            {
                if (_propertyIndex < _element.Properties.Count - 1)
                {
                    _propertyIndex++;
                    return true;
                }
            }
            return false;
        }

        public override bool MoveToNextNamespace(XPathNamespaceScope namespaceScope)
        {
            return false;
        }

        public override bool MoveToParent()
        {
            if (_propertyIndex >= 0)
            {
                _propertyIndex = -1;
                return false;
            }
            WomElement element = this._element.Parent;
            if (element != null)
            {
                this._element = element;
                this._elementIndex = -1;
                return true;
            }
            return false;
        }

        public override bool MoveToPrevious()
        {
            if (_propertyIndex < 0)
            {
                int index = GetElementIndex();
                if (index >= 0 && --index >= 0)
                {
                    this._element = this._element._parent.ElementList[index];
                    this._elementIndex = index;
                    return true;
                }
            }
            return false;
        }

        private int GetElementIndex()
        {
            if (_elementIndex < 0)
            {
                if (_element._parent != null)
                {
                    WomElement parent = _element._parent;
                    for (int i = 0; i < parent.ElementList.Count; i++)
                    {
                        if (_element == parent.ElementList[i])
                        {
                            _elementIndex = i;
                            break;
                        }
                    }
                }
            }
            return _elementIndex;
        }
    }
}
