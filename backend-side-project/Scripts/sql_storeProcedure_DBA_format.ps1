# Import the SqlServer module
Import-Module "SqlServer"  


#指定要匯出哪個DB、哪個table
$taget_DB_Name = "CenterDB"
$target_sp_name = "PrPs_CheckBonusEvent_ThenAssignBonus"
$connString = "Server=0.0.0.0;Initial Catalog=" + $taget_DB_Name + ";Persist Security Info=False;User ID=sa;Password=[REDACTED];MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=True;Connection Timeout=30;"


#SQL連線字串
$Database = Get-SqlDatabase -ConnectionString $connString


#宣告檔案路徑
$FilePath = "C:\Users\user\Documents\DBA_format\" +  $target_sp_name +  ".sql"
$proc = $Database.StoredProcedures | ?{ $_.Name -eq $target_sp_name} 



$prefixText = "USE "+ $taget_DB_Name +"
GO
IF NOT EXISTS(SELECT * FROM sys.procedures WHERE name = '" + $target_sp_name + "')
BEGIN
	DECLARE @SQL NVARCHAR(MAX) 
	SET @SQL = 'CREATE PROCEDURE " + $target_sp_name +" AS BEGIN RETURN END'
	EXEC (@SQL)
END
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
"


$prefixText + $proc.ScriptHeader($true) + $proc.TextBody | Out-File -FilePath $FilePath

