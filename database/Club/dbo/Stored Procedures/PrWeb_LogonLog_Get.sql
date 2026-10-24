






-- =============================================
-- Author:  harry
-- Create date: 2018/08/31
-- Description:	查询登入纪录
--
-- =============================================
CREATE PROCEDURE [dbo].[PrWeb_LogonLog_Get]
	@AgentUID		uniqueidentifier,
	@intUserID		int = NULL,
	@chvloginName	VARCHAR(50) = NULL,
	@dtStartDate	datetime = NULL,
	@dtEndDate		datetime = NULL
AS
BEGIN	

	DECLARE @tnyUserType tinyint --玩家身分 100:系統管理者 0:渠道代理 1234級代理 99:一般玩家  
	SELECT @tnyUserType = Class FROM AgentUsers WHERE ID = @AgentUID

	IF @tnyUserType = 100  -- 100: 系統管理者 可查询所有申请清单
	BEGIN
		SELECT L.LogID, U.UserID, U.LoginName, L.IPAddress, L.CreateTime FROM LogonLog L
		LEFT JOIN AgentUsers U ON U.ID = L.ID
		WHERE (@intUserID IS NULL OR @intUserID = U.UserID)
		AND (@chvloginName IS NULL OR @chvloginName = U.LoginName) 
		AND (@dtStartDate IS NULL OR @dtStartDate <= L.CreateTime)
		AND (@dtEndDate IS NULL OR @dtEndDate >= L.CreateTime)  
	END
	ELSE
	BEGIN
		SELECT L.LogID, U.UserID, U.LoginName, L.IPAddress, L.CreateTime FROM LogonLog L
		LEFT JOIN AgentUsers U ON U.ID = L.ID
		WHERE (L.ID = @AgentUID) 
		AND (@intUserID IS NULL OR @intUserID = U.UserID)
		AND (@chvloginName IS NULL OR @chvloginName = U.LoginName) 
		AND (@dtStartDate IS NULL OR @dtStartDate <= L.CreateTime)
		AND (@dtEndDate IS NULL OR @dtEndDate >= L.CreateTime)  
	END
	
END



