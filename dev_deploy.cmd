call "%VS120COMNTOOLS%\vsvars32.bat"
msbuild server_api/server_api.sln /m /p:DeployOnBuild=true /p:PublishProfile=deployment /p:publishUrl=c:\dev\api_deployment