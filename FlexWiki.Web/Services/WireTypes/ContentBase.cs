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
using System.Xml.Serialization;

namespace FlexWiki.Web.Services.WireTypes
{
    [XmlType(Namespace = "http://www.flexwiki.com/webservices/")]
    public class ContentBase : BELObject
    {
        private DateTime created;
        private DateTime lastRead;
        private bool secure;
        private string description;
        private bool disableNamespaceEmoticons;
        private string contact;
        private string imageURL;
        private string homePage;
        private bool displaySpacesInWikiLinks;
        private string ns;
        private string title;

        public ContentBase()
        {
        }

        public ContentBase(FlexWiki.NamespaceManager manager)
        {
            //CA The LastRead and Created properties do not appear to be used anywhere else, 
            //CA so it was removed during the rearchitecture. We need to keep it 
            //CA for wire compatibility, so we just fake it. 
            this.Created = manager.Federation.TimeProvider.Now;
            this.LastRead = manager.Federation.TimeProvider.Now;
            //CA The secure propertyName went away at some point. Need to keep it around
            //CA for backwards compatibility with existing wire format. 
            this.Secure = false;
            this.Description = manager.Description;
            this.DisableNamespaceEmoticons = manager.DisableNamespaceEmoticons;
            this.Contact = manager.Contact;
            this.ImageURL = manager.ImageURL;
            this.HomePage = manager.HomePage;
            this.DisplaySpacesInWikiLinks = manager.DisplaySpacesInWikiLinks;
            this.Namespace = manager.Namespace;
            this.Title = manager.Title;
        }


        public DateTime Created
        {
            get { return created; }
            set { created = value; }
        }

        public DateTime LastRead
        {
            get { return lastRead; }
            set { lastRead = value; }
        }

        public bool Secure
        {
            get { return secure; }
            set { secure = value; }
        }

        public string Description
        {
            get { return description; }
            set { description = value; }
        }
        public bool DisableNamespaceEmoticons
        {
            get { return disableNamespaceEmoticons; }
            set { disableNamespaceEmoticons = value; }
        }
        public string Contact
        {
            get { return contact; }
            set { contact = value; }
        }

        public string ImageURL
        {
            get { return imageURL; }
            set { imageURL = value; }
        }

        public string HomePage
        {
            get { return homePage; }
            set { homePage = value; }
        }

        public bool DisplaySpacesInWikiLinks
        {
            get { return displaySpacesInWikiLinks; }
            set { displaySpacesInWikiLinks = value; }
        }

        public string Namespace
        {
            get { return ns; }
            set { ns = value; }
        }

        public string Title
        {
            get { return title; }
            set { title = value; }
        }

    }

}