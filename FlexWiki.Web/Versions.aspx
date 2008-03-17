<%@ Page language="c#" CodeBehind="Versions.aspx.cs" AutoEventWireup="false" Inherits="FlexWiki.Web.Versions" %>
<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">
<html xmlns="http://www.w3.org/1999/xhtml" xml:lang="en" lang="en" >
	<head>
		<title><%= GetTitle() %></title>
		<meta name="Robots" content="NOINDEX, NOFOLLOW" />
<script  type="text/javascript" language="javascript">
/* <![CDATA[ */
function PageInit() {
    hf = document.getElementById("topicversions");
    if(!hf) return;
    lis = hf.getElementsByTagName("li");
    for (i=0;i<lis.length;i++) {
        inputs=lis[i].getElementsByTagName("input");
        if(inputs[0] && inputs[1]) {
                inputs[0].onclick = diffcheck;
                inputs[1].onclick = diffcheck;
        }
    }
    diffcheck();
}
function diffcheck() { 
    var dli = false; // the li where the diff radio is checked
    var oli = false; // the li where the oldid radio is checked
    hf = document.getElementById("topicversions");
    if(!hf) return;
    lis = hf.getElementsByTagName("li");
    for (i=0;i<lis.length;i++) {
        inputs=lis[i].getElementsByTagName("input");
        if(inputs[1] && inputs[0]) {
            if(inputs[1].checked || inputs[0].checked) { // this row has a checked radio button
                if(inputs[1].checked && inputs[0].checked && inputs[0].value == inputs[1].value) return false;
                if(oli) { // it's the second checked radio
                    if(inputs[1].checked) {
                    oli.className = "selected";
                    return false 
                    }
                } else if (inputs[0].checked) {
                    return false;
                }
                if(inputs[0].checked) dli = lis[i];
                if(!oli) inputs[0].style.visibility = "hidden";
                if(dli) inputs[1].style.visibility = "hidden";
                lis[i].className = "selected";
                oli = lis[i];
            }  else { // no radio is checked in this row
                if(!oli) inputs[0].style.visibility = "hidden";
                else inputs[0].style.visibility = "visible";
                if(dli) inputs[1].style.visibility = "hidden";
                else inputs[1].style.visibility = "visible";
                lis[i].className = "";
            }
        }
    }
}
/* ]]> */
    </script>
    <%= BuildPageOne() %>
        <asp:PlaceHolder ID="phResult" runat="server" />
    <%= BuildPageTwo() %>

