





-- =============================================
-- Author:  alan
-- Create date: 2018/08/10
-- Description:	日報表 統計前一日代理的收益
-- =============================================
CREATE PROCEDURE [dbo].[PrJob_Stat_AgentProfit_Daily]
AS
Begin

DECLARE @dtCreateTime datetime
DECLARE @intYesterday int

SET @dtCreateTime = DATEADD(DAY,-1,GETDATE())
SET @intYesterday = CAST ( CONVERT(varchar,@dtCreateTime,112) AS INT)

DECLARE @AgentID uniqueidentifier
DECLARE @intClass int

	DECLARE StatCursor CURSOR FOR 
		Select ID,Class from AgentUsers with (nolock) Where class not in (99,100) --只撈取總代理,1,2,3,4級代理

 
	OPEN StatCursor

	FETCH NEXT FROM StatCursor INTO @AgentID, @intClass

	WHILE @@FETCH_STATUS = 0
	BEGIN


		DECLARE @intSumFlows bigint	= 0	--當天流水
		DECLARE @intSumIncome bigint = 0--當天收益
		DECLARE @intType tinyint = 1	--收益结算

		--從即時收益取得
		SELECT  @intSumFlows = ISNULL(sum(flows),0), @intSumIncome = ISNULL(sum(Income),0)
		FROM [AgentProfitLog] with (nolock) where YYYYMMDD = @intYesterday and ID = @AgentID

		UPDATE [dbo].[Stat_AgentProfitDaily] set Rate=0, Income=@intSumIncome, Flows=@intSumFlows where ID=@AgentID and YYYYMMDD = @intYesterday
		IF @@ROWCOUNT = 0
			Insert into [dbo].[Stat_AgentProfitDaily] (YYYYMMDD, ID, Flows, Rate, Income) Values(@intYesterday,@AgentID,@intSumFlows,0,@intSumIncome)

			
		UPDATE [dbo].[WithdrawLog] set  UpdateAmount=@intSumIncome
		WHERE ID = @AgentID and YYYYMMDD = @intYesterday and [Type]=@intType
		IF @@ROWCOUNT = 0	
			Insert into [dbo].[WithdrawLog] (YYYYMMDD, ID, CurrentAmount, UpdateAmount, RemainAmount, Memo, [Type]) 
			Values (@intYesterday, @AgentID, 0, @intSumIncome,0, '收益结算', @intType)
		
		--更新錢包(改為即時更新)
		--UPDATE dbo.AgentUsers SET RemainAmount = RemainAmount+@intSumIncome WHERE ID=@AgentID
		--------------------------------------------------------------------------------------------



		FETCH NEXT FROM StatCursor INTO @AgentID, @intClass

	END

	CLOSE StatCursor
	DEALLOCATE StatCursor

END

