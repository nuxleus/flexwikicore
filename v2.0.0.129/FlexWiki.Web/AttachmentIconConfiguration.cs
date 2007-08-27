using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Serialization;

namespace FlexWiki.Web
{
    public class AttachmentIconConfiguration
    {
 		private string _href;
		private string _iconKey;

 		/// <summary>
		/// HREF to icon to be linked
		/// </summary>
		[XmlAttribute]
		public string Href
		{
			get { return _href; }
			set 
			{ 
				_href = value;
			}
		}

		/// <summary>
		/// IconKey of icon, i.e. what attachment types is it used for
		/// </summary>
		[XmlAttribute]
		public string IconKey
		{
			get { return _iconKey; }
			set 
			{
                _iconKey = value;
			}
		}

  }
}
