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

function nav(s)
{
	window.navigate(s);
}

function diffToggle()
{
	if (showDiffs.checked)
		showChanges();
	else
		hideChanges();
}

function showVersion()
{
	nav(VersionList.value);
}
