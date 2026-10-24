-- =============================================
-- Author:  <xxx>
-- Create date: <xxx>
-- Description:	<机器人登录>
--	2018/03/23	Jason		調整#robot SELECT方式降低IO
--	2018/05/04	Jason		調整寫法
--	2018/05/08	Jason		改為以Sort排序
--	2018/05/09	Jason		Game.UserGameInfo改為left join
--	2018/06/09	alan		找不到serverid, 不做任何操作
--	2019/07/23	Eddie		新增回傳AvatarFrameID欄位
-- =============================================
CREATE PROCEDURE [dbo].[PrGs_RobotLogon]
(
	@intServerID			INT,
	@intCurrentRobotCount	INT,
	@lngMinMoneyAmount		BIGINT,
	@lngMaxMoneyAmount		BIGINT,
	@intLogoutRobotCount	INT			OUTPUT
)
AS
BEGIN
	
	SET NOCOUNT ON

	DECLARE @ServerID		INT = @intServerID	--伺服器編號
	DECLARE @intMinCount	INT					--設定中最低數量
	DECLARE @intMaxCount	INT					--設定中最高數量
	DECLARE @intMinLevelID	INT					--設定中最低等級
	DECLARE @intMaxLevelID	INT					--設定中最高等級
	DECLARE @intLogonCount	INT					--需要登入數量
	DECLARE @Status			TINYINT = 1			--0:停用 1:啟用
	DECLARE @NowTime		DATETIME = GETDATE()
	DECLARE @NowMinute		INT = DATEPART(HOUR,@NowTime) * 60 + DATEPART(MINUTE,@NowTime)
	DECLARE @GameID			INT

	DECLARE @Temp_RobotList	TABLE (UserID INT,LevelID INT)

	SET @intLogoutRobotCount = 0

	SET @GameID = (SELECT TOP 1 GameID FROM [ServerInfo].[GameServerList] WITH (NOLOCK) WHERE ServerID = @intServerID)


	--找不到serverid, 不做任何操作
	IF @GameID is null or @GameID = 0
	BEGIN
		RETURN 0
	END
	
	
	
	/*  取得對應伺服器機器人設定  */
	----------------------------------------------------------------------------------
	SELECT	TOP 1
			@intMaxCount = MaxCount,
			@intMinCount = MinCount,
			@intMinLevelID = MinLevel,
			@intMaxLevelID = MaxLevel
	FROM Robot.ServerConfig WITH (NOLOCK)
	WHERE
		ServerID = @ServerID
		AND [StartMinute] < @NowMinute
		AND [EndMinute] >= @NowMinute
		AND Status = @Status

	--取無設定不執行
	IF (@intMaxCount IS NULL OR @intMinCount IS NULL)
	BEGIN
		RETURN 0
	END
	----------------------------------------------------------------------------------

	
		/*  计算当前服务器人数  */
	----------------------------------------------------------------------------------
	DECLARE @intRndCount INT
	SET @intRndCount = @intMinCount + CAST((@intMaxCount - @intMinCount) * RAND() AS INT)

	--计算登入登出的机器人数量
	SET @intLogoutRobotCount = @intRndCount - @intCurrentRobotCount

	--计算需要执行的登入登出操作

	--不做任何操作
	IF @intRndCount = @intCurrentRobotCount
	BEGIN
		RETURN 0
	END
	
	--需要登出
	IF @intRndCount < @intCurrentRobotCount
	BEGIN
		RETURN -1
	END

	--需要登入數量
	SET @intLogonCount = @intRndCount - @intCurrentRobotCount
	SET @intLogoutRobotCount = @intLogonCount
	SET @intLogonCount = CASE WHEN @intLogonCount > 50 THEN 50 ELSE @intLogonCount END
	----------------------------------------------------------------------------------


	/*  --获取用户信息  */
	----------------------------------------------------------------------------------
	--取機器人暫存及更新
	UPDATE Robot.RobotList
	SET ServerID = @ServerID
	OUTPUT INSERTED.UserID,INSERTED.LevelID INTO @Temp_RobotList
	FROM (
			SELECT	TOP (@intLogonCount)
					r.UserID,
					r.LevelID
			FROM Robot.RobotList AS r WITH (NOLOCK)
				INNER JOIN Money.Cash AS m WITH (NOLOCK) ON r.UserID = m.UserID
			WHERE
				r.ServerID = 0 
				AND m.Amount BETWEEN @lngMinMoneyAmount AND @lngMaxMoneyAmount
				AND r.LevelID BETWEEN @intMinLevelID AND @intMaxLevelID
			ORDER BY Sort ASC
		) AS r
	WHERE
		r.UserID = Robot.RobotList.UserID
	----------------------------------------------------------------------------------


	/*  更新遊戲用戶清單  */
	----------------------------------------------------------------------------------
	INSERT INTO game.GameUserList
		(	UserID,		ServerID,	HDType )
	SELECT	r.UserID,	@ServerID,	0
	FROM @Temp_RobotList AS r
	WHERE NOT EXISTS(SELECT 1 FROM Game.GameUserList WITH (NOLOCK) WHERE UserID = r.UserID)
	----------------------------------------------------------------------------------


	/*  回傳機器人資訊  */
	----------------------------------------------------------------------------------
	SELECT	r.UserID,
			u.UserType,
			dbo.Fn_GetUserNickName(u.LastLogonIPAddress) AS NickName,
			u.FaceID,
			u.Sex,
			m.Amount AS CashAmount,
			ISNULL(ugi.WinCount,0) AS WinCount,
			ISNULL(ugi.LoseCount,0) AS LoseCount,
			ISNULL(ugi.DrawCount,0) AS DrawCount,
			r.LevelID,
			u.AvatarFrame
	FROM @Temp_RobotList AS r
		INNER JOIN users.UserInfo AS u WITH (NOLOCK) ON r.UserID = u.UserID
		INNER JOIN Money.Cash AS m WITH (NOLOCK) ON r.UserID = m.UserID
		LEFT JOIN Game.UserGameInfo AS ugi WITH (NOLOCK) ON m.UserID = ugi.UserID AND GameID = @GameID
	----------------------------------------------------------------------------------

	RETURN 1
END
GO
GRANT EXECUTE
    ON OBJECT::[dbo].[PrGs_RobotLogon] TO [GameUser]
    AS [dbo];


GO
GRANT EXECUTE
    ON OBJECT::[dbo].[PrGs_RobotLogon] TO [xbuser]
    AS [dbo];