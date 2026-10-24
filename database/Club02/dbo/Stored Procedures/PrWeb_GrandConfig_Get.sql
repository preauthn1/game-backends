








-- =============================================
-- Author:  harry
-- Create date: 2018/08/10
-- Description:	查询代理阶级设定
--
-- =============================================
CREATE PROCEDURE [dbo].[PrWeb_GrandConfig_Get]
	@AgentUID		uniqueidentifier,
	@intUserID		INT = NULL,
	@chvloginName	VARCHAR(50) = NULL
AS
BEGIN	

	DECLARE @tnyClass tinyint --玩家身分 100: 系統管理者 0:渠道代理 1:一級代理  2:二級代理 99:一般玩家  
	DECLARE @intRate int --

	SELECT @tnyClass = Class, @intRate = G.DistributeRate
	FROM AgentUsers  U
	LEFT JOIN GrandConfig G with (nolock) ON G.AgentID = U.ID AND G.Level = U.Level
	WHERE U.ID = @AgentUID

	IF @tnyClass = 100  -- 0: 系統管理者 可查询所有代理
	BEGIN
		SELECT G.ID, G.AgentID, G.ParentID, U.UserID, U.LoginName, U.Class, G.Level, G.GradeName, G.DistributeRate, G.UpGradeLimit,  G.DistributeRate
		FROM dbo.GrandConfig G
		JOIN dbo.AgentUsers U ON G.AgentID = U.ID AND (U.Class != 0 AND U.Class != 99 AND U.Class != 100)
		AND (@intUserID IS NULL OR @intUserID = U.UserID) 
		AND (@chvloginName IS NULL OR @chvloginName = U.LoginName) 
	END
	ELSE
	BEGIN
		SELECT G.ID, G.AgentID, G.ParentID, U.UserID, U.LoginName, U.Class, G.Level, G.GradeName, G.DistributeRate, G.UpGradeLimit,  G.DistributeRate
		FROM dbo.GrandConfig G
		JOIN dbo.AgentUsers U ON G.AgentID = U.ID
		LEFT JOIN 
			(SELECT TOP 1 RM.*, @AgentUID AS rangeID from [fn_RecursiveUp](@AgentUID) U
			LEFT JOIN [AgentRateRange] RM ON RM.ID = U.Id
			WHERE Class = 0) RU ON  RU.rangeID =  G.ParentID
		WHERE (G.ParentID = @AgentUID)
		AND (@intUserID IS NULL OR @intUserID = U.UserID) 
		AND (@chvloginName IS NULL OR @chvloginName = U.LoginName) 
	END


END




