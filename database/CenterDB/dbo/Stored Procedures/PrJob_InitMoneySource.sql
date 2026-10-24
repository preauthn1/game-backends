-- =============================================
-- Author:  <xxx>
-- Create date: <xxx>
-- Description:	<初始化金币统计>
--	2018/05/15	Jason	代理统计初始化 若無代理資料則不寫入
--	2018/06/05	Jason	#406 金幣統計改為LogDB計算及同步更新CenterDB 調整記錄寫入方式及日期格式
--	2018/8/7	alan	關閉AgentTransferSourceLog, 因為沒用到
-- =============================================
CREATE PROCEDURE [dbo].[PrJob_InitMoneySource]
AS
BEGIN
	SET NOCOUNT ON

	DECLARE @StartStatTime		DATETIME = DATEADD(DAY,DATEDIFF(DAY,0,GETDATE()),0)
	DECLARE @EndStatTime		DATETIME = DATEADD(DAY,DATEDIFF(DAY,0,GETDATE()),1)
	DECLARE @NextEndStatTime	DATETIME = DATEADD(DAY,DATEDIFF(DAY,0,GETDATE()),2)

	--现金统计初始化
	INSERT INTO [Stat].[MoneySourceLog]
		([SourceID],[Amount],[RobotAmount],[RobotTaxAmount],[TaxAmount],[StatTime])
	SELECT SourceID,0,0,0,0,@StartStatTime
	FROM (
			SELECT SourceID
			FROM Stat.MoneySourceType WITH (NOLOCK)
			EXCEPT
			SELECT [SourceID]
			FROM [Stat].[MoneySourceLog] WITH (NOLOCK)
			WHERE
				StatTime = @StartStatTime
		) AS A
	UNION ALL
	SELECT SourceID,0,0,0,0,@EndStatTime
	FROM (
			SELECT SourceID
			FROM Stat.MoneySourceType WITH (NOLOCK)
			EXCEPT
			SELECT [SourceID]
			FROM [Stat].[MoneySourceLog] WITH (NOLOCK)
			WHERE
				StatTime = @StartStatTime
		) AS A

	--银行统计初始化
	INSERT INTO [Stat].[BankSourceLog]
		([SourceID],[Amount],[StatTime])
	SELECT	[SourceID],0,@StartStatTime
	FROM (
			SELECT [SourceID]
			FROM [Stat].[BankSourceType] WITH (NOLOCK)
			EXCEPT
			SELECT [SourceID]
			FROM [Stat].[BankSourceLog] WITH (NOLOCK)
			WHERE StatTime = @StartStatTime
		) AS A
	UNION ALL
	SELECT	[SourceID],0,@EndStatTime
	FROM (
			SELECT [SourceID]
			FROM [Stat].[BankSourceType] WITH (NOLOCK)
			EXCEPT
			SELECT [SourceID]
			FROM [Stat].[BankSourceLog] WITH (NOLOCK)
			WHERE StatTime = @StartStatTime
		) AS A


	--INSERT INTO Stat.AgentTransferSourceLog(AgentID,SourceID,Amount,StatTime)
	--SELECT	AgentID,SourceID,Amount,StatTime
	--FROM (
	--		SELECT U.AgentID,T.SourceID,0 AS Amount,@StartStatTime AS StatTime
	--		FROM Agent.TransferType AS T WITH (NOLOCK)
	--			LEFT JOIN Agent.Users AS U WITH (NOLOCK) ON 1=1
	--		WHERE
	--			NOT EXISTS(SELECT 1 FROM Stat.AgentTransferSourceLog WITH (NOLOCK) WHERE StatTime >= @StartStatTime AND StatTime < @EndStatTime)
	--		UNION ALL
	--		SELECT U.AgentID,T.SourceID,0 AS Amount,@EndStatTime AS StatTime
	--		FROM Agent.TransferType AS T WITH (NOLOCK)
	--			LEFT JOIN Agent.Users AS U WITH (NOLOCK) ON 1=1
	--		WHERE
	--			NOT EXISTS(SELECT 1 FROM Stat.AgentTransferSourceLog WITH (NOLOCK) WHERE StatTime >= @StartStatTime AND StatTime < @EndStatTime)
	--	) AS A
	--WHERE
	--	AgentID IS NOT NULL

	
	--兑换统计初始化
	INSERT INTO [Stat].[ConvertLog]
		([Amount],[TaxAmount],[RealTaxAmount],[StatTime])
	SELECT	0,0,0,[StatTime]
	FROM (
			SELECT	[StatTime]
			FROM (
					SELECT [StatTime] = @StartStatTime
					UNION ALL
					SELECT [StatTime] = @EndStatTime
				) AS A
			EXCEPT
			SELECT [StatTime]
			FROM [Stat].[ConvertLog] WITH (NOLOCK)
		) AS A

	RETURN 1
END

GO
GRANT EXECUTE
    ON OBJECT::[dbo].[PrJob_InitMoneySource] TO [xbuser]
    AS [dbo];

