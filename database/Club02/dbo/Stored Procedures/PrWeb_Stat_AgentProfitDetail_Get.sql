





-- =============================================
-- Author:  alan
-- Create date: 2018/8/21
-- Description:
--	1. 取得代理獲取的流水收益來源明細
--	2. 依據代理的歷史比例計算流水收益
-- =============================================
CREATE PROCEDURE [dbo].[PrWeb_Stat_AgentProfitDetail_Get]
	@intAgentID	    uniqueidentifier,	--查询代理身分
	@fromYYYYMMDD	int,				--开始时间
	@toYYYYMMDD		int					--结束时间
AS
BEGIN
	SET NOCOUNT ON;

	--取得某段時間區間的收益比例資訊
	--CREATE TABLE #Rate (YYYYMMDD INT, ID Uniqueidentifier, Rate int)
	--INSERT INTO #Rate(YYYYMMDD,ID,Rate)
	--EXEC dbo.PrPs_RateHistory_Get @intAgentID,@fromYYYYMMDD,@toYYYYMMDD

	SELECT S.*, U.Class, U.PlatformID, U.AreaID, U.ParentID,P.Name AS PlatformName, A.AreaName from 
	(
		SELECT  F.LogID, F.YYYYMMDD,F.ID as AgentID, ISNULL(F.[Level],0) as AgentLevel
		, L.ID, L.UserID, L.flows
		,F.Rate
		,F.Flows * F.Rate / 100 as Income
		,F.CreateTime
		FROM dbo.AgentProfitLog F
		LEFT JOIN dbo.UserFlowsLog L ON F.LogID = L.LogID
		--LEFT JOIN #Rate R with (nolock)
		--ON F.YYYYMMDD = R.YYYYMMDD
		Where 
		F.ID = @intAgentID AND
		F.YYYYMMDD >= @fromYYYYMMDD and F.YYYYMMDD <=@toYYYYMMDD
	
	) as S
	LEFT JOIN dbo.AgentUsers U with (nolock) ON S.ID = U.ID
	LEFT JOIN CenterDB.Area.Area A ON A.AreaID = U.AreaID 
	LEFT JOIN CenterDB.Area.Platform P ON P.PlatformID = U.PlatformID
	ORDER BY S.YYYYMMDD DESC, S.LogID DESC

END


