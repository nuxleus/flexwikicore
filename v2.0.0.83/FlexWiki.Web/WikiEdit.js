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
	var r = document.getElementById("ReturnTopic");
    	document.getElementById("SaveButtonPressed").value = "Save"; 
	if (r != null)
	{
		r.value = ""; // prevent return action by emptying this out
	}
	document.getElementById("Form1").submit();
}

function SaveAndReturn()
{
	SetUserName();
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

function SetUserName()
{
	var r = document.getElementById("UserNameEntryField");
	if (r != null)
	{
		document.forms["Form1"].UserSuppliedName.value = r.value;
	}
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

function textArea_OnBlur(event)
{
	document.onkeypress = null;
}

function textArea_OnFocus(event)
{
	document.onkeypress = Document_OnKeyPress;
}


