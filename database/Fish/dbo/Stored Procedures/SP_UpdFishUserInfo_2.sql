CREATE PROCEDURE [dbo].[SP_UpdFishUserInfo_2]
  @in_UserID AS int,
  @in_ServerID AS int ,
  @in_MaxWeapon AS int ,
  @in_Exp AS int ,
  @in_OnlineTime AS int ,
  @in_HisMaxMoney AS bigint ,
  @in_DayWinMoney AS bigint ,
  @in_DayLoseMoney AS bigint ,
  @in_ShotNum AS int ,
  @in_MissMoneyLib AS int 
AS
BEGIN
	
	update dbo.FishUserInfo set MaxWeaponLevel = @in_MaxWeapon, Exp = Exp + @in_Exp, OnlineTime = OnlineTime + @in_OnlineTime, HisMaxMoney = @in_HisMaxMoney,
	TotalWinMoney = TotalWinMoney + @in_DayWinMoney, TotalLoseMoney = TotalLoseMoney + @in_DayLoseMoney, DayWinMoney = DayWinMoney + @in_DayWinMoney,
	DayLoseMoney = DayLoseMoney + @in_DayLoseMoney, TotalShotNum = TotalShotNum + @in_ShotNum, MissMoneyLib = MissMoneyLib + @in_MissMoneyLib
	where UserID=@in_UserID;
	
	-- 修改金币日志
	DECLARE @StrSql varchar(8000);
	DECLARE @StrSqlHead varchar(100);
	DECLARE @CurrentDate varchar(10);
	DECLARE @TableName varchar(40);
	declare @CountNum int;
	
	SELECT @CurrentDate = CONVERT(varchar, GETDATE(), 112);
	
	SET @TableName = 'dbo.FishUserMoneyLog_' + @CurrentDate;
	
	SELECT @CountNum = COUNT(1) FROM sysobjects WHERE id = object_id(@TableName) AND type = 'U';
	IF @CountNum = 0
	BEGIN
		SET @StrSql = 'SELECT * INTO ' + @TableName + ' FROM dbo.FishUserMoneyLog WHERE 2=1;';
		EXEC(@StrSql);
	END
	
	SET @StrSqlHead = 'insert into ' + @TableName;
	SET @StrSql = '(UserID, ServerID, Exp, OnlineTime, WinMoney, LoseMoney, ShotNum, MissMoneyLib, CreateDate';
	SET @StrSql = @StrSql + ') values(';
	SET @StrSql = @StrSql + CAST(@in_UserID AS VARCHAR) + ',';
	SET @StrSql = @StrSql + CAST(@in_ServerID AS VARCHAR) + ',';
	SET @StrSql = @StrSql + CAST(@in_Exp AS VARCHAR) + ',';
	SET @StrSql = @StrSql + CAST(@in_OnlineTime AS VARCHAR) + ',';
	SET @StrSql = @StrSql + CAST(@in_DayWinMoney AS VARCHAR) + ',';
	SET @StrSql = @StrSql + CAST(@in_DayLoseMoney AS VARCHAR) + ',';
	SET @StrSql = @StrSql + CAST(@in_ShotNum AS VARCHAR) + ',';
	SET @StrSql = @StrSql + CAST(@in_MissMoneyLib AS VARCHAR) + ',';
	SET @StrSql = @StrSql + 'GETDATE() )';
	--PRINT @StrSqlHead + @StrSql;
	EXEC(@StrSqlHead + @StrSql);
	
	
END;