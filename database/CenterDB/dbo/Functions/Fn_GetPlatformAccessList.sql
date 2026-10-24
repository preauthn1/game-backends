-- =============================================
-- Author:  Jason
-- Create date: 2018/06/28
-- Description:	回傳具備權限的平台/渠道列表
-- =============================================
CREATE FUNCTION [dbo].[Fn_GetPlatformAccessList]
(
	@intAdminID	INT
)
RETURNS @temp_platformid TABLE (platformid int) --,areaid int,gameid int)   
AS
BEGIN

	--insert into @temp_platformid
	--select	a.platformid,
	--		b.areaid,
	--		gameid
	--from [Area].[GameList] as a
	--	inner join area.area as b on a.PlatformID = b.PlatformID
	--	inner join [Area].[PlatformAdminUser] as z on b.PlatformID = z.PlatformID and z.AdminID = @intAdminID

	insert into @temp_platformid
	select	platformid from [Area].[PlatformAdminUser] with (nolock) where AdminID=@intAdminID


	RETURN
END

GO
GRANT SELECT
    ON OBJECT::[dbo].[Fn_GetPlatformAccessList] TO [GameUser]
    AS [dbo];


GO
GRANT SELECT
    ON OBJECT::[dbo].[Fn_GetPlatformAccessList] TO [xbuser]
    AS [dbo];

