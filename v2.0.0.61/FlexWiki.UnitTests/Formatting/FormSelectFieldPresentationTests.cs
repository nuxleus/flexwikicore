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
                @"<p><select name=""selectTest"" id=""selectTest"" size=""1""><option>one</option><option>two</option><option>three</option></select></p>
");
        }
        [Test]
        public void SelectFieldWithValuesTest()
        {
            FormatTest(
                @"@@Presentations.ComboSelectField(""selectTest"", [""one"", ""two"", ""three""], null, [1, 2, 3])@@",
                @"<p><select name=""selectTest"" id=""selectTest"" size=""1""><option value=""1"">one</option><option value=""2"">two</option><option value=""3"">three</option></select></p>
");
        }
        [Test]
        public void SelectFieldWithMismatchedValuesTest()
        {
            FormatTest(
                @"@@Presentations.ComboSelectField(""selectTest"", [""one"", ""two"", ""three""], null, [1, 2])@@",
                @"<p><span class=""ErrorMessage""><span class=""ErrorMessageBody"">The values array does not contain the same number of items as the options array
Parameter name: values</span></span></p>
");
        }
        [Test]
        public void SelectFieldEmptyOptionsTest()
        {
            FormatTest(
                @"@@Presentations.ComboSelectField(""selectTest"", [])@@",
                @"<p><select name=""selectTest"" id=""selectTest"" size=""1""></select></p>
");
        }
        [Test]
        public void SelectFieldSelectedOptionTest()
        {
            FormatTest(
                @"@@Presentations.ComboSelectField(""selectTest"", [""one"", ""two"", ""three""], ""two"")@@",
                @"<p><select name=""selectTest"" id=""selectTest"" size=""1""><option>one</option><option selected=""selected"">two</option><option>three</option></select></p>
");
        }
        [Test]
        public void SelectFieldSelectedValueTest()
        {
            FormatTest(
                @"@@Presentations.ComboSelectField(""selectTest"", [""one"", ""two"", ""three""], null, [1, 2, 3], 2)@@",
                @"<p><select name=""selectTest"" id=""selectTest"" size=""1""><option value=""1"">one</option><option value=""2"" selected=""selected"">two</option><option value=""3"">three</option></select></p>
");
        }

        [Test]
        public void FormCheckboxTest()
        {
            FormatTest(
                @"@@Presentations.Checkbox(""checkboxTest2"", ""99"", false)@@",
                @"<p><input type=""checkbox"" name=""checkboxTest2"" value=""99""/></p>
");
            FormatTest(
                @"@@Presentations.Checkbox(""checkboxTest"", ""999"", true, ""class='myChxbx'"")@@",
                @"<p><input type=""checkbox"" name=""checkboxTest"" value=""999"" class='myChxbx' checked=""true""/></p>
");
        }
        [Test]
        public void FormRadioButtonTest()
        {
            FormatTest(
                @"@@Presentations.Radio(""radioTest1"", ""421"", false)@@",
                @"<p><input type=""radio"" name=""radioTest1"" value=""421""/></p>
");

            FormatTest(
                @"@@Presentations.Radio(""radioTest"", ""42"", true, ""class='myRdbx'"")@@",
                @"<p><input type=""radio"" name=""radioTest"" value=""42"" class='myRdbx' checked=""true"" /></p>
");
        }
        [Test]
        public void FormLabelTest()
        {
            FormatTest(
                @"@@Presentations.Label(""forInputXY"", ""This is the label text"")@@",
                @"<p><label for=""forInputXY"">This is the label text</label></p>
");
            FormatTest(
                @"@@Presentations.Label(""forInputXY2"", ""This is the label text"", ""class='myLabels'"")@@",
                @"<p><label class='myLabels' for=""forInputXY2"">This is the label text</label></p>
");
        }
        [Test]
        public void FormInputFieldTest()
        {
            FormatTest(
                @"@@Presentations.InputField(""myText"", ""This is the default text"", 100, ""class='mytxt'"")@@",
                @"<p><input type=""text"" name=""myText"" id=""myText"" size=""100"" class='mytxt' value=""This is the default text"" /></p>
");
            FormatTest(
                @"@@Presentations.InputField(""myText"", ""This is the default text"")@@",
                @"<p><input type=""text"" name=""myText"" id=""myText"" value=""This is the default text"" /></p>
");
        }
        [Test]
        public void FormHiddenFieldTest()
        {
            FormatTest(
                @"@@Presentations.HiddenField(""myHidden"", ""param"", ""class='mytxt'"")@@",
                @"<p><input style=""display: none"" type=""text"" name=""myHidden"" value=""param"" class='mytxt' /></p>
");
            FormatTest(
                @"@@Presentations.HiddenField(""myHidden"", ""param"")@@",
                @"<p><input style=""display: none"" type=""text"" name=""myHidden"" value=""param"" /></p>
");
        }
        [Test]
        public void FormResetButtonTest()
        {
            FormatTest(
                @"@@Presentations.ResetButton(""myCncl"", ""Cancel"", ""class='mytxt'"")@@",
                @"<p><input type=""reset"" name=""myCncl"" class='mytxt' value=""Cancel"" /></p>
");
            FormatTest(
                @"@@Presentations.ResetButton(""myCncl"", ""Cancel"")@@",
                @"<p><input type=""reset"" name=""myCncl"" value=""Cancel"" /></p>
");
        }
        [Test]
        public void FormTextareaTest()
        {
            FormatTest(
                @"@@Presentations.Textarea(""txtName"", ""This is the label text"", 10, 40, ""class='myTxtarea'"")@@",
                @"<p><textarea name=""txtName"" rows=""10"" cols=""40"" class='myTxtarea'>This is the label text</textarea></p>
");
            FormatTest(
                @"@@Presentations.Textarea(""txtName"", ""This is the label text"", 80)@@",
                @"<p><textarea name=""txtName"" rows=""80"">This is the label text</textarea></p>
");

        }

        [Test]
        public void SelectFieldMultilineTest()
        {
            FormatTest(
                @"@@Presentations.ListSelectField(""selectTest"", 2, false, [""one"", ""two"", ""three""])@@",
                @"<p><select name=""selectTest"" id=""selectTest"" size=""2""><option>one</option><option>two</option><option>three</option></select></p>
");
        }
        [Test]
        public void SelectFieldMultilineMultiSelectTest()
        {
            FormatTest(
                @"@@Presentations.ListSelectField(""selectTest"", 2, true, [""one"", ""two"", ""three""])@@",
                @"<p><select name=""selectTest"" id=""selectTest"" size=""2"" multiple=""multiple""><option>one</option><option>two</option><option>three</option></select></p>
");
        }
        [Test]
        public void SelectFieldMultilineWithValuesTest()
        {
            FormatTest(
                @"@@Presentations.ListSelectField(""selectTest"", 2, true, [""one"", ""two"", ""three""], null, [1, 2, 3])@@",
                @"<p><select name=""selectTest"" id=""selectTest"" size=""2"" multiple=""multiple""><option value=""1"">one</option><option value=""2"">two</option><option value=""3"">three</option></select></p>
");
        }
        [Test]
        public void SelectFieldMultilineWithMismatchedValuesTest()
        {
            FormatTest(
                @"@@Presentations.ListSelectField(""selectTest"", 2, true, [""one"", ""two"", ""three""], null, [1, 2])@@",
                @"<p><span class=""ErrorMessage""><span class=""ErrorMessageBody"">The values array does not contain the same number of items as the options array
Parameter name: values</span></span></p>
");
        }
        [Test]
        public void SelectFieldMultilineEmptyOptionsTest()
        {
            FormatTest(
                @"@@Presentations.ListSelectField(""selectTest"", 2, true, [])@@",
                @"<p><select name=""selectTest"" id=""selectTest"" size=""2"" multiple=""multiple""></select></p>
");
        }
        [Test]
        public void SelectFieldMultilineSelectedOptionTest()
        {
            FormatTest(
                @"@@Presentations.ListSelectField(""selectTest"", 2, true, [""one"", ""two"", ""three""], ""two"")@@",
                @"<p><select name=""selectTest"" id=""selectTest"" size=""2"" multiple=""multiple""><option>one</option><option selected=""selected"">two</option><option>three</option></select></p>
");
        }
        [Test]
        public void SelectFieldMultilineSelectedValueTest()
        {
            FormatTest(
                @"@@Presentations.ListSelectField(""selectTest"", 2, true, [""one"", ""two"", ""three""], null, [1, 2, 3], 2)@@",
                @"<p><select name=""selectTest"" id=""selectTest"" size=""2"" multiple=""multiple""><option value=""1"">one</option><option value=""2"" selected=""selected"">two</option><option value=""3"">three</option></select></p>
");
        }

    }
}
