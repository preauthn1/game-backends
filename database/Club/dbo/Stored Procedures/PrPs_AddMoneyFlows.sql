




-- =============================================
-- Author:  alan
-- Create date: 2018/9/28
-- Description:	計算輸贏的流水與收益
-- =============================================
CREATE PROCEDURE [dbo].[PrPs_AddMoneyFlows]
	@intUserID	      INT,
	@lngAmount		  BIGINT  --遊戲輸贏金额流水
AS
SET NOCOUNT ON;

DECLARE @UUID uniqueidentifier
DECLARE @UserClass tinyint
DECLARE @lngFlows Bigint
DECLARE @dtCreateTime datetime
DECLARE @intYYYYMMDD int
DECLARE @intLogID	bigint --会员流水明细 与 代理获得每笔收益 对应id  (UsersFlowLog & AgentProfitLog)


SET @dtCreateTime = GETDATE()
SET @intYYYYMMDD = CAST ( CONVERT(varchar,@dtCreateTime,112) AS INT)

SELECT @UUID=ID, @UserClass=Class FROM [dbo].[AgentUsers] WITH (NOLOCK) WHERE UserID = @intUserID
IF @@ROWCOUNT > 0
BEGIN

	BEGIN TRAN
	
	--流水收益金額 = 遊戲輸贏流水金額2.5%
	SET @lngFlows = @lngAmount * 0.025 
	
	--找出上層節點(也包含自己)
	SELECT ID,Class,Rate,ParentID Into #tmpAgentList from [fn_RecursiveUp](@UUID)


	--紀錄用戶遊戲流水金额明細
	INSERT INTO  [dbo].[UserFlowsLog] (YYYYMMDD,ID ,UserID, Class, Amount, Flows, CreateTime)
	VALUES(@intYYYYMMDD, @UUID, @intUserID, @UserClass, @lngAmount, @lngFlows, @dtCreateTime)	
	--透过LogID与代理的每筆收益(AgentProfitLog)mapping
	SET @intLogID = SCOPE_IDENTITY()


	--更新當天總流水
	UPDATE dbo.UserFlowsSummary set Flows=Flows+@lngFlows, Amount=Amount+@lngAmount where ID = @UUID AND YYYYMMDD = @intYYYYMMDD
	IF @@ROWCOUNT = 0
		INSERT INTO  dbo.UserFlowsSummary (YYYYMMDD,ID ,UserID, Amount, Flows) VALUES(@intYYYYMMDD, @UUID, @intUserID, @lngAmount, @lngFlows)
	----------------------------------------------------------------------------------------------


	--更新代理當天獲得的總流水(加總下線的流水)
	--如果有代理未被加到當天流加總表格內, 先新增代理到當天加總表內
	INSERT INTO [dbo].[AgentFlowsSummary] (YYYYMMDD,ID,Flows)
	SELECT @intYYYYMMDD, ID, 0 AS Flows from #tmpAgentList where Class not in (99,100) and ID not in (Select ID from [dbo].[AgentFlowsSummary] with (nolock) where [YYYYMMDD] = @intYYYYMMDD)

	UPDATE dbo.[AgentFlowsSummary] Set Flows=Flows+@lngFlows 
	WHERE ID IN
	(
		SELECT ID from #tmpAgentList where Class not in (99,100)
	)
	AND YYYYMMDD = @intYYYYMMDD
	----------------------------------------------------------------------------------------------

	
	--紀錄代理的即時每筆收益
	Insert into AgentProfitLog(LogID,YYYYMMDD,ID, Flows, Class, ConfigRate, Rate, Income)

	--select @intLogID as LogID, @intYYYYMMDD as YYYYMMDD, U.ID, U.[Level], @lngFlows as Flows

	--, CASE WHEN U.Class = 0 THEN ISNULL(R.DistributeRate, 0)
	--	ELSE ISNULL(G.DistributeRate,0) END as Rate

	--, CASE WHEN U.Class = 0 THEN @lngFlows * ISNULL(R.DistributeRate, 0) / 100
	--	ELSE @lngFlows * ISNULL(G.DistributeRate,0) / 100 END as Income

	--from #tmpAgentList U
	--left join dbo.GrandConfig G on G.AgentID = U.Id and G.Level = U.Level
	--left join dbo.AgentRateRange R on R.ID = U.Id
	--where U.Class not in (99,100)


	 select @intLogID as LogID, @intYYYYMMDD as YYYYMMDD, A.ID, @lngFlows as Flows, 
	 A.Class,
	 A.Rate as ConfigRate, --設定的收益比例
	 ISNULL( (A.Rate-B.Rate), A.Rate ) as Rate, --與下級的差異比例 DiffRate 
	 @lngFlows* ISNULL( (A.Rate-B.Rate), A.Rate ) / 100 as Income
	 from #tmpAgentList A 
	 left join #tmpAgentList B
	 on A.id=B.ParentID 
	 where A.class not in (99,100)



	----------------------------------------------------------------------------------------------



	--更新代理總流水(4,3,2,1級, 總代)
	UPDATE dbo.AgentUsers Set TotalFlows=TotalFlows+@lngFlows 
	WHERE ID in 
	(
		SELECT ID from #tmpAgentList where  Class not in (99,100)
	)
	--更新自己的用戶總流水
	Update dbo.AgentUsers set UserTotalFlows = UserTotalFlows+@lngFlows WHERE ID = @UUID
	----------------------------------------------------------------------------------------------


	--更新代理總收益(4,3,2,1級, 總代)
	Merge into  dbo.AgentUsers as T
	Using (
		--select 
		--U.ID,
		--CASE WHEN U.Class = 0 THEN @lngFlows * ISNULL(R.DistributeRate, 0) / 100
		--ELSE @lngFlows * ISNULL(G.DistributeRate, 0) / 100 END 
		--as Income

		--FROM #tmpAgentList U

		--left join dbo.GrandConfig G on G.AgentID = U.Id and G.Level = U.Level
		--left join dbo.AgentRateRange R on R.ID = U.Id
		--where U.Class not in (99,100) --(4,3,2,1級, 總代)


		 select
		 A.ID,(@lngFlows* ISNULL( (A.Rate-B.Rate), A.Rate)) / 100 as Income
		 from #tmpAgentList A 
		 left join #tmpAgentList B
		 on A.id=B.ParentID 
		 where A.class not in (99,100)


		) as S
	ON T.ID = S.ID
	WHEN MATCHED THEN
	UPDATE SET T.TotalIncome = T.TotalIncome + S.Income, T.RemainAmount=T.RemainAmount+S.Income;
	----------------------------------------------------------------------------------------------


	COMMIT TRAN
	RETURN 1
END
