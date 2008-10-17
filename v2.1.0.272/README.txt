FlexWiki 2.0 Release To Web (RTW) README

New Installation Instructions
=============================
* Download the -web-full-bin-release.zip from the SourceForge project
  site at http://sf.net/projects/flexwiki
* Extract it to the directory of your choice
* Use inetmgr.exe to create a new IIS virtual directory pointing to
  the directory where the release was unzipped. 
* Optionally secure FlexWiki by enabling authentication and
  restricting access to the /admin, /logs, and /Namespaces
  directories.
* Configure FlexWiki by opening the wiki /admin application in a web
  browser. 

Help can be found on the FlexWiki users mailing list at
flexwiki-users@lists.sf.net. 

Upgrading An Existing Installation
==================================
* Back up your existing wiki files. 
* Delete the contents of your wiki's /bin directory
* Download the -web-upgrade-release.zip from the SourceForge project
  site at http://sf.net/projects/flexwiki
* Unzip the upgrade zip file over the top of your existing wiki. 
* Edit the flexwiki.config file to reflect your preferences - no
  automatic upgrade from FlexWiki 1.8 settings is provided. You can
  edit the configuration file via the FlexWiki /admin application in
  your web browser. 

Known Issues
============
* During release testing, FlexWiki was reported to consume large
  amounts of CPU. The developers were unable to reproduce the error,
  but disabling newsletters (via setting the
  NewsletterConfiguration/Enabled value to "false" in flexwiki.config)
  reportedly addressed the problem. You may wish to disable
  newsletters if you encounter high CPU utilization. 
