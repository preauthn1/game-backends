$pw = convertto-securestring -AsPlainText -Force -String "terpro123!@#"
$cred = new-object -typename System.Management.Automation.PSCredential -argumentlist "0.0.0.0\Administrator",$pw

$sess = New-PSSession -Credential $cred -ComputerName 0.0.0.0

Invoke-Command -Session $sess -ScriptBlock { Import-Module WebAdministration; Stop-WebSite 'API.AdminSys' }
Invoke-Command -Session $sess -ScriptBlock { Import-Module WebAdministration; Restart-WebAppPool (Get-Website -Name 'API.AdminSys').applicationPool }