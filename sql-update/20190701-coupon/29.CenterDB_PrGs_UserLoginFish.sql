USE [CenterDB]
GO
/****** Object:  StoredProcedure [dbo].[PrGs_UserLoginFish]    Script Date: 2019/6/24 下午 05:21:45 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:  <xxx>
-- Create date: <xxx>
-- Description:	<用户进入游戏房间,捕鱼对接专用>
--  2018/06/24  Rory    新增禮碼欄位
-- =============================================
ALTER PROCEDURE [dbo].[PrGs_UserLoginFish]
@sintServerID		SMALLINT,				--服务器ID
@intUserID			INT,                	--用户ID
@chvIPAddress		VARCHAR(15),			--IP
@chvCer				VARCHAR(32),			--证书
@tnyHDType			TINYINT,            	--硬件类型
@chvHDCode			VARCHAR(64),			--硬件码
@chvErrMsg			VARCHAR(128) OUTPUT	--错误消息
AS

begin try
	declare @ErrorNumber int;
	declare @ErrorMessage varchar(8000);
	declare @ErrorSeverity int;
	declare @ErrorState int;
	declare @ErrorLine int;
	declare @ErrorProcedure varchar(128);

SET NOCOUNT ON


--延时封禁账号
--IF EXISTS(SELECT 1 FROM Game.WaitingForbidUser WHERE UserID = @intUserID)
--begin
--	EXEC dbo.PrWeb_Game_ForbidUser @intUserID,'','',@chvErrMsg
	
--	SET @chvErrMsg = '登录失败：此账号已经被封，请联系客服.'
--	select -1, @chvErrMsg as ErrMsg
--	RETURN -1
--end

--检查封号
if exists(select 1 from Game.ForbidUser  WITH(NOLOCK) where UserID = @intUserID)
begin
	SET @chvErrMsg = '登录失败：此账号已经被封，请联系客服.'
	select -1, @chvErrMsg as ErrMsg
	RETURN -1
end

--检查封锁IP
IF EXISTS(SELECT 1 FROM Game.ForbidIPAddress  WITH(NOLOCK) WHERE IPAddress = @chvIPAddress)
BEGIN
	SET @chvErrMsg = '登录失败：此IP被封,请联系客服.'
	select -1, @chvErrMsg as ErrMsg
	RETURN -1	
END

--设置断线重连数据
DECLARE @bOffLine bit
SET @bOffLine = 0

--验证用户证书
IF NOT EXISTS(SELECT 1 FROM Game.Certification WITH(NOLOCK) WHERE UserID = @intUserID AND CerCode = @chvCer)
BEGIN
	SET @chvErrMsg = '登录失败：用户信息不正确'
	select -1, @chvErrMsg as ErrMsg
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
		FROM ServerInfo.GameServerList WITH(NOLOCK)  WHERE ServerID = @sintExServerID
	
		SET @chvErrMsg = '您已经在[' + dbo.Fn_GetRoomName(@sintExServerID)+ ']房间游戏'
		select -1, @chvErrMsg as ErrMsg
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
FROM ServerInfo.GameServerList WITH(NOLOCK) 
WHERE ServerID = @sintServerID

DECLARE 
@lngUserMoney BIGINT,
@lngUserTotalMoney BIGINT,
@lngCouponAmount BIGINT,
@lngTotalAmount BIGINT,
@intWinCount INT,
@intLoseCount INT, 
@intDrawCount INT

--获取用户金币数
SELECT @lngUserMoney = Amount, @lngCouponAmount = CouponAmount, @lngUserTotalMoney = (SELECT SumCharge FROM Users.UserInfo where UserID = @intUserID)
FROM Money.Cash WHERE UserID = @intUserID
SET @lngUserMoney = ISNULL(@lngUserMoney, 0)
SET @lngUserTotalMoney = ISNULL(@lngUserTotalMoney, 0)
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
	SET @chvErrMsg = '金币不足,进入房间需要' + CAST(dbo.Fn_ShowMoney(@lngLoginMoney) AS VARCHAR) + ',您的金币数为' + CAST(dbo.Fn_ShowMoney(@lngUserMoney) AS VARCHAR)
	select -1, @chvErrMsg as ErrMsg
	RETURN -2
END

--初始化用户数据
--IF NOT EXISTS(SELECT 1 FROM Game.UserGameInfo WHERE UserID = @intUserID AND GameID = @sintGameID)
INSERT INTO Game.UserGameInfo
        ( UserID ,
          GameID ,
          WinCount ,
          LoseCount ,
          DrawCount ,
          Score ,
          GameTime
        )
SELECT @intUserID,
@sintGameID,
0,
0,
0,
0,
0 where NOT EXISTS(SELECT 1 FROM Game.UserGameInfo WHERE UserID = @intUserID AND GameID = @sintGameID)

--房间上锁
IF @sintExServerID IS NULL
INSERT INTO Game.GameUserList
        ( UserID, ServerID, HDType )
SELECT
	@intUserID,@sintServerID, @tnyHDType where not exists (select 1 from Game.GameUserList g where 
		g.UserID = @intUserID and g.ServerID = @sintServerID and g.HDType = @tnyHDType)

--获取用户金币数
SELECT @lngUserMoney = Amount,@lngUserTotalMoney = (SELECT SumCharge FROM Users.UserInfo where UserID = @intUserID)
FROM Money.Cash WHERE UserID = @intUserID
SET @lngUserMoney = ISNULL(@lngUserMoney, 0)
SET @lngUserTotalMoney = ISNULL(@lngUserTotalMoney, 0)	

declare @lngBankAmount bigint
select @lngBankAmount = [Amount]
  FROM [CenterDB].[Money].[Bank]
  where userid = @intUserID

declare @lngTradeID bigint
exec [PerGameLog].[dbo].[PrGs_GetTradeID]
	@lngTradeID = @lngTradeID OUTPUT

--获取用户信息
SELECT Ugi.UserID,
u.UserType,
--dbo.Fn_GetUserNickName(@chvIPAddress) as NickName,
'ID:' + cast(Ugi.UserID as varchar(32)) as NickName,
u.FaceID,
u.Sex,
@lngUserMoney AS CashAmount,
@lngCouponAmount AS CouponAmount,
@lngUserTotalMoney AS TotalScore,
ugi.WinCount AS WinCount,
ugi.LoseCount AS LoseCount,
ugi.DrawCount AS DrawCount,
@bOffLine AS IsOffLine,
ISNULL(@chvServerAddr,'') as ServerAddr,
CASE WHEN @sintGameID = 14 THEN '000' ELSE '' END as GameBuff
,ISNULL(u.SumCharge, 0) + ISNULL(u.SumAgentCharge, 0) as TotalCharge,
--20190402 捕魚對接新增
@lngTradeID as TradeID,
ISNULL(@lngBankAmount, 0) as BankAmount
FROM Game.UserGameInfo ugi WITH(NOLOCK)
inner JOIN users.UserInfo u WITH(NOLOCK) ON ugi.UserID = u.UserID
WHERE ugi.UserID = @intUserID and GameID = @sintGameID and u.UserID = @intUserID




RETURN 1

end try
begin catch
	select @ErrorNumber = ERROR_NUMBER(),
	@ErrorMessage = ERROR_MESSAGE(),
	@ErrorSeverity = ERROR_SEVERITY(),
	@ErrorState = ERROR_STATE(),
	@ErrorLine = ERROR_LINE(),
	@ErrorProcedure = ERROR_PROCEDURE();

	--insert into [CenterDB].[dbo].[DBErrorLog]([ErrorNumber],[ErrorMessage],[ErrorSeverity],[ErrorState],[ErrorLine],[ErrorProcedure],[createtime])
	--select @ErrorNumber,@ErrorMessage,@ErrorSeverity,@ErrorState,@ErrorLine,@ErrorProcedure,GETDATE();

	RAISERROR(@ErrorMessage, @ErrorSeverity, @ErrorState) WITH NOWAIT;
end catch