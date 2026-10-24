dotnet build ../target.BackendUtility.ALL.sln
start http://localhost:55888
dotnet run target.Backend.Utility --urls=http://localhost:55888
timeout /t 30 /nobreak