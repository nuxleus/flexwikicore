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
	[ExposedClass("ThumbnailPresentation", "Presents a thumbnail")]
	public class ThumbnailPresentation : PresentationPrimitive
	{
		private string _title;
		private string _titleColour;
		private string _URL;
		private string _imageURL;
		private int    _size;
		private string _borderColour;
		private string _borderWidth;
		private string _borderStyle;
		private bool   _clickable;

		public ThumbnailPresentation(string title, string titleColour, string URL, string imageURL, string size, string borderColour, string borderWidth, string borderStyle, string clickable)
		{
			Init(title, titleColour, URL, imageURL, size, borderColour, borderWidth, borderStyle, clickable);
		}
		private void Init(string title, string titleColour, string URL, string imageURL, string size, string borderColour, string borderWidth, string borderStyle, string clickable)
		{
			_title        = title;
			_titleColour  = titleColour ;
			_URL          = URL ;
   	        _imageURL = imageURL;
			if(size.ToLower().Equals("small"))
				_size = 72 ;
			else if(size.ToLower().Equals("medium"))
				_size = 144 ;
			else if(size.ToLower().Equals("large"))
				_size = 288 ;
			else
				_size = Convert.ToInt16(size) ;
			_borderColour = borderColour ;
			_borderWidth  = borderWidth ;
			_borderStyle  = borderStyle ;
			if(clickable.ToLower().Equals("true"))
			   _clickable = true ;
			else
			   _clickable = false ;
		}

		string Title
		{
			get
			{
				return _title;
			}
		}

		string TitleColour
		{
			get
			{
				return _titleColour;
			}
		}

		string imageURL
		{
			get
			{
				return _imageURL;
			}
		}

		string URL
		{
			get
			{
				return _URL;
			}
		}
		
		int size
		{
			get
			{
				return _size;
			}
		}
		
		string BorderColour
		{
			get
			{
				return _borderColour;
			}
		}	

		string BorderWidth
		{
			get
			{
				return _borderWidth;
			}
		}	

		string BorderStyle
		{
			get
			{
				return _borderStyle;
			}
		}	

		bool Clickable
		{
			get
			{
				return _clickable;
			}
		}	
		
		public override void OutputTo(WikiOutput output)
		{
 			output.WriteThumbnail(_title,_titleColour,_URL,_imageURL, _size, _borderColour, _borderWidth, _borderStyle,_clickable);
		}
	}
}
