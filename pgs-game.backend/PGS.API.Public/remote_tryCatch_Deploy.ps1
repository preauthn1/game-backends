
$pw = convertto-securestring -AsPlainText -Force -String "terpro123!@#"
$cred = new-object -typename System.Management.Automation.PSCredential -argumentlist "0.0.0.0\Administrator",$pw
$sess = New-PSSession -Credential $cred -ComputerName 0.0.0.0


function callRemoteDeleteFolder
{
    Invoke-Command -Session $sess -ScriptBlock { 
		
        try
        {
            import-Module WebAdministration; 
        
            Restart-WebAppPool (Get-Website -Name 'API.Public').applicationPool;

            Set-ItemProperty "IIS:\Sites\API.Public" applicationPool "API.Public";
        
            Restart-WebAppPool (Get-Website -Name 'API.Public').applicationPool;

            Get-ChildItem -Path "D:\wwwroot\API.Public" -Include *.* -File -Recurse -Force | foreach { $_.Delete()}

            Set-ItemProperty "IIS:\Sites\API.Public" applicationPool "API.Public"

		    cmd /c exit 1
        }
        catch
        {
            Write-Warning $Error[0]
            cmd /c exit -1
        }
    }

    $resultCode = invoke-command -ScriptBlock { $lastexitcode } -Session $sess
    Write-Output $resultCode 
    return $resultCode;
}


$code = callRemoteDeleteFolder

If ($code -eq 1) {
    
   
    Write-Output "-----------------------";
    Write-Output "-----------------------";
    Write-Output "success delete!";
    Write-Output "-----------------------";
    Write-Output "-----------------------";

    dotnet publish target.API.Public.csproj -p:PublishProfile=Dev_Deploy.pubxml /p:Password=[REDACTED]

} 
else {

	Write-Output "-----------------------";
	Write-Output "-----------------------";
	Write-Output " first-time is error.... will retry again in 90s ...";
	Write-Output "-----------------------";
	Write-Output "-----------------------";


    ### 閒置N秒後再試一次
    Start-Sleep -s 90
    
    $code2 = callRemoteDeleteFolder
    Write-Output $code2 
    

    If ($code2 -eq 1) {


        Write-Output "-----------------------";
        Write-Output "-----------------------";
        Write-Output "success delete (at 2th times) !";
        Write-Output "-----------------------";
        Write-Output "-----------------------";


        dotnet publish target.API.Public.csproj -p:PublishProfile=Dev_Deploy.pubxml /p:Password=[REDACTED]
    }
    else
    {
        Write-Output "-----------------------";
        Write-Output "-----------------------";
        Write-Output "fail... exception !";
        Write-Output "-----------------------";
        Write-Output "-----------------------";
		
		Start-Sleep -s 90
    }
    
}

