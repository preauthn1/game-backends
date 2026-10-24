-- =============================================
-- Author:  <xxx>
-- Create date: <xxx>
-- Description:	<获取用户登录信息>
-- =============================================
CREATE PROCEDURE [dbo].[PrPs_UserLogon_GetUserInfo]
@intUserID INT,							--用户ID
@tnyHDType TINYINT,						--客户端类型
@chvCertification VARCHAR(32) output,	--证书
@sintServerID SMALLINT,					--广场服务器ID
@chvIPAddress VARCHAR(15),              --Ip地址
@lngCashAmount BIGINT OUTPUT,			--现金
@lngBankAmount BIGINT OUTPUT,			--银行现金
@intNoTalk int OUTPUT,					--是否禁止聊天
@bIsLock BIT OUTPUT,					--是否在游戏房间
@chvServerAddr VARCHAR(128) OUTPUT,	--服务器地址
@intGameID INT OUTPUT,					--游戏ID
@bKick BIT OUTPUT,						--是否踢人
@chvErrMsg VARCHAR(128) OUTPUT			--提示消息
AS
IF EXISTS(SELECT 1 FROM Game.ForbidUser  with (nolock) WHERE UserID = @intUserID)
BEGIN
	SET @chvErrMsg = '您的帐号' + CONVERT(varchar,@intUserID) + '被封,请和管理员联系'
	RETURN -1
END

--检查封锁IP
IF EXISTS(SELECT 1 FROM Game.ForbidIPAddress with (nolock) WHERE IPAddress = @chvIPAddress)
BEGIN
	SET @chvErrMsg = '登录失败：此IP被封,请和管理员联系.'
	RETURN -1	
END

--更新证书和大厅在线
DECLARE @intKickRet int
EXEC @intKickRet = PrPs_UpdateUserCerAndHall
	 @intUserID,
	 @tnyHDType,
     @chvCertification output,
     @sintServerID

SET @bKick = CASE WHEN @intKickRet = 1 THEN 1 ELSE 0 end


SELECT @lngCashAmount = ISNULL(cash.Amount, 0),             --现金
@lngBankAmount = ISNULL(bank.Amount,0),
@intNoTalk = 0  --禁止聊天
FROM Money.Cash cash with (nolock) LEFT JOIN Money.Bank bank  with (nolock) ON cash.UserID = bank.UserID
WHERE cash.UserID = @intUserID


--获取用户服务器信息
SELECT @chvServerAddr = gs.ClientAddr,
@intGameID = gs.GameID
FROM Game.GameUserList gu with (nolock), ServerInfo.GameServerList gs with (nolock)
WHERE gu.UserID = @intUserID
AND gu.ServerID = gs.ServerID	


SET @lngCashAmount = ISNULL(@lngCashAmount, 0)

SET @bIsLock= CASE WHEN @chvServerAddr IS NULL THEN 0 ELSE 1 END
SET @chvServerAddr = ISNULL(@chvServerAddr, '')


RETURN 1

GO
GRANT EXECUTE
    ON OBJECT::[dbo].[PrPs_UserLogon_GetUserInfo] TO [GameUser]
    AS [dbo];


GO
GRANT EXECUTE
    ON OBJECT::[dbo].[PrPs_UserLogon_GetUserInfo] TO [xbuser]
    AS [dbo];

