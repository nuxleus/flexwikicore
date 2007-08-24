function FilterChanged()
{
    var filterControl = document.getElementById("Filter");
    var filter = filterControl.value;
    var id = 0;
    while (true)
    {
        var keyData = KEYDATAPREFIX + id;
        var key = KEYPREFIX + id;

        var valueElement = document.getElementById(keyData);
        if (valueElement == null)
            break;
        var show = true;
        var test = "";
        try 
        {
            if (!valueElement.textContent)
                test = "" + valueElement.innerText.toUpperCase();
            else
                test = "" + valueElement.textContent.toUpperCase();
        } 
        catch (e) 
        {
            test = "" + valueElement.textContent.toUpperCase();
        }
        if (filter != "" && test.indexOf(filter.toUpperCase()) < 0)
            show = false;
        var styleString = show ? "" : "none";

        var keyPrefixElement = document.getElementById(key);
        keyPrefixElement.style.display = styleString;
        id++;
    }
}

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