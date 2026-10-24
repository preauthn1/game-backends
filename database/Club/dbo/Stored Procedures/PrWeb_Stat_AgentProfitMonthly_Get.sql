



-- =============================================
-- Author:  alan
-- Create date: 2018/8/22
-- Description: 取得我的下級代理每月的收益資訊
-- =============================================
CREATE PROCEDURE [dbo].[PrWeb_Stat_AgentProfitMonthly_Get]
	@intAgentID	    uniqueidentifier,
	@intUserID		int = NULL,
	@chvLoginName	varchar(50) = NULL,
	@intPlatformID	int = NULL,
	@intAreaID		int = NULL,
	@fromYYYYMM		int,
	@toYYYYMM		int
AS
BEGIN
	SET NOCOUNT ON;

	Declare @intClass int 
	Select @intClass = Class from dbo.AgentUsers with (nolock) where ID = @intAgentID
	
	CREATE TABLE #AgentUser(ID uniqueidentifier)
	IF @intClass = 100
		INSERT INTO #AgentUser SELECT ID from AgentUsers where Class in (0,1,2,3,4)
	ELSE
		INSERT INTO #AgentUser SELECT ID from (
			SELECT ID from [fn_Recursivedown](@intAgentID) where Class <> 99 AND ParentID = @intAgentID
			UNION
			SELECT @intAgentID as ID --包含自己
		) as R



	select R.YYYYMM, R.ID, U.UserID, U.LoginName, U.Class, P.Name AS PlatformName, A.AreaName, R.SumFlows, R.SumIncome FROM 
	(
		--從每月報表取得
		SELECT YYYYMM, ID, flows as SumFlows, Income as SumIncome From Stat_AgentProfitMonthly With (nolock)
		WHERE ID in 
		(SELECT ID FROM #AgentUser)
		and YYYYMM >= @fromYYYYMM and YYYYMM <=@toYYYYMM
	) as R

	LEFT JOIN dbo.AgentUsers U with (nolock) on R.ID = U.ID
	LEFT JOIN CenterDB.Area.Area A ON A.AreaID = U.AreaID 
	LEFT JOIN CenterDB.Area.Platform P ON P.PlatformID = U.PlatformID
	WHERE (@intUserID IS NULL OR @intUserID = U.UserID) 
	AND (@chvloginName IS NULL OR @chvloginName = U.LoginName) 
	AND (@intPlatformID IS NULL OR @intPlatformID = A.PlatformID) 
	AND (@intAreaID IS NULL OR @intAreaID = U.AreaID) 
	ORDER BY R.YYYYMM DESC

END
