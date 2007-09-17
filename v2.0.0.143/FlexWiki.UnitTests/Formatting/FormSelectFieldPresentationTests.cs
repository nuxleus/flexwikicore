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

using NUnit.Framework;

using FlexWiki;
using FlexWiki.Collections;
using FlexWiki.Formatting;

namespace FlexWiki.UnitTests.Formatting
{
    [TestFixture]
    public class FormSelectFieldPresentationTests : FormattingTestsBase
    {        
        [Test]
        public void SelectFieldTest()
        {
            FormatTest(
                @"@@Presentations.ComboSelectField(""selectTest"", [""one"", ""two"", ""three""])@@",
                @"<select name=""selectTest"" id=""selectTest"" size=""1""><option>one</option><option>two</option><option>three</option></select>
");
        }
        [Test]
        public void SelectFieldWithValuesTest()
        {
            FormatTest(
                @"@@Presentations.ComboSelectField(""selectTest"", [""one"", ""two"", ""three""], null, [1, 2, 3])@@",
                @"<select name=""selectTest"" id=""selectTest"" size=""1""><option value=""1"">one</option><option value=""2"">two</option><option value=""3"">three</option></select>
");
        }
        [Test]
        public void SelectFieldWithMismatchedValuesTest()
        {
            FormatTest(
                @"@@Presentations.ComboSelectField(""selectTest"", [""one"", ""two"", ""three""], null, [1, 2])@@",
                @"<span class=""ErrorMessage""><span class=""ErrorMessageBody"">The values array does not contain the same number of items as the options array
Parameter name: values</span></span>
");
        }
        [Test]
        public void SelectFieldEmptyOptionsTest()
        {
            FormatTest(
                @"@@Presentations.ComboSelectField(""selectTest"", [])@@",
                @"<select name=""selectTest"" id=""selectTest"" size=""1""></select>
");
        }
        [Test]
        public void SelectFieldSelectedOptionTest()
        {
            FormatTest(
                @"@@Presentations.ComboSelectField(""selectTest"", [""one"", ""two"", ""three""], ""two"")@@",
                @"<select name=""selectTest"" id=""selectTest"" size=""1""><option>one</option><option selected=""selected"">two</option><option>three</option></select>
");
        }
        [Test]
        public void SelectFieldSelectedValueTest()
        {
            FormatTest(
                @"@@Presentations.ComboSelectField(""selectTest"", [""one"", ""two"", ""three""], null, [1, 2, 3], 2)@@",
                @"<select name=""selectTest"" id=""selectTest"" size=""1""><option value=""1"">one</option><option value=""2"" selected=""selected"">two</option><option value=""3"">three</option></select>
");
        }

        [Test]
        public void FormCheckboxTest()
        {
            FormatTest(
                @"@@Presentations.Checkbox(""checkboxTest2"", ""99"", false)@@",
                @"<input type=""checkbox"" name=""checkboxTest2"" value=""99""/>
");
            FormatTest(
                @"@@Presentations.Checkbox(""checkboxTest"", ""999"", true, ""class='myChxbx'"")@@",
                @"<input type=""checkbox"" name=""checkboxTest"" value=""999"" class='myChxbx' checked=""true""/>
");
        }
        [Test]
        public void FormRadioButtonTest()
        {
            FormatTest(
                @"@@Presentations.Radio(""radioTest1"", ""421"", false)@@",
                @"<input type=""radio"" name=""radioTest1"" value=""421""/>
");

            FormatTest(
                @"@@Presentations.Radio(""radioTest"", ""42"", true, ""class='myRdbx'"")@@",
                @"<input type=""radio"" name=""radioTest"" value=""42"" class='myRdbx' checked=""true"" />
");
        }
        [Test]
        public void FormLabelTest()
        {
            FormatTest(
                @"@@Presentations.Label(""forInputXY"", ""This is the label text"")@@",
                @"<label for=""forInputXY"">This is the label text</label>
");
            FormatTest(
                @"@@Presentations.Label(""forInputXY2"", ""This is the label text"", ""class='myLabels'"")@@",
                @"<label class='myLabels' for=""forInputXY2"">This is the label text</label>
");
        }
        [Test]
        public void FormInputFieldTest()
        {
            FormatTest(
                @"@@Presentations.InputField(""myText"", ""This is the default text"", 100, ""class='mytxt'"")@@",
                @"<input type=""text"" name=""myText"" id=""myText"" size=""100"" class='mytxt' value=""This is the default text"" />
");
            FormatTest(
                @"@@Presentations.InputField(""myText"", ""This is the default text"")@@",
                @"<input type=""text"" name=""myText"" id=""myText"" value=""This is the default text"" />
");
        }
        [Test]
        public void FormHiddenFieldTest()
        {
            FormatTest(
                @"@@Presentations.HiddenField(""myHidden"", ""param"", ""class='mytxt'"")@@",
                @"<input style=""display: none"" type=""text"" name=""myHidden"" value=""param"" class='mytxt' />
");
            FormatTest(
                @"@@Presentations.HiddenField(""myHidden"", ""param"")@@",
                @"<input style=""display: none"" type=""text"" name=""myHidden"" value=""param"" />
");
        }
        [Test]
        public void FormResetButtonTest()
        {
            FormatTest(
                @"@@Presentations.ResetButton(""myCncl"", ""Cancel"", ""class='mytxt'"")@@",
                @"<input type=""reset"" name=""myCncl"" class='mytxt' value=""Cancel"" />
");
            FormatTest(
                @"@@Presentations.ResetButton(""myCncl"", ""Cancel"")@@",
                @"<input type=""reset"" name=""myCncl"" value=""Cancel"" />
");
        }
        [Test]
        public void FormTextareaTest()
        {
            FormatTest(
                @"@@Presentations.Textarea(""txtName"", ""This is the label text"", 10, 40, ""class='myTxtarea'"")@@",
                @"<textarea name=""txtName"" rows=""10"" cols=""40"" class='myTxtarea'>This is the label text</textarea>
");
            FormatTest(
                @"@@Presentations.Textarea(""txtName"", ""This is the label text"", 80)@@",
                @"<textarea name=""txtName"" rows=""80"">This is the label text</textarea>
");

        }

        [Test]
        public void SelectFieldMultilineTest()
        {
            FormatTest(
                @"@@Presentations.ListSelectField(""selectTest"", 2, false, [""one"", ""two"", ""three""])@@",
                @"<select name=""selectTest"" id=""selectTest"" size=""2""><option>one</option><option>two</option><option>three</option></select>
");
        }
        [Test]
        public void SelectFieldMultilineMultiSelectTest()
        {
            FormatTest(
                @"@@Presentations.ListSelectField(""selectTest"", 2, true, [""one"", ""two"", ""three""])@@",
                @"<select name=""selectTest"" id=""selectTest"" size=""2"" multiple=""multiple""><option>one</option><option>two</option><option>three</option></select>
");
        }
        [Test]
        public void SelectFieldMultilineWithValuesTest()
        {
            FormatTest(
                @"@@Presentations.ListSelectField(""selectTest"", 2, true, [""one"", ""two"", ""three""], null, [1, 2, 3])@@",
                @"<select name=""selectTest"" id=""selectTest"" size=""2"" multiple=""multiple""><option value=""1"">one</option><option value=""2"">two</option><option value=""3"">three</option></select>
");
        }
        [Test]
        public void SelectFieldMultilineWithMismatchedValuesTest()
        {
            FormatTest(
                @"@@Presentations.ListSelectField(""selectTest"", 2, true, [""one"", ""two"", ""three""], null, [1, 2])@@",
                @"<span class=""ErrorMessage""><span class=""ErrorMessageBody"">The values array does not contain the same number of items as the options array
Parameter name: values</span></span>
");
        }
        [Test]
        public void SelectFieldMultilineEmptyOptionsTest()
        {
            FormatTest(
                @"@@Presentations.ListSelectField(""selectTest"", 2, true, [])@@",
                @"<select name=""selectTest"" id=""selectTest"" size=""2"" multiple=""multiple""></select>
");
        }
        [Test]
        public void SelectFieldMultilineSelectedOptionTest()
        {
            FormatTest(
                @"@@Presentations.ListSelectField(""selectTest"", 2, true, [""one"", ""two"", ""three""], ""two"")@@",
                @"<select name=""selectTest"" id=""selectTest"" size=""2"" multiple=""multiple""><option>one</option><option selected=""selected"">two</option><option>three</option></select>
");
        }
        [Test]
        public void SelectFieldMultilineSelectedValueTest()
        {
            FormatTest(
                @"@@Presentations.ListSelectField(""selectTest"", 2, true, [""one"", ""two"", ""three""], null, [1, 2, 3], 2)@@",
                @"<select name=""selectTest"" id=""selectTest"" size=""2"" multiple=""multiple""><option value=""1"">one</option><option value=""2"" selected=""selected"">two</option><option value=""3"">three</option></select>
");
        }

    }
}
