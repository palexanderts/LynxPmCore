@echo off
set LYNX_CLIENT=iberofama
set LYNX_ENVIROMENT=Development
cd /d C:\Users\palex\source\repos\LynxPmCore
dotnet run --project src/LynxPmCore.Api/LynxPmCore.Api.csproj --no-build --urls http://localhost:5015 > api_run.log 2>&1
