  // Copyright (c) Microsoft Corporation.  All rights reserved.
  //
  // The use and distribution terms for this software are covered by the 
  // Common Public License 1.0 (http://opensource.org/licenses/cpl.php)
  // which can be found in the file CPL.TXT at the root of this distribution.
  // By using this software in any fashion, you are agreeing to be bound by 
  // the terms of this license.
  //
  // You must not remove this notice, or any other, from this software.

function attachFile_OnClick()
{
    var insertText1 = "";
    var insertText2 = "";
    var insertText3 = "";
    var insertText4 = "";
    var textArea = document.forms["Form1"].EditBox;
    var docTitle = document.getElementById("Form3DocTitle").value;
    var urlString = document.getElementById("_fileUrl").value;
    var iconUrlString = document.getElementById("_fileIconUrl").value;
    var sizeString = document.getElementById("_fileSize").value;
    var filedDate = document.getElementById("Form3Filed").value;
    var publishedDate = document.getElementById("Form3Published").value;
    var authorName = document.getElementById("Form3Author").value;
    var fileNo = document.getElementById("Form3FileNo").value;
    var versionNo = document.getElementById("Form3Version").value;
    var commentText = document.getElementById("Form3Comment").value;
    var attachFormatString = getCheckedValue(document.forms["Form3"].attachFormat);
    var statusDate = document.getElementById("Form3StatusDate").value;
    var templateCombo = document.getElementById("Form3Status");
	if ((templateCombo) && (templateCombo.selectedIndex > -1))
	{
        var statusString = templateCombo.options[templateCombo.selectedIndex].value;
    }
    if (attachFormatString == "Normal")
    {
        insertText1 = '@@Presentations.Image(federation.LinkMaker.LinkToImage("' + iconUrlString + '"))@@ '; 
        insertText2 = '@@Presentations.Link("' + urlString + '", "' + docTitle + '", "' + sizeString + '")@@';
    }
    if (attachFormatString == "Folder")
    {
        insertText1 = '|| ' + filedDate + ' || @@Presentations.Image(federation.LinkMaker.LinkToImage("' + iconUrlString + '"))@@ ';
        insertText2 = '@@Presentations.Link("' + urlString + '", "' + docTitle + '", "' + sizeString + '")@@ || ';
        insertText3 = fileNo + ' || ' + authorName + ' || ' + publishedDate + ' || ' + commentText + ' ||';
    }
    if (attachFormatString == "DocMan")
    {
        insertText1 = '|| ' + filedDate + ' || @@Presentations.Image(federation.LinkMaker.LinkToImage("' + iconUrlString + '"))@@ ';
        insertText2 = '@@Presentations.Link("' + urlString + '", "' + docTitle + '", "' + sizeString + '")@@ || ';
        insertText3 = fileNo + ' || ' + authorName + ' || ' + statusString + ' || ' + statusDate + ' || ';
        insertText4 = versionNo +  ' || @@Presentations.Checkbox("checkOut_' + statusString + '_' + versionNo + '", "strUrl", false)@@ ||' + commentText + ' ||';
    }
    textArea.value += "\n\r" + insertText1 + insertText2 + insertText3 + insertText4;
    textArea.focus();
    document.forms["Form3"].PostBox.value = textArea.value;
    document.getElementById("_processAttachment").value = "IsAttachment";
}

function CalcEditBoxHeight()
{
	var answer = CalcEditZoneHeight();
	return answer;
}

function CalcEditZoneHeight()
{
	var answer = MainHeight();
	return answer;
}

function Cancel()
{
	history.back();
}

function ChangeTemplate(selectId)
{
	var templateCombo = document.getElementById(selectId);
	if ((templateCombo) && (templateCombo.selectedIndex > -1))
	{
		var strInsertText = templateCombo.options[templateCombo.selectedIndex].value;
		
		var objTextArea = document.forms['Form1'].EditBox;
		if (document.selection && document.selection.createRange)
		{
			objTextArea.focus();
			var objSelectedTextRange = document.selection.createRange();
			var strSelectedText = objSelectedTextRange.text;
			objSelectedTextRange.text = strInsertText + strSelectedText;
		}
		else
		{
			objTextArea.value += strInsertText;
			objTextArea.focus();
		}
	}
	var s = document.forms["Form1"].EditBox.value;
	document.forms["Form3"].PostBox.value = s;
}

function Document_OnKeyPress(event)
{
	if (event != null) // FireFox only
	{
		if (event.keyCode == 9)
		{
			textArea = document.forms["Form1"].EditBox;
			selStart = textArea.selectionStart;
			selEnd = textArea.selectionEnd;
			selTop = textArea.scrollTop;
			textArea.value = textArea.value.substring(0, selStart) + String.fromCharCode(9) + textArea.value.substring(selEnd, textArea.textLength);
			textArea.selectionStart = selEnd + 1;
			textArea.selectionEnd = selEnd + 1;
			textArea.scrollTop = selTop;
			return false;
		}
	}
	return true;
}

function FileUploadSend_OnClick()
{
	var s = document.forms["Form1"].EditBox.value;
	document.forms["Form3"].PostBox.value = s;
    document.getElementById("_processAttachment").value = "IsAttachment";
	document.getElementById("Form3").submit();
}

// return the value of the radio button that is checked
// return an empty string if none are checked, or
// there are no radio buttons
function getCheckedValue(radioObj) {
	if(!radioObj)
	{
		return "";
	}
	var radioLength = radioObj.length;
	if(radioLength == undefined)
	{
		if(radioObj.checked)
		{
			return radioObj.value;
		}
		else
		{
			return "";
		}
	}
	for(var i = 0; i < radioLength; i++) {
		if(radioObj[i].checked) {
			return radioObj[i].value;
		}
	}
	return "";
}

function ItemDisplay(itemId, display)
{
    var a = document.getElementById(itemId);
    if (display)
    {
        a.style.display = 'block';
    }
    else
    {
        a.style.display = 'none';
    }
}

function MainHeight()
{
	var answer = document.body.clientHeight;
	var e;
	return answer;
}

function MainWidth()
{
	var answer = document.body.clientWidth;
	var e;

	e = document.getElementById("Sidebar");
	if (e != null)
	{
		answer -= e.scrollWidth;
	}
	return answer;
}

function preview()
{
	var s = document.forms["Form1"].EditBox.value;
	document.forms["Form2"].body.value = s;
	window.open('about:blank', 'previewWindow');
	document.forms["Form2"].submit();
}

function ResizeEditBox()
{
    var height = CalcEditBoxHeight();
    var editBox = document.getElementById("EditBox");
    if (null != editBox)
    {
        editBox.style.height = Math.max(100, height) + "px";
    }
}

function Save()
{
	SetUserName();
	SetCaptcha();
    document.getElementById("_processAttachment").value = "IsNotAttachment";
    document.getElementById("SaveButtonPressed").value = "Save"; 
	var r = document.getElementById("ReturnTopic");
	if (r != null)
	{
		r.value = ""; // prevent return action by emptying this out
	}
	document.getElementById("Form1").submit();
}

function SaveAndReturn()
{
	SetUserName();
    document.getElementById("_processAttachment").value = "IsNotAttachment";
    document.getElementById("SaveButtonPressed").value = "Back"; 
	document.getElementById("Form1").submit();
}

function search()
{
	window.open('Search.aspx');
}

function SetCaptcha()
{
    var c = document.getElementById("CaptchaContext"); 
    
    if (c != null)
    {
        document.forms["Form1"].CaptchaContextSubmitted.value = c.value; 
    }
    
    var e = document.getElementById("CaptchaEntered"); 
    
    if (e != null)
    {
        document.forms["Form1"].CaptchaEnteredSubmitted.value = e.value; 
    }
}
function setCheckedValue(radioObj, newValue)
{
    if(!radioObj)
    {
        return;
    }
    var radioLength = radioObj.length;
    if (radioLength == undefined)
    {
        radioObj.checked = (radioObj.value == newValue);
        return;
    }
    for(var i = 0; i < radioLength; i++)
    {
        radioObj[i].checked = false;
        if(radioObj[i].value == newValue)
        {
            radioObj[i].checked = true;
        }
    }
}
function SetUserName()
{
	var r = document.getElementById("UserNameEntryField");
	if (r != null)
	{
		document.forms["Form1"].UserSuppliedName.value = r.value;
	}
}

function showDocMan_OnFocus()
{
    setCheckedValue(document.getElementById("Radio3"), "DocMan");
    ItemDisplay("CommonFields", true)
    ItemDisplay("FiledFields", true);
    ItemDisplay("PublishedFields", false);
    ItemDisplay("AuthorFields", true);
    ItemDisplay("FileNoFields", true);
    ItemDisplay("VersionFields", true);
    ItemDisplay("CommentFields", true);
    ItemDisplay("StatusDateFields", true);
    ItemDisplay("StatusFields", true);
    ItemDisplay("attachFileBtn", true);
}

function showFolder_OnFocus()
{
    setCheckedValue(document.getElementById("Radio2"),"Folder");
    ItemDisplay("CommonFields", true)
    ItemDisplay("FiledFields", true);
    ItemDisplay("PublishedFields", true);
    ItemDisplay("AuthorFields", true);
    ItemDisplay("FileNoFields", true);
    ItemDisplay("VersionFields", false);
    ItemDisplay("CommentFields", true);
    ItemDisplay("StatusDateFields", false);
    ItemDisplay("StatusFields", false);
    ItemDisplay("attachFileBtn",true);
}

function showNormal_OnFocus()
{
    setCheckedValue(document.getElementById("Radio1"),"Normal");
    ItemDisplay("CommonFields", true)
    ItemDisplay("FiledFields", false);
    ItemDisplay("PublishedFields", false);
    ItemDisplay("AuthorFields", false);
    ItemDisplay("FileNoFields", false);
    ItemDisplay("VersionFields", false);
    ItemDisplay("CommentFields", false);
    ItemDisplay("StatusDateFields", false);
    ItemDisplay("StatusFields", false);
    ItemDisplay("attachFileBtn",true);
}

function ShowTip(tipid)
{
	var s = document.getElementById(tipid);
	var tipArea = document.getElementById("TipArea");
	if (null != tipArea)
	{
	    tipArea.innerHTML = s.innerHTML;
    	tipArea.style.display = 'block';
	}
}

function Swap(alpha, beta)
{
	var a = document.getElementById(alpha);
	var b = document.getElementById(beta);
	if (a.style.display == 'block')
	{
		a.style.display = 'none';
		b.style.display = 'block';
	}
	else
	{
		b.style.display = 'none';
		a.style.display = 'block';
	}	
}

function textArea_OnFocus(event)
{
	document.onkeypress = Document_OnKeyPress;
}

function textArea_OnBlur(event)
{
	document.onkeypress = null;
}
function textarea_OnKeyPress(event)
{
    if (document.all && event.keyCode == 9) 
    {  
        event.returnValue= false; 
        document.selection.createRange().text = String.fromCharCode(9)
    }
}