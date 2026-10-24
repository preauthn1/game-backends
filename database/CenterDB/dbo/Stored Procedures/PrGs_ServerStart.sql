-- =============================================
-- Author:  <xxx>
-- Create date: <xxx>
-- Description:	<游戏服务器启动>
--	2018/03/26	Jason		新增@chvHttpAddr參數
-- =============================================
CREATE PROCEDURE [dbo].[PrGs_ServerStart]
@sintGameID SMALLINT,                      	--游戏ID
@chvServerName VARCHAR(64),                	--服务器名
@chvShowName VARCHAR(64),					   	--显示名称
@chvClientAddr VARCHAR(32),					--客户端地址
@chvNotifyAddr VARCHAR(32),					--通知地址
@chvHttpAddr VARCHAR(32),					--Http地址
@lngLoginMoney BIGINT,						--进入房间金币数
@sintServerID SMALLINT ,				--服务器ID,
@chvErrMsg VARCHAR(128) OUTPUT
AS
SET NOCOUNT ON

--更新服务器信息
UPDATE ServerInfo.GameServerList
SET ClientAddr = @chvClientAddr,
NotifyAddr = @chvNotifyAddr,
HttpAddr =@chvHttpAddr,
Money =@lngLoginMoney,
ServerName = @chvServerName,
ShowName = @chvShowName,
LastUpdateTime = GETDATE()
WHERE ServerID = @sintServerID AND GameID = @sintGameID

IF @@ROWCOUNT = 0
begin
	SET @chvErrMsg = '服务器启动失败:不存在服务器配置信息'
	RETURN 0
END

--更新金币源
IF NOT EXISTS(SELECT 1 FROM stat.MoneySourceType WHERE SourceID = @sintServerID)
INSERT INTO Stat.MoneySourceType
        ( SourceID, SourceName, ChangeType , GroupID)
SELECT @sintServerID,@chvServerName, 1, 2

--解锁玩家房间锁
DELETE game.GameUserList where ServerID = @sintServerID

--获取游戏版本号
SELECT AndroidVersion,IOSVersion,Status,'系统维护中，请稍后再试!' as StatusMessage
FROM ServerInfo.GameList
WHERE GameID = @sintGameID

--机器人解锁
UPDATE Robot.RobotList
SET ServerID = 0
WHERE ServerID = @sintServerID

--清除房间网络消息记录
DELETE FROM Log.NetworkMsgLog where ServerID = @sintServerID

SET @chvErrMsg = '服务器启动成功'

RETURN 1

GO
GRANT EXECUTE
    ON OBJECT::[dbo].[PrGs_ServerStart] TO [GameUser]
    AS [dbo];


GO
GRANT EXECUTE
    ON OBJECT::[dbo].[PrGs_ServerStart] TO [xbuser]
    AS [dbo];

