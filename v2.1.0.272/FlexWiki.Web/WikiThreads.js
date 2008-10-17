  // Copyright (c) Microsoft Corporation.  All rights reserved.
  //
  // The use and distribution terms for this software are covered by the 
  // Common Public License 1.0 (http://opensource.org/licenses/cpl.php)
  // which can be found in the file CPL.TXT at the root of this distribution.
  // By using this software in any fashion, you are agreeing to be bound by 
  // the terms of this license.
  //
  // You must not remove this notice, or any other, from this software.


function ToggleDisplay(alpha)
 {
        id_link = "Link_" + alpha;
	var a = document.getElementById(id_link);
	id_thread = "Thread_" + alpha;
	var b = document.getElementById(id_thread);
	if (a.innerHTML == "Hide Thread")
	{
		a.innerHTML = "Show Thread";
		DoSubThreads(alpha, "hide");
		b.style.display = 'none';
	}
	else
	{
		a.innerHTML = "Hide Thread";
		DoSubThreads(alpha, "show");
		b.style.display = 'block';
	}
 }
function DoSubThreads(alpha, vis)
 {
	var re = new RegExp(alpha);
	var divs = document.getElementsByTagName("div");
	for (var i = 0; i < divs.length; i++)
	{
		if(divs[i].id.match(re))
		{
			var myIds = new Array();
			myIds = divs[i].id.split(";");
			if (vis == "hide")
			{
				if (((alpha.indexOf(myIds[myIds.length - 2])) == -1) && ((myIds[myIds.length - 2].indexOf("Thread_")) == -1))
				{
					var x = document.getElementById(myIds[myIds.length - 2]);
					x.style.display = 'none';
				}
			}
			else
			{
				if ((myIds[myIds.length - 2].indexOf("Thread_")) == -1)
				{
					var x = document.getElementById(myIds[myIds.length - 2]);
					x.style.display = 'block';
				}
			}
		} 
	}
	
 }
function Collapse(){
        var arrDivs = getElementsByClassName(document.getElementById("TopicBody"), "div", "Depth7");
        for (var i = 0; i < arrDivs.length; i++)
        {
                 arrDivs[i].style.display = 'none';
        }
        var arrDivs = getElementsByClassName(document.getElementById("TopicBody"), "div", "Depth6");
        for (var i = 0; i < arrDivs.length; i++)
        {
                 arrDivs[i].style.display = 'none';
        }
        var arrDivs = getElementsByClassName(document.getElementById("TopicBody"), "div", "Depth5");
        for (var i = 0; i < arrDivs.length; i++)
        {
                 arrDivs[i].style.display = 'none';
        }
        var arrDivs = getElementsByClassName(document.getElementById("TopicBody"), "div", "Depth4");
        for (var i = 0; i < arrDivs.length; i++)
        {
                 arrDivs[i].style.display = 'none';
        }
        var arrDivs = getElementsByClassName(document.getElementById("TopicBody"), "div", "Depth3");
        for (var i = 0; i < arrDivs.length; i++)
        {
                 arrDivs[i].style.display = 'none';
        }
        var arrDivs = getElementsByClassName(document.getElementById("TopicBody"), "div", "Depth2");
        for (var i = 0; i < arrDivs.length; i++)
        {
                 arrDivs[i].style.display = 'none';
        }
        var arrDivs = getElementsByClassName(document.getElementById("TopicBody"), "div", "Depth1");
        for (var i = 0; i < arrDivs.length; i++)
        {
                 arrDivs[i].style.display = 'none';
        }
 
}
function OpenAll(){
        var arrDivs = getElementsByClassName(document.getElementById("TopicBody"), "div", "Depth0");
        for (var i = 0; i < arrDivs.length; i++)
        {
                 arrDivs[i].style.display = 'block';
        }
        var arrDivs = getElementsByClassName(document.getElementById("TopicBody"), "div", "Depth1");
        for (var i = 0; i < arrDivs.length; i++)
        {
                 arrDivs[i].style.display = 'block';
        }
        var arrDivs = getElementsByClassName(document.getElementById("TopicBody"), "div", "Depth2");
        for (var i = 0; i < arrDivs.length; i++)
        {
                 arrDivs[i].style.display = 'block';
        }
        var arrDivs = getElementsByClassName(document.getElementById("TopicBody"), "div", "Depth3");
        for (var i = 0; i < arrDivs.length; i++)
        {
                 arrDivs[i].style.display = 'block';
        }
        var arrDivs = getElementsByClassName(document.getElementById("TopicBody"), "div", "Depth4");
        for (var i = 0; i < arrDivs.length; i++)
        {
                 arrDivs[i].style.display = 'block';
        }
        var arrDivs = getElementsByClassName(document.getElementById("TopicBody"), "div", "Depth5");
        for (var i = 0; i < arrDivs.length; i++)
        {
                 arrDivs[i].style.display = 'block';
        }
        var arrDivs = getElementsByClassName(document.getElementById("TopicBody"), "div", "Depth6");
        for (var i = 0; i < arrDivs.length; i++)
        {
                 arrDivs[i].style.display = 'block';
        }
        var arrDivs = getElementsByClassName(document.getElementById("TopicBody"), "div", "Depth7");
        for (var i = 0; i < arrDivs.length; i++)
        {
                 arrDivs[i].style.display = 'block';
        }

}
/*
	Written by Jonathan Snook, http://www.snook.ca/jonathan
	Add-ons by Robert Nyman, http://www.robertnyman.com
*/

function getElementsByClassName(oElm, strTagName, strClassName){
	var arrElements = (strTagName == "*" && oElm.all)? oElm.all : oElm.getElementsByTagName(strTagName);
	var arrReturnElements = new Array();
	strClassName = strClassName.replace(/\-/g, "\\-");
	var oRegExp = new RegExp("(^|\\s)" + strClassName + "(\\s|$)");
	var oElement;
	for(var i=0; i<arrElements.length; i++){
		oElement = arrElements[i];
		if(oRegExp.test(oElement.className)){
			arrReturnElements.push(oElement);
		}
	}
	return (arrReturnElements)
}

