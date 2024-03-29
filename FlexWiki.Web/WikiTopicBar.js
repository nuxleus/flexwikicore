  // Copyright (c) Microsoft Corporation.  All rights reserved.
  //
  // The use and distribution terms for this software are covered by the 
  // Common Public License 1.0 (http://opensource.org/licenses/cpl.php)
  // which can be found in the file CPL.TXT at the root of this distribution.
  // By using this software in any fashion, you are agreeing to be bound by 
  // the terms of this license.
  //
  // You must not remove this notice, or any other, from this software.

function TopicBarMouseOver()
{
	//TopicBar.className='TopicBarHover';
	obj = document.getElementById("TopicBar");
	obj.className="TopicBarHover";
}

function TopicBarMouseOut()
{
	//TopicBar.className='TopicBar';
	obj = document.getElementById("TopicBar");
	obj.className="TopicBar";
}

function IsEditing()
{
	return document.getElementById("DynamicTopicBar").style.display == 'block';
}

function SetEditing(flag)
{
	var isEditing = IsEditing();
	if (isEditing == flag)
		return; 
	isEditing = flag;
	if (isEditing)
	{
		document.getElementById("StaticTopicBar").style.display = 'none';
		document.getElementById("DynamicTopicBar").style.display = 'block';
	}
	else
	{
		document.getElementById("StaticTopicBar").style.display = 'block';
		document.getElementById("DynamicTopicBar").style.display = 'none';
	}
}

function TopicBarClick(event)
{
	event.cancelBubble = true;
	if (IsEditing())
		return;

	// Grab these dimensions before SetEditng() to get them before the control is hidden (thus h=0;w=0!)
	var staticWide = document.getElementById("StaticTopicBar").offsetWidth;
	var staticHigh = document.getElementById("StaticTopicBar").offsetHeight;

	SetEditing(true);

	document.getElementById("DynamicTopicBar").left = document.getElementById("TopicBar").offsetLeft;
	document.getElementById("DynamicTopicBar").top = document.getElementById("TopicBar").offsetTop;
	document.getElementById("DynamicTopicBar").width = document.getElementById("TopicBar").width;
	document.getElementById("DynamicTopicBar").height = document.getElementById("TopicBar").height;

	var tbi = tbinput();
	tbi.left = document.getElementById("DynamicTopicBar").left;
	tbi.top = document.getElementById("DynamicTopicBar").top;
	tbi.width = staticWide;
	tbi.height = staticHigh;
	tbi.value = '';
	tbi.focus();
	tbi.select();
}

function tbinput()
{
	return 	document.getElementById('TopicBarInputBox');
}

