server_api\.nuget\nuget.exe restore server_api/server_api.sln
call "%VS120COMNTOOLS%\vsvars32.bat"
msbuild server_api/server_api.sln /t:Rebuild /m