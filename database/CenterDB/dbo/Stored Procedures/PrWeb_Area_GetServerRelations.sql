-- =============================================
-- Author:  <xxx>
-- Create date: <xxx>
-- Description:	獲取某渠道是否可以看到某區廳清單
-- =============================================
CREATE PROCEDURE [dbo].[PrWeb_Area_GetServerRelations]
@intAreaID INT=null,
@intGameID INT=null
AS
BEGIN

	SET NOCOUNT ON


	if (@intAreaID is null or @intGameID is null)
	begin
		set @intAreaID=0
		set @intGameID=0
	end

	select A.AreaName, @intAreaID as AreaID, G.ServerID, G.ServerName, G.GameID, G.ShowName as FlagID, ISNULL(R.Status,0) as Status
	from ServerInfo.GameServerList G
	left join [ServerInfo].[ServerRelations] R on G.ServerID=R.ServerID and R.AreaID=@intAreaID and R.GameID=@intGameID
	left join Area.Area A on A.areaID = @intAreaID
	where G.gameid = @intGameID 



END
GO
GRANT EXECUTE
    ON OBJECT::[dbo].[PrWeb_Area_GetServerRelations] TO [GameUser]
    AS [dbo];

