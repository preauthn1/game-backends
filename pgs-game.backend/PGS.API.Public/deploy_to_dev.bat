dotnet build ../target.API.ALL.sln
Powershell.exe -File remote_tryCatch_Deploy.ps1
timeout /t 120