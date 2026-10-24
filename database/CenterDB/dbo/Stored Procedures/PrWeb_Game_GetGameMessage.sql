-- =============================================
-- Author:  alan
-- Create date: 2019/04/02
-- Description:	取得公告清单
-- =============================================
CREATE PROCEDURE [dbo].[PrWeb_Game_GetGameMessage]
@intAdminID		int,
@intPlatformID	int = null
AS
BEGIN

	--渠道篩選
	DECLARE @Temp_PlatformID TABLE (PlatformID	INT)
	INSERT INTO @Temp_PlatformID Select PlatformID From CenterDB.Area.PlatformAdminUser where AdminID=@intAdminID

	Declare @intOpUserType int
	select @intOpUserType=UserType from AdminSys.Admin.Users with (nolock) where UserID=@intAdminID

	select 
	CASE WHEN G.AllPlatform=1 THEN '全平台' ELSE P.Name END as Name, 
	
	* from Game.GameMessage G with (nolock)
	left join area.Platform P on G.PlatformID=P.PlatformID
	 where 
	G.PlatformID in (select PlatformID from @Temp_PlatformID) 
	AND (@intPlatformID is null or G.PlatformID=@intPlatformID)


END


GO
GRANT EXECUTE
    ON OBJECT::[dbo].[PrWeb_Game_GetGameMessage] TO [GameUser]
    AS [dbo];

