-- =============================================
-- Author:  <xxx>
-- Create date: <xxx>
-- Description:	<获取游戏房间名称>
-- =============================================
CREATE FUNCTION [dbo].[Fn_GetRoomName]
(
	@intServerID	int
)
RETURNS varchar(32)
AS
BEGIN
	
	declare @chvRoomName varchar(32)
	select  @chvRoomName = g.GameName + '(' + dbo.Fn_GetRoomSceneName(gs.ShowName, gs.GameID)+')'
	from ServerInfo.GameServerList as gs left join ServerInfo.GameList as g on gs.GameID=g.GameID
	where gs.serverid = @intServerID
	return ISNULL(@chvRoomName,'未知房间')
END


GO
GRANT EXECUTE
    ON OBJECT::[dbo].[Fn_GetRoomName] TO [GameUser]
    AS [dbo];


GO
GRANT EXECUTE
    ON OBJECT::[dbo].[Fn_GetRoomName] TO [xbuser]
    AS [dbo];

