






-- =============================================
-- Author:  harry
-- Create date: 2018/08/24
-- Description:	查询提款纪录
--
-- =============================================
CREATE PROCEDURE [dbo].[PrWeb_WithdrawLog_Get]
	@agentUID		uniqueidentifier,
	@intUserID		int = NULL,
	@chvloginName	VARCHAR(50) = NULL,
	@tnyType		tinyint = NULL,
	@fromYYYYMMDD	VARCHAR(50) = NULL,
	@toYYYYMMDD		VARCHAR(50) = NULL
AS
BEGIN

	DECLARE @tnyClass tinyint --用戶身分 100: 系統管理者 0:渠道代理 1234級代理 99:一般玩家  
	SELECT @tnyClass = Class FROM AgentUsers WHERE ID = @AgentUID
	IF @tnyClass = 100  --系統管理者 可查询所有代理
	BEGIN
		SELECT W.YYYYMMDD, U.UserID, U.LoginName, W.CurrentAmount, W.UpdateAmount, W.RemainAmount, U.Class, P.Name AS PlatformName, R.AreaName, W.Memo, W.CreateTime FROM WithdrawLog W
		LEFT JOIN AgentUsers U ON U.ID = W.ID
		LEFT JOIN CenterDB.Area.Area R ON R.AreaID = U.AreaID 
		LEFT JOIN CenterDB.Area.Platform P ON P.PlatformID = U.PlatformID
		WHERE W.Type = 0
		AND (@intUserID IS NULL OR @intUserID = U.UserID) 
		AND (@chvloginName IS NULL OR @chvloginName = U.LoginName) 
		AND (@tnyType IS NULL OR @tnyType = U.Class) 
		AND (@fromYYYYMMDD IS NULL OR @fromYYYYMMDD <= W.YYYYMMDD) 
		AND (@toYYYYMMDD IS NULL OR @toYYYYMMDD >= W.YYYYMMDD) 
	END
	ELSE
	BEGIN
		SELECT W.YYYYMMDD, U.UserID, U.LoginName, W.CurrentAmount, W.UpdateAmount, W.RemainAmount, U.Class, P.Name AS PlatformName, R.AreaName, W.Memo, W.CreateTime FROM WithdrawLog W
		LEFT JOIN AgentUsers U ON U.ID = W.ID
		LEFT JOIN CenterDB.Area.Area R ON R.AreaID = U.AreaID 
		LEFT JOIN CenterDB.Area.Platform P ON P.PlatformID = U.PlatformID
		WHERE W.Type = 0 AND U.ID = @agentUID
		AND (@intUserID IS NULL OR @intUserID = U.UserID) 
		AND (@chvloginName IS NULL OR @chvloginName = U.LoginName) 
		AND (@tnyType IS NULL OR @tnyType = U.Class) 
		AND (@fromYYYYMMDD IS NULL OR @fromYYYYMMDD <= W.YYYYMMDD) 
		AND (@toYYYYMMDD IS NULL OR @toYYYYMMDD >= W.YYYYMMDD) 
	END

END



