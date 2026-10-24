





-- =============================================
-- Author:  harry
-- Create date: 2018/08/10
-- Description:	查询代理所有客户(不包含自己)
--
-- =============================================
CREATE PROCEDURE [dbo].[PrWeb_AgentCustomer_Get]
	@AgentUID		uniqueidentifier,
	@intUserID		INT = NULL,
	@chvloginName	VARCHAR(50) = NULL,
	@tnyClass		tinyint = NULL,
	@dtStartDate		datetime = NULL,
	@dtEndDate		datetime = NULL
AS
BEGIN	

	DECLARE @tnyUserType tinyint --玩家身分 100:系統管理者 0:渠道代理 1,2,3,4級代理 99:一般玩家  
	SELECT @tnyUserType = Class FROM AgentUsers WHERE ID = @AgentUID

	IF @tnyUserType = 100  -- 100: 系統管理者 可查询所有渠道代理
	BEGIN
		SELECT U.UserID, U.LoginName, U.Class, U.Tel, U.Email, U.QQ, U.Weixin, U.Level, U.TotalFlows, U.TotalIncome, U.RemainAmount, U.UserTotalFlows, P.Name AS PlatformName, A.AreaName, U.CreateTime FROM AgentUsers U
		LEFT JOIN CenterDB.Area.Area A ON A.AreaID = U.AreaID 
		LEFT JOIN CenterDB.Area.Platform P ON P.PlatformID = U.PlatformID
		WHERE (@intUserID IS NULL OR @intUserID = U.UserID) 
		AND (@chvloginName IS NULL OR @chvloginName = U.LoginName) 
		AND (@tnyClass IS NULL OR @tnyClass = U.Class) 
		AND (@dtStartDate IS NULL OR @dtStartDate <= U.CreateTime) 
		AND (@dtEndDate IS NULL OR @dtEndDate >= U.CreateTime) 
		AND Class = 99
	END
	ELSE
	BEGIN
		SELECT U.UserID, U.LoginName, U.Class, U.Tel, U.Email, U.QQ, U.Weixin, U.Level, U.TotalFlows, U.TotalIncome, U.RemainAmount, U.UserTotalFlows, P.Name AS PlatformName, A.AreaName, U.CreateTime FROM AgentUsers U
		LEFT JOIN CenterDB.Area.Area A ON A.AreaID = U.AreaID 
		LEFT JOIN CenterDB.Area.Platform P ON P.PlatformID = U.PlatformID
		WHERE  U.ParentID = @AgentUID AND U.Class = 99
		AND (@intUserID IS NULL OR @intUserID = U.UserID) 
		AND (@chvloginName IS NULL OR @chvloginName = U.LoginName) 
		AND (@tnyClass IS NULL OR @tnyClass = U.Class) 
		AND (@dtStartDate IS NULL OR @dtStartDate <= U.CreateTime) 
		AND (@dtEndDate IS NULL OR @dtEndDate >= U.CreateTime) 
	END


	
END

