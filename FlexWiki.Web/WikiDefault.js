  // Copyright (c) Microsoft Corporation.  All rights reserved.
  //
  // The use and distribution terms for this software are covered by the 
  // Common Public License 1.0 (http://opensource.org/licenses/cpl.php)
  // which can be found in the file CPL.TXT at the root of this distribution.
  // By using this software in any fashion, you are agreeing to be bound by 
  // the terms of this license.
  //
  // You must not remove this notice, or any other, from this software.

function SetActiveStylesheet(title) {
  var i, a, main;
  for(i=0; (a = document.getElementsByTagName("link")[i]); i++) {
    if(a.getAttribute("rel").indexOf("style") != -1 && a.getAttribute("title")) {
      a.disabled = true;
      if(a.getAttribute("title") == title) a.disabled = false;
    }
  }
}

function GetActiveStyleSheet() {
  var i, a;
  for(i=0; (a = document.getElementsByTagName("link")[i]); i++) {
    if(a.getAttribute("rel").indexOf("style") != -1 && a.getAttribute("title") && !a.disabled) return a.getAttribute("title");
  }
  return null;
}

function GetPreferredStylesheet() {
  var i, a;
  for(i=0; (a = document.getElementsByTagName("link")[i]); i++) {
    if(a.getAttribute("rel").indexOf("style") != -1
       && a.getAttribute("rel").indexOf("alt") == -1
       && a.getAttribute("title")
       ) return a.getAttribute("title");
  }
  return null;
}

function CreateCookie(name,value,days) {
  if (days) {
    var date = new Date();
    date.setTime(date.getTime()+(days*24*60*60*1000));
    var expires = "; expires="+date.toGMTString();
  }
  else expires = "";
  document.cookie = name+"="+value+expires+"; path=/";
}

function ReadCookie(name) {
  var nameEQ = name + "=";
  var ca = document.cookie.split(';');
  for(var i=0;i < ca.length;i++) {
    var c = ca[i];
    while (c.charAt(0)==' ') c = c.substring(1,c.length);
    if (c.indexOf(nameEQ) == 0) return c.substring(nameEQ.length,c.length);
  }
  return null;
}

window.onload = function(e) {
  var cookie = ReadCookie("fw-style");
  var title = cookie ? cookie : GetPreferredStylesheet();
  SetActiveStylesheet(title);
}

window.onunload = function(e) {
  var title = GetActiveStyleSheet();
  CreateCookie("fw-style", title, 365);
}

var cookie = ReadCookie("fw-style");
var title = cookie ? cookie : GetPreferredStylesheet();
SetActiveStylesheet(title);
var styleMenuOffTimeout = null;


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
