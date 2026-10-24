






-- =============================================
-- Author:  alan
-- Create date: 2018/08/22
-- Description:	即时更新代理的收益
-- =============================================
CREATE PROCEDURE [dbo].[PrJob_Stat_AgentProfit_Today]
AS
Begin

DECLARE @dtCreateTime datetime
DECLARE @intToday int

SET @dtCreateTime = Getdate()
SET @intToday = CAST ( CONVERT(varchar,@dtCreateTime,112) AS INT)


DECLARE @AgentID uniqueidentifier
DECLARE @intClass int
--DECLARE @intCurrentAmount BIGINT

DECLARE StatCursor CURSOR FOR 
	Select ID,Class from AgentUsers with (nolock) Where class not in (99,100) --只撈取總代理,1,2,3,4級代理

 
OPEN StatCursor                   

FETCH NEXT FROM StatCursor INTO @AgentID, @intClass

WHILE @@FETCH_STATUS = 0


BEGIN
	
	declare @intSumFlows bigint	=0	--當天流水
	declare @intSumIncome bigint =0	--當天收益


	--從即時收益取得
	select  @intSumFlows = ISNULL(sum(flows),0), @intSumIncome = ISNULL(sum(Income),0)
	from [AgentProfitLog] with (nolock) where YYYYMMDD = @intToday and ID = @AgentID

	update [dbo].[Stat_AgentProfitDaily] set Rate=0, Income=@intSumIncome, Flows=@intSumFlows where ID=@AgentID and YYYYMMDD = @intToday
	IF @@ROWCOUNT = 0
		Insert into [dbo].[Stat_AgentProfitDaily] (YYYYMMDD, ID, Flows, Rate, Income) Values(@intToday,@AgentID,@intSumFlows,0,@intSumIncome)


    FETCH NEXT FROM StatCursor INTO @AgentID, @intClass

END

CLOSE StatCursor
DEALLOCATE StatCursor
 

END


