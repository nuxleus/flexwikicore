function textArea_OnFocus(event)
{
	document.onkeypress = Document_OnKeyPress;
}

function textArea_OnBlur(event)
{
	document.onkeypress = null;
}

function Document_OnKeyPress(event)
{
	if (event != null) // FireFox only
	{
		if (event.keyCode == 9)
		{
			textArea = document.forms["Form1"].Text1;
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

function ShowTip(tipid)
{
	var s = document.getElementById(tipid);
	TipArea.innerHTML = s.innerHTML;
	TipArea.style.display = 'block';
}

function preview()
{
	var s = document.forms["Form1"].Text1.value;
	document.forms["Form2"].body.value = s;
	window.open('about:blank', 'previewWindow');
	document.forms["Form2"].submit();
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
		document.forms["Form1"].UserSuppliedName.value = r.value;
}

function Cancel()
{
	history.back();
}

function Save()
{
	SetUserName();
	SetCaptcha();
	var r = document.getElementById("ReturnTopic");
	if (r != null)
		r.value = ""; // prevent return action by emptying this out
	document.getElementById("Form1").submit();
}

function SaveAndReturn()
{
	SetUserName();
	document.getElementById("Form1").submit();
}

function search()
{
	window.open('Search.aspx');
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
		answer -= e.scrollWidth;
	return answer;
}


function ChangeTemplate(selectId)
{
	var templateCombo = document.getElementById(selectId);
	if ((templateCombo) && (templateCombo.selectedIndex > -1))
	{
		var strInsertText = templateCombo.options[templateCombo.selectedIndex].value;
		
		var objTextArea = document.forms['Form1'].Text1;
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
