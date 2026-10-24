

CREATE PROCEDURE [dbo].[SP_AddFishJackpotLog]

	@in_ServerID	AS int,
	@in_ShotID	AS bigint,
	@in_ShotNum	AS int,
	@in_ShotBet	AS bigint,
	@in_FishMoney	AS bigint,
	@in_Win	AS bigint,
	@in_CommissionMoney	AS int,
	@in_LineNum	AS int,
	@in_LineBase	AS int,
	@in_LineUp	AS int,
	@in_LineDown	AS int,
	@in_PowerBase	AS int,
	@in_PowerAdd	AS int,
	@in_Power	AS int,
	@in_AvgBetTotal	AS decimal(10,3),
	@in_ShotTotal	AS bigint,
	@in_ShotBetCalc	AS bigint,
	@in_FishMoneyTotal	AS bigint,
	@in_WinTotal	AS bigint,
	@in_ShotBetTotal	AS bigint,
	@in_CommissionTotal	AS bigint,
	@in_InitCalc	AS int
AS
BEGIN

	DECLARE @StrSql varchar(8000);
	DECLARE @StrSqlHead varchar(100);
	DECLARE @CurrentDate varchar(10);
	DECLARE @TableName varchar(40);
	declare @CountNum int;
	
	SELECT @CurrentDate = CONVERT(varchar, GETDATE(), 112);
	
	SET @TableName = 'dbo.FishJackpotLog_' + @CurrentDate;
	
	SELECT @CountNum = COUNT(1) FROM sysobjects WHERE id = object_id(@TableName) AND type = 'U';
	IF @CountNum = 0
	BEGIN
		SET @StrSql = 'SELECT * INTO ' + @TableName + ' FROM dbo.FishJackpotLog WHERE 2=1;';
		EXEC(@StrSql);
	END
	
	SET @StrSqlHead = 'insert into ' + @TableName;
	SET @StrSql = '(ServerID,ShotID,ShotNum,ShotBet,FishMoney,Win,CommissionMoney,LineNum,LineBase,LineUp,LineDown,PowerBase,PowerAdd,' +
				'Power,AvgBetTotal,ShotTotal,ShotBetCalc,FishMoneyTotal,WinTotal,ShotBetTotal,CommissionTotal,InitCalc,CurrDate';
	SET @StrSql = @StrSql + ') values(';
	SET @StrSql = @StrSql + CAST(@in_ServerID AS VARCHAR) + ',';
	SET @StrSql = @StrSql + CAST(@in_ShotID AS VARCHAR) + ',';
	SET @StrSql = @StrSql + CAST(@in_ShotNum AS VARCHAR) + ',';
	SET @StrSql = @StrSql + CAST(@in_ShotBet AS VARCHAR) + ',';
	SET @StrSql = @StrSql + CAST(@in_FishMoney AS VARCHAR) + ',';
	SET @StrSql = @StrSql + CAST(@in_Win AS VARCHAR) + ',';
	SET @StrSql = @StrSql + CAST(@in_CommissionMoney AS VARCHAR) + ',';
	SET @StrSql = @StrSql + CAST(@in_LineNum AS VARCHAR) + ',';
	SET @StrSql = @StrSql + CAST(@in_LineBase AS VARCHAR) + ',';
	SET @StrSql = @StrSql + CAST(@in_LineUp AS VARCHAR) + ',';
	SET @StrSql = @StrSql + CAST(@in_LineDown AS VARCHAR) + ',';
	SET @StrSql = @StrSql + CAST(@in_PowerBase AS VARCHAR) + ',';
	SET @StrSql = @StrSql + CAST(@in_PowerAdd AS VARCHAR) + ',';
	SET @StrSql = @StrSql + CAST(@in_Power AS VARCHAR) + ',';
	SET @StrSql = @StrSql + CAST(@in_AvgBetTotal AS VARCHAR) + ',';
	SET @StrSql = @StrSql + CAST(@in_ShotTotal AS VARCHAR) + ',';
	SET @StrSql = @StrSql + CAST(@in_ShotBetCalc AS VARCHAR) + ',';
	SET @StrSql = @StrSql + CAST(@in_FishMoneyTotal AS VARCHAR) + ',';
	SET @StrSql = @StrSql + CAST(@in_WinTotal AS VARCHAR) + ',';
	SET @StrSql = @StrSql + CAST(@in_ShotBetTotal AS VARCHAR) + ',';
	SET @StrSql = @StrSql + CAST(@in_CommissionTotal AS VARCHAR) + ',';
	SET @StrSql = @StrSql + CAST(@in_InitCalc AS VARCHAR) + ',';
	SET @StrSql = @StrSql + 'GETDATE() )';
	PRINT @StrSqlHead + @StrSql;
	EXEC(@StrSqlHead + @StrSql);
	
	SELECT @CountNum = COUNT(1) FROM FishJackpot WHERE ServerID = @in_ServerID;
	IF(@CountNum = 0)
	BEGIN
		SET @StrSqlHead = 'insert into FishJackpot ';
		EXEC(@StrSqlHead + @StrSql);
	END
	ELSE
	BEGIN
		UPDATE FishJackpot 
		SET ShotID = @in_ShotID,
				ShotNum = @in_ShotNum,
				ShotBet = @in_ShotBet,
				FishMoney = @in_FishMoney,
				Win = @in_Win,
				CommissionMoney = @in_CommissionMoney,
				LineNum = @in_LineNum,
				LineBase = @in_LineBase,
				LineUp = @in_LineUp,
				LineDown = @in_LineDown,
				PowerBase = @in_PowerBase,
				PowerAdd = @in_PowerAdd,
				Power = @in_Power,
				AvgBetTotal = @in_AvgBetTotal,
				ShotTotal = @in_ShotTotal,
				ShotBetCalc = @in_ShotBetCalc,
				FishMoneyTotal = @in_FishMoneyTotal,
				WinTotal = @in_WinTotal,
				ShotBetTotal = @in_ShotBetTotal,
				CommissionTotal = @in_CommissionTotal,
				InitCalc = @in_InitCalc
		WHERE ServerID = @in_ServerID;
	END
END;
