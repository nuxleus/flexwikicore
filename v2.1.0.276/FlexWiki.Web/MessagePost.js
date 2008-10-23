  // Copyright (c) Microsoft Corporation.  All rights reserved.
  //
  // The use and distribution terms for this software are covered by the 
  // Common Public License 1.0 (http://opensource.org/licenses/cpl.php)
  // which can be found in the file CPL.TXT at the root of this distribution.
  // By using this software in any fashion, you are agreeing to be bound by 
  // the terms of this license.
  //
  // You must not remove this notice, or any other, from this software.

function previewPost()
{
	var s = "!!" + document.forms["Form1"].MessageTitleText.value + "\n";
	s = s + document.forms["Form1"].MessageText.value + "\n";
	s = s + document.forms["Form1"].UserText.value + "\n";
	document.forms["Form2"].body.value = s;
	window.open('about:blank', 'previewWindow');
	document.forms["Form2"].submit();
}

