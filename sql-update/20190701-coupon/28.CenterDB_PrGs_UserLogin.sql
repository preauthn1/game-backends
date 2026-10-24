USE [CenterDB]
GO
/****** Object:  StoredProcedure [dbo].[PrGs_UserLogin]    Script Date: 2019/6/24 下午 05:20:45 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:  <xxx>
-- Create date: <xxx>
-- Description:	<用户进入游戏房间>
--	2018/4/23	alan	增加判斷serverid是否存在
--	2018/04/30	Jason	#28 新增取得TradeID
--	2018/05/07	Jason	#76 使用者各遊戲最後一局輸贏紀錄@intLastWinLose
--  2018/06/24  Rory    新增禮碼欄位
-- =============================================
ALTER PROCEDURE [dbo].[PrGs_UserLogin]
@sintServerID		SMALLINT,				--服务器ID
@intUserID			INT,                	--用户ID
@chvIPAddress		VARCHAR(15),			--IP
@chvCer				VARCHAR(32),			--证书
@tnyHDType			TINYINT,            	--硬件类型
@chvHDCode			VARCHAR(64),			--硬件码
@chvErrMsg			VARCHAR(128) OUTPUT,	--错误消息
@lngTradeID			BIGINT		OUTPUT		--用戶TradeID識別

AS
SET NOCOUNT ON

--檢查ServerID是否存在
IF not exists(select 1 from ServerInfo.GameServerList where ServerID = @sintServerID)
BEGIN
	SET @chvErrMsg = '服务器编号不存在'
	RETURN -1
END

IF @sintServerID = 0
BEGIN
	SET @chvErrMsg = '登录失败：服务器错误'
	RETURN -1
END

--检查封号
if exists(select 1 from Game.ForbidUser where UserID = @intUserID)
begin
	SET @chvErrMsg = '登录失败：此账号已经被封，请和管理员联系.'
	RETURN -1
end

--检查封锁IP
IF EXISTS(SELECT 1 FROM Game.ForbidIPAddress WHERE IPAddress = @chvIPAddress)
BEGIN
	SET @chvErrMsg = '登录失败：此IP被封,请和管理员联系.'
	RETURN -1	
END

--设置断线重连数据
DECLARE @bOffLine bit
SET @bOffLine = 0

--验证用户证书
IF NOT EXISTS(SELECT 1 FROM Game.Certification WHERE UserID = @intUserID AND CerCode = @chvCer)
BEGIN
	SET @chvErrMsg = '登录失败：用户信息不正确'
	RETURN -1
END

--检查用户是否在房间上锁
DECLARE @sintExServerID SMALLINT,@chvExServerName VARCHAR(64),@chvServerAddr varchar(64)

SELECT @sintExServerID = ServerID
FROM Game.GameUserList 
WHERE UserID = @intUserID

IF @sintExServerID IS NOT NULL 
BEGIN
	IF @sintExServerID <> @sintServerID
	BEGIN
		--获取房间信息
		SELECT @chvExServerName = ServerName
		,@chvServerAddr = cast(GameID as varchar) + '|' + ClientAddr
		FROM ServerInfo.GameServerList WHERE ServerID = @sintExServerID
	
		SET @chvErrMsg = '您已经在[' + dbo.Fn_GetRoomName(@sintExServerID)+ ']房间游戏,是否进入房间继续游戏'
		SELECT 0 as UserID,
			1 as UserType,
			'' as NickName,
			1 as FaceID,
			1 as Sex,
			0 AS CashAmount,
			0 AS CouponAmount,
			0 AS WinCount,
			0 AS LoseCount,
			0 AS DrawCount,
			@bOffLine AS IsOffLine,
			ISNULL(@chvServerAddr,'') as ServerAddr,
			'' as GameBuff,
			0 AS TradeID,
			0 AS LastWinLose,
			0 AS TimeSeq
		RETURN -3
	END
	ELSE
	--断线重连
	SET @bOffLine = 1
	
END

--获取游戏ID
DECLARE @sintGameID SMALLINT,@tnyGameMode TINYINT,@lngLoginMoney BIGINT
SELECT @sintGameID = GameID,
@lngLoginMoney = Money 
FROM ServerInfo.GameServerList
WHERE ServerID = @sintServerID

DECLARE 
@lngUserMoney BIGINT,
@lngCouponAmount BIGINT,
@lngTotalAmount BIGINT,
@intWinCount INT,
@intLoseCount INT, 
@intDrawCount INT

--获取用户金币数
SELECT @lngUserMoney = Amount, @lngCouponAmount = CouponAmount
FROM Money.Cash WHERE UserID = @intUserID
SET @lngUserMoney = ISNULL(@lngUserMoney, 0)
SET @lngCouponAmount = ISNULL(@lngCouponAmount, 0)
-- 非by, xby, dfdc 不允許帶入 CouponAmount
IF @sintGameID not in (14, 24, 25)
BEGIN
	SET @lngCouponAmount = 0
END

SET @lngTotalAmount = @lngUserMoney + @lngCouponAmount

--检查金币是否足够
IF @lngTotalAmount < @lngLoginMoney
BEGIN		
		SET @chvErrMsg = '金币不足' + char(13) + char(10) + '进入房间需要' + CAST(dbo.Fn_ShowMoney(@lngLoginMoney) AS VARCHAR) + '金币,您的金币数为' + CAST(dbo.Fn_ShowMoney(@lngUserMoney) AS VARCHAR) + '金币'
		RETURN -2
END


EXEC [PERGAMELOGLINKEDSERVER].[PerGameLog].[dbo].[PrGs_GetTradeID]
	@lngTradeID = @lngTradeID	OUTPUT

--初始化用户数据
IF NOT EXISTS(SELECT 1 FROM Game.UserGameInfo WHERE UserID = @intUserID AND GameID = @sintGameID)
INSERT INTO Game.UserGameInfo
        ( UserID ,
          GameID ,
          WinCount ,
          LoseCount ,
          DrawCount ,
          Score ,
          GameTime,
		  [LastWinLose]
        )
SELECT @intUserID,
@sintGameID,
0,
0,
0,
0,
0,
0

--房间上锁
IF @sintExServerID IS NULL
INSERT INTO Game.GameUserList
        ( UserID, ServerID, HDType )
SELECT
	@intUserID,@sintServerID, @tnyHDType
	




--获取用户信息
SELECT Ugi.UserID,
u.UserType,
dbo.Fn_GetUserNickName(@chvIPAddress) as NickName,
u.FaceID,
u.Sex,
@lngUserMoney AS CashAmount,
@lngCouponAmount AS CouponAmount,
ugi.WinCount AS WinCount,
ugi.LoseCount AS LoseCount,
ugi.DrawCount AS DrawCount,
@bOffLine AS IsOffLine,
ISNULL(@chvServerAddr,'') as ServerAddr,
CASE WHEN @sintGameID = 14 THEN '000' ELSE '' END as GameBuff,
@lngTradeID AS TradeID,
ugi.LastWinLose,
dbo.Fn_GetTimeSeq() AS TimeSeq
FROM Game.UserGameInfo ugi WITH(NOLOCK)
LEFT JOIN users.UserInfo u WITH(NOLOCK) ON ugi.UserID = u.UserID
WHERE ugi.UserID = @intUserID and GameID = @sintGameID

RETURN 1
