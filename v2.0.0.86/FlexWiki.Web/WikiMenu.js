function LinkMenu(anArray, e)
{
	var src = "";
	var h = 0;
	var w = 0;

	e.cancelBubble = 'true';

	
	var menu = document.getElementById("LinkMenu");
	if (menu == null)
	{
		menu = document.createElement("div");
		menu.id = 'LinkMenu';
		menu.className = 'Menu';
		document.body.appendChild(menu);
	}
	menu.innerHTML = "";
	for (var i = 0; i < anArray.length; i++)
	{
		var pair = anArray[i];
		var each = "";
		var itemName = 'LinkMenu' + i;
		each += '<div class="MenuItemNormal" onmouseover="MenuItemIn(this);" onmouseout="MenuItemOut(this);">';
		each += '<span id="' + itemName + '" onclick="MenuClick(' + "'" + pair[1] + "'" + ')">' + pair[0] + '<' + '/span>';
		each += '<' + '/div>';
		menu.innerHTML += each;
		var item = document.getElementById(itemName);
		if (item.offsetWidth > w)
			w = item.offsetWidth;
		h += item.offsetHeight;
	}
	menu.innerHTML = '<div class="MenuItems" onmouseover="MenuIn(this);" onmouseout="MenuOut(this);">' + menu.innerHTML + '<' + '/div>';
	menu.style.left = document.body.scrollLeft + e.clientX;
	menu.style.top = document.body.scrollTop + e.clientY;
	menu.style.height = h;
	menu.style.width = w;
	timeout = window.setTimeout("MenuTimeout()", 4000, "JavaScript");
	menu.style.display = 'block';
}

var timeout = null;
var tipOffTimeout = null;

function TopicTipOn(anchor, id, event)
{
	var targetY = document.body.scrollTop + event.clientY + 18;
	var targetX = document.body.scrollLeft + event.clientX + 12;
	
	var topicTip = document.getElementById("TopicTip");
	if (null != topicTip)
	{
	    topicTip.style.left = targetX + "px";
	    topicTip.style.top = targetY + "px";
	    var tip = document.getElementById(id);
	    if (null != tip)
	    {
	        topicTip.innerHTML = tip.innerHTML;
	        topicTip.style.display = "block";
	    }
	}
	if (tipOffTimeout != null)
		window.clearTimeout(tipOffTimeout);
	tipOffTimeout = window.setTimeout("TopicTipOff()", 4000, "JavaScript");
}

function TopicTipOff()
{
	if (tipOffTimeout != null)
		window.clearTimeout(tipOffTimeout);
	tipOffTimeout = null;				
	document.getElementById("TopicTip").style.display = "none";
}


function MenuClick(url)
{
	MenuHide();
	document.location.href = url;
}

function MenuIn(obj)
{
	if (timeout == null)
		return;
	window.clearTimeout(timeout);
	timeout = null;
}

function MenuOut(obj)
{
	timeout = window.setTimeout("MenuTimeout()", 1000, "JavaScript");
}

function MenuTimeout()
{
	MenuHide();
}

function MenuHide()
{
	var menu = document.getElementById("LinkMenu");
	menu.style.display = 'none';
}

function cleanObfuscatedLink(obj, text, URL)
{
	obj.innerText = text;
	obj.href = URL;
}

function ShowObfuscatedLink(link)
{
	var i = 0;
	for (i = 0; i < link.length; i++)
	{
		document.write(link.substr(i, 1));
		if (i < 10)
			document.write('..');
	}
}

function MenuItemIn(obj)
{
	obj.className = 'MenuItemHover';
}

function MenuItemOut(obj)
{
	obj.className = 'MenuItemNormal';
}
