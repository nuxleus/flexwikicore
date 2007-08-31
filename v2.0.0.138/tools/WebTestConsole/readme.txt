! What is WebTestConsole? 

WebTestConsole is an extremely simple performance measurement tool
written by CraigAndera. Given a list of URLs, it visits them in
sequence (one at a time), measuring the time it takes to retrieve each
URL. It was written primarily as a rough evaluation tool for the
caching code added to FlexWiki 2.0 code, but there is nothing about it
that is specific to FlexWiki. 

! How Do I Use WebTestConsole? 

Usage of WebTestConsole is as follows: 

webtestconsole /baseUrl=http://your.server.com/path/to/wiki/
  /iterations=n /urllist=path\to\urls.txt /output=path\to\output.csv
  /tag=your-tag

An example would be: 

webtestconsole /baseUrl=http://w2k3baseline/wiki/ /iterations=3
  /urllist=urls.txt /output=flexwiki20.csv /tag=flexwiki20

The command line switches are all required, and have the following
meanings: 

/baseUrl=	Specifies the "base URL". URLs in the URL file (see
		/urllist below), are appended to this base URL. 
/iterations=    The number of times to loop through the URL file. 
/urllist=       A path to a file containing a list of URLs, one per
		line. URLs are relative to the base URL (see /baseUrl
		above), and are retrieved via an HTTP GET. 
/output=	The name of a file with which to create the results
		and error files. See below for an explanation of how
		output files are named. 
/tag=		A bit of text that is copied into the result file for
		every result. Meant as a short, descriptive tag for
		the test being run. E.g. "flexwiki20". 

WebTestConsole creates two output files for every run of the program:
a results file and an errors file. The errors file is simply a
straight text dump of any exceptions encountered while running, along
with the URL and iteration where the error occurred. The results file
format is comma-separated-values (CSV), and is specified below. 

Output file names are based on the /output= parameter as follows: 

!! Output File Names

The date and time at the start of the test is recorded, and the output
files names are formed as follows: 

name-YYYY-MM-DD-HH-mm-ss.ext (result file)
name-YYYY-MM-DD-HH-mm-ss.ext.errors (errors file)

where name and ext are the name and extension specified via /output=.
So, for example, if 

/output=flexwiki20.csv 

is specified during a run of WebTestConsole.exe, the following two
files might be created: 

flexwiki20-2007-08-10-14-18-52.csv
flexwiki20-2007-08-10-14-18-52.csv.errors

!! Result File Format

The results file is a textual, comma-separated file with the following
columns: 

tag, starttime, time, iteration, exception, statuscode, ttfb, ttlb,
bytes, url

tag = The tag specified via /tag= at the command line. 
starttime = The date and time the test was started. 
time = The date and time this particular URL was tested.
iteration = The number of the iteration for this particular row. 
exception = "True" if an exception was encountered during processing
	    of this URL. 
statuscode = The HTTP status code returned. 
ttfb = The Time To First Byte when retrieving this URL. 
ttlb = The Time To Last Byte when retrieving this URL. 
bytes = The number of bytes retrieved for this URL. 
url = The URL that was retrieved. 

So a row of the result file might look like this (wrapped for
readability):

flexwiki20,8/10/2007 2:18:52 PM,8/10/2007 2:19:17
PM,0,False,200,24515,24519,8969,http://w2k3baseline/wiki/default.aspx
/FlexWiki/foo.html

!! Example URL List File

The URL file is extremely simple: one URL per line. Here is a small
example: 

/default.aspx/FlexWiki/1.html
/default.aspx/FlexWiki/1557377ExamplePage1.html
/default.aspx/FlexWiki/1557377ExamplePage2.html
/default.aspx/FlexWiki/2004 3Q.html
/default.aspx/FlexWiki/600057.html
/default.aspx/FlexWiki/aaaaaaaaaaa.html
/default.aspx/FlexWiki/aanimal's.html
/default.aspx/FlexWiki/AaronSachs.html
/default.aspx/FlexWiki/aarseth.html


