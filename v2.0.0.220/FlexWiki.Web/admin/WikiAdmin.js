// Copyright (c) Microsoft Corporation.  All rights reserved.
//
// The use and distribution terms for this software are covered by the 
// Common Public License 1.0 (http://opensource.org/licenses/cpl.php)
// which can be found in the file CPL.TXT at the root of this distribution.
// By using this software in any fashion, you are agreeing to be bound by 
// the terms of this license.
//
// You must not remove this notice, or any other, from this software.

function PagesChanged()
{
    var pagesCombo = document.getElementById("pages");
    if (null != pagesCombo)
    {
        var url = "ShowCache.aspx";
        var numKeys = pagesCombo.options[pagesCombo.selectedIndex].value;
        if (null != numKeys)
        {
            url += "?keys=" + numKeys;
            var firstKey = getQueryStringValue("start");
            if (null != firstKey)
            {
                url += "&start=" + firstKey;
            }
            var lastKey = getQueryStringValue("end");
            if (null != lastKey)
            {
                url += "&end=" + lastKey;
            }
            var searchBox = document.getElementById("filter");
            if (searchBox.value.length > 0)
            {
                url += "&search=" + searchBox.value;
            }
            window.location = url;
        }
    }
}

function getQueryStringValue(key)
{
    var queryString = window.location.search.substring(1);
    var items = queryString.split("&");
    for (var i = 0; i < items.length; i++)
    {
        var keyValue = items[i].split("=");
        if (keyValue[0] == key)
        {
            return keyValue[1];
        }
    }
    return null;
}