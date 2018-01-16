# ApplicationSamples\datatierplugin

The datatierplugin sample illustrates EF Migration support on the Apprenda platform. 

This repo contains two elements:
* A sample EF Migration plugin which implements Apprenda's Persistence API to provide Code-First EF support
* A sample app which has been modified to use the plugin and EF Migrations

In this folder you'll find "build.ps1".  The default build will produce the plugin and generate 4 archives for the sample app, each a different version.

* V1: creates a baseline app
* V2: adds two fields DOB & FavoriteColor to the Student records. NOTE: In the app's GUI these are only visible in detail/edit
* V3: is a copy of V2. This shows the behavior of having a version which does not implement DB changes, but must still include the plugin
* V4: renames FavoriteColor to FavColor in the DB.  The app's UI is not changed.

## Notes
* See: http://docs.apprenda.com/8-0/data-tier-plugin for additional documentation on this feature
* Make sure to download the Apprenda SDK for the version of Apprenda you are using. The SDK is available at http://docs.apprenda.com/downloads. You will need the SDK to get access to the latest DLL *Apprenda.API.Persistence.dll* that exposes the Data Tier interfaces for development. 
