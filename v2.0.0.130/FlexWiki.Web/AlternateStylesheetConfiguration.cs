using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Serialization;

namespace FlexWiki.Web
{
	public class AlternateStylesheetConfiguration
	{
		private string _href;
		private string _title;
		private bool _explicitTitle;

		/// <summary>
		/// HREF to linked stylesheet
		/// </summary>
		[XmlAttribute]
		public string Href
		{
			get { return _href; }
			set 
			{ 
				_href = value;
				// reset the "auto" title
				if (!_explicitTitle) _title = null;
			}
		}

		/// <summary>
		/// Title of alternate stylesheet
		/// </summary>
		[XmlAttribute]
		public string Title
		{
			get 
			{ 
				if (_title != null) return _title;
				// try to create a title based on the css file name
				if (_href == null) return null;
				int epos = _href.LastIndexOf(".");
				if (epos == -1) epos = _href.Length - 1;
				int spos = _href.LastIndexOf("/");
				if (spos == -1) spos = 0;
				else spos = spos +1; // eat "/";
				_title = _href.Substring(spos, epos - spos);
				return _title;
			}
			set 
			{ 
				_title = value;
				_explicitTitle = true;
			}
		}

	}
}
