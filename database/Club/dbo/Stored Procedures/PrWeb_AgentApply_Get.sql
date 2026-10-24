







-- =============================================
-- Author:  harry
-- Create date: 2018/08/20
-- Description:	查询代理申请清单
--
-- =============================================
CREATE PROCEDURE [dbo].[PrWeb_AgentApply_Get]
	@AgentUID		uniqueidentifier,
	@intUserID		int = NULL,
	@dtStartDate	datetime = NULL,
	@dtEndDate		datetime = NULL
AS
BEGIN

	DECLARE @tnyUserType tinyint --玩家身分 100:系統管理者 0:渠道代理 1,2,3,4級代理 99:一般玩家  
	SELECT @tnyUserType = Class FROM AgentUsers WHERE ID = @AgentUID

	IF @tnyUserType = 100  -- 100: 系統管理者 可查询所有申请清单
	BEGIN
		SELECT A.ApplyID, U.UserID, U.LoginName, A.Tel, A.Weixin, A.QQ, A.Email, A.ApplyID, A.ApplyClass, U.Class, P.Name AS PlatformName, R.AreaName, A.Status, A.Memo, A.CreateTime, A.Tel, A.Weixin, A.QQ, A.Email FROM AgentApply A
		LEFT JOIN AgentUsers U ON U.ID = A.ID
		LEFT JOIN CenterDB.Area.Area R ON R.AreaID = U.AreaID 
		LEFT JOIN CenterDB.Area.Platform P ON P.PlatformID = U.PlatformID

		WHERE (@intUserID IS NULL OR @intUserID = A.UserID) 
		AND (@dtStartDate IS NULL OR @dtStartDate <= A.CreateTime) 
		AND (@dtEndDate IS NULL OR @dtEndDate >= A.CreateTime) 
		RETURN 1
	END
	ELSE
	BEGIN
		SELECT A.ApplyID, U.UserID, U.LoginName, A.Tel, A.Weixin, A.QQ, A.Email, A.ApplyID, A.ApplyClass, U.Class, P.Name AS PlatformName, R.AreaName, A.Status, A.Memo, A.CreateTime, A.Tel, A.Weixin, A.QQ, A.Email FROM AgentApply A
		LEFT JOIN AgentUsers U ON U.ID = A.ID
		LEFT JOIN CenterDB.Area.Area R ON R.AreaID = U.AreaID 
		LEFT JOIN CenterDB.Area.Platform P ON P.PlatformID = U.PlatformID

		WHERE (@AgentUID IS NULL OR @AgentUID = U.ParentID)  
		AND (@intUserID IS NULL OR @intUserID = A.UserID) 
		AND (@dtStartDate IS NULL OR @dtStartDate <= A.CreateTime) 
		AND (@dtEndDate IS NULL OR @dtEndDate >= A.CreateTime) 
		RETURN 1
	END
END



