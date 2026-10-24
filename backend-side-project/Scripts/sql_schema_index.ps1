# Import the SqlServer module
Import-Module "SqlServer"  


#指定要匯出哪個DB、哪個table
$taget_DB_Name = "LogDB"
$target_table_name = "EGamingDataMigratePendingQueue"
$connString = "Server=0.0.0.0;Initial Catalog=" + $taget_DB_Name + ";Persist Security Info=False;User ID=sa;Password=[REDACTED];MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=True;Connection Timeout=30;"


#SQL連線字串
$Database = Get-SqlDatabase -ConnectionString $connString



#宣告檔案路徑
$date = Get-Date
$dateText = $date.ToString("yyyyMMddHHmmss")
$FilePath = "C:\Users\user\Documents\Output_" + $target_table_name + "_" + $dateText + ".sql"



$scriptOptions = New-Object -TypeName Microsoft.SqlServer.Management.Smo.ScriptingOptions
$scriptOptions.NoCollation = $True
$scriptOptions.ScriptSchema = $True
$scriptOptions.ExtendedProperties = $true
$scriptOptions.Indexes = $true



#根據上方設定 執行匯出指令
($Database.Tables | Where Name -eq $target_table_name).EnumScript($scriptOptions) | Out-File -FilePath $FilePath