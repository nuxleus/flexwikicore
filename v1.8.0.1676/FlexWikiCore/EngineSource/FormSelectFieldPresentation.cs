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
using System.Collections;
using FlexWiki;
using FlexWiki.Formatting;

namespace FlexWiki
{
	/// <summary>
	/// Presents a form select field.
	/// </summary>
	[ExposedClass("FormSelectFieldPresentation", "Presents a form select field")]
	public class FormSelectFieldPresentation : FlexWiki.PresentationPrimitive
	{
		#region Class data
		private string _FieldName;
		private int _Size;
		private bool _Multiple;
		private ArrayList _Options;
		private ArrayList _Values;
		private string _SelectedOption;
		private object _SelectedValue;
		#endregion

		#region Constructors
		public FormSelectFieldPresentation(string fieldName, int size, bool multiple, 
			ArrayList options, string selectedOption, 
			ArrayList values, object selectedValue)
		{
			_FieldName = fieldName;
			_Size = size;
			_Multiple = multiple;
			_Options = options;
			_SelectedOption = selectedOption;
			_Values = values;
			_SelectedValue = selectedValue;
		}
		#endregion

		#region Public Properties
		public string FieldName
		{
			get { return _FieldName; }
		}

		public int Size
		{
			get { return _Size; }
		}

		public bool Multiple
		{
			get { return _Multiple; }
		}

		public ArrayList Options
		{
			get { return _Options; }
		}

		public string SelectedOption
		{
			get { return _SelectedOption; }
		}

		public ArrayList Values
		{
			get { return _Values; } 
		}

		public object SelectedValue
		{
			get { return _SelectedValue; }
		}
		#endregion

		#region Presentation overrides
		public override void OutputTo(WikiOutput output)
		{
			output.FormSelectField(FieldName, Size, Multiple, Options, SelectedOption, Values, SelectedValue);
		}
		#endregion
	}
}
