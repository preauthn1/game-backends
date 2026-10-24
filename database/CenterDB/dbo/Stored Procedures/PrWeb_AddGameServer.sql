-- =============================================
-- Author:  <xxx>
-- Create date: <xxx>
-- Description:	新增游戏server
-- =============================================
CREATE PROCEDURE [dbo].[PrWeb_AddGameServer]
@intServerID INT,
@intGameID INT,
@chvServerName varchar(64),
@chvShowName  varchar(64),
@chvClientAddr  varchar(32)='',
@chvNotifyAddr  varchar(32)='',
@chvHttpAddr  varchar(32)='',
@intMoney bigint,
@chvErrMsg varchar(32) output,
@intRtnCode int output

AS
BEGIN

	SET NOCOUNT ON


	if exists(select 1 from ServerInfo.GameServerList with (nolock) where ServerID=@intServerID or ServerName=@chvServerName)
	begin

		set @chvErrMsg='操作失败：房间重复'
		set @intRtnCode = 0
		RETURN 
	end

	insert into ServerInfo.GameServerList(ServerID, GameID, ServerName, ShowName, ClientAddr, NotifyAddr, HttpAddr, Money,LastUpdateTime)
	values(@intServerID,@intGameID,@chvServerName,@chvShowName,@chvClientAddr,@chvNotifyAddr,@chvHttpAddr, @intMoney, getdate())

	set @chvErrMsg='操作成功：添加房间成功'
	set @intRtnCode = 1
	RETURN

END
GO
GRANT EXECUTE
    ON OBJECT::[dbo].[PrWeb_AddGameServer] TO [GameUser]
    AS [dbo];

