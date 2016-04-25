# Airvolution Project: Server API


## About

The Airvolution Project consists of multiple components. This component is the Server API. 


##Server Deployment


 * On Windows, use Git to download the entire airvolution.server.api source folder. 
 * If simply running locally.
 * Open the solution file (.sln) inside the server_api folder with Visual Studio 2013 (Ultimate).
 * Build and run the project.
 
If running on a server:

 * Run the “build.cmd” file to build the project.
 * Run the “dev_deploy.cmd” to deploy the project to a specified folder.
 * Point IIS or other web hosting service to correct deployment folder.
 * Access API through {url}/api/
 * Also, Swagger is accessed through {url}/api/swagger

##Database Deployment

* Build the project as detailed above.
* Update the Connection String within the Web.config file to point to your database.
* Within the “Package Manager Console” run “Update-Database -Verbose”
*This will automatically update your database to match the model of the project.**

##AirNowSaveData
This tool can be used to download data from the [AirNow API](www.airnowapi.org).

* After the solution has been built, you may specify arguments for the begin date (YYYY-MM-DD) and end date (YYYY-MM-DD). 
* Simply executing the program will download all of the data and save it within a generated folder “AirNow Backup Directory” within the “airnow_retrieval” folder.

##AirNowStoreToDB
This tool is used to upload data to your database via the running server API.

* Build the project as detailed above.
* Any files saved within the “AirNow Backup Directory” will be uploaded to the you database via API endpoints of your running server.


## ElasticSearch Search Service
Elastic search is needed to enable the search functionality on the front end. This is separate from the API and must be deployed separately. [Elasticsearch](https://www.elastic.co/products/elasticsearch) can be downloaded for free. However, as it is a search service it will require custom configuration and setup to obtain proper search results. Part of this will be loading data into ES. This can be accomplished in many ways. The most robust will require you to 
create a custom sync service which will pull data from the DB into search to ensure that the search indices are accurate. This is non-trivial and beyond the scope of a readme. This solution will be entirely dependent on your data and how often it changes. 


# Requirements

AirNow API Key
Google API Key
Microsoft Azure Sendgrid API Key
SQL Server 2008+
Visual Studio 2013 (Ultimate)+



