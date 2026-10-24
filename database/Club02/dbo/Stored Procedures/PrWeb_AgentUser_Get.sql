


-- =============================================
-- Author:  harry
-- Create date: 2018/08/10
-- Description:	查询代理所有客户(不包含自己)
--
-- =============================================
CREATE PROCEDURE [dbo].[PrWeb_AgentUser_Get]
	@AgentUID		uniqueidentifier,
	@intUserID		INT = NULL,
	@chvloginName	VARCHAR(50) = NULL,
	@tnyType		tinyint = NULL,
	@bitIsAdmin		bit,
	@intSumTotal	int = 0 OUT,
	@chvErrMsg		Varchar(50) OUT
AS
BEGIN

	DECLARE @tnyUserType tinyint --玩家身分 100:系統管理者 0:渠道代理 1:一級代理  2:二級代理 99:一般玩家  
	DECLARE @tnyClass tinyint --

	SELECT @tnyUserType = Class FROM AgentUsers WHERE ID = @AgentUID

	IF @@ROWCOUNT = 0 
	BEGIN
		SET @chvErrMsg = '代理不存在'
		RETURN -1
	END

	SET @tnyClass = CASE 
	WHEN @chvloginName IS NOT NULL OR @intUserID IS NOT NULL OR @tnyType IS NOT NULL THEN NULL
	WHEN @tnyUserType = 100 THEN 0 --系統管理者可看渠道代理
	ELSE @tnyUserType + 1 END

	IF @tnyUserType = 100  -- 100: 系統管理者 可查询所有渠道代理
	BEGIN
		SELECT @intSumTotal = SUM(TotalIncome) FROM [Club].[dbo].[AgentUsers] WHERE Class != 100

		SELECT U.ID, U.UserID, U.LoginName, U.Class, U.Tel, U.Email, U.QQ, U.Weixin, U.Level, U.Status, U.TotalFlows, U.TotalIncome, U.RemainAmount, U.UserTotalFlows, P.Name AS PlatformName
		, A.AreaName, U.InviteCode, U.CreateTime 
		, R.DistributeRate, R.Class1RateRange, R.Class2RateRange, R.Class3RateRange, R.Class4RateRange
		, U.AllowWithdraw
		,ISNULL(QRUrl,'') as QRUrl --二維碼位置

		FROM AgentUsers U
		LEFT JOIN CenterDB.Area.Area A ON A.AreaID = U.AreaID 
		LEFT JOIN CenterDB.Area.Platform P ON P.PlatformID = U.PlatformID
		LEFT JOIN dbo.AgentRateRange R ON R.ID = U.ID
		WHERE (@intUserID IS NULL OR @intUserID = U.UserID) 
		AND (@chvloginName IS NULL OR @chvloginName = U.LoginName) 
		AND (@tnyClass IS NULL OR @tnyClass = U.Class)
		RETURN 1
	END
	ELSE
	BEGIN
		IF @bitIsAdmin = 0
			BEGIN
				SELECT U.ID, U.UserID, U.LoginName, U.Class, U.Tel, U.Email, U.QQ, U.Weixin, U.Level, U.Status, U.TotalFlows, U.TotalIncome, U.RemainAmount, U.UserTotalFlows, P.Name AS PlatformName
				, A.AreaName, U.InviteCode, U.CreateTime 
				, R.DistributeRate, R.Class1RateRange, R.Class2RateRange, R.Class3RateRange, R.Class4RateRange
				, U.AllowWithdraw
				,ISNULL(QRUrl,'') as QRUrl  --二維碼位置
		
				FROM AgentUsers U
				LEFT JOIN CenterDB.Area.Area A ON A.AreaID = U.AreaID 
				LEFT JOIN CenterDB.Area.Platform P ON P.PlatformID = U.PlatformID
				LEFT JOIN dbo.AgentRateRange R ON R.ID = U.ID
				WHERE  U.ID in 
				(
					SELECT ID from [fn_RecursiveDown](@AgentUID)
					WHERE ID != @AgentUID AND (@tnyClass IS NULL OR Class = @tnyClass) AND  Class != 99 AND (ParentID = @AgentUID)
				) 
				AND (@intUserID IS NULL OR @intUserID = U.UserID) 
				AND (@chvloginName IS NULL OR @chvloginName = U.LoginName) 
				AND (@tnyType IS NULL OR @tnyType = U.Class)
			END
			ELSE
			BEGIN

				SELECT U.ID, U.UserID, U.LoginName, U.Class, U.Tel, U.Email, U.QQ, U.Weixin, U.Level, U.Status, U.TotalFlows, U.TotalIncome, U.RemainAmount, U.UserTotalFlows, P.Name AS PlatformName
				, A.AreaName, U.InviteCode, U.CreateTime 
				, R.DistributeRate, R.Class1RateRange, R.Class2RateRange, R.Class3RateRange, R.Class4RateRange
				, U.AllowWithdraw
				,ISNULL(QRUrl,'') as QRUrl  --二維碼位置

				FROM AgentUsers U
				LEFT JOIN CenterDB.Area.Area A ON A.AreaID = U.AreaID 
				LEFT JOIN CenterDB.Area.Platform P ON P.PlatformID = U.PlatformID
				LEFT JOIN dbo.AgentRateRange R ON R.ID = U.ID
				WHERE  U.ID in 
				(
					SELECT ID from [fn_RecursiveDown](@AgentUID)
					WHERE ID != @AgentUID AND ((@tnyClass IS NULL OR Class = @tnyClass)) AND  Class != 99
				) 
				AND (@intUserID IS NULL OR @intUserID = U.UserID) 
				AND (@chvloginName IS NULL OR @chvloginName = U.LoginName) 
				AND (@tnyType IS NULL OR @tnyType = U.Class)
			END

			SELECT @intSumTotal = SUM(TotalIncome) FROM AgentUsers U
			WHERE  U.ID in 
			(
				SELECT ID from [fn_RecursiveDown](@AgentUID) WHERE ID != @AgentUID
			) 
			RETURN 1
	END
END



