-- =============================================
-- Author:  Rory
-- Create date: 2019/07/12
-- Description:	^Ǩvx/DC
-- =============================================
CREATE FUNCTION [dbo].[Fn_GetAdminAccessRights]
(
	@intAdminID	INT
)
RETURNS @temp_platformid TABLE (PlatformID int, AreaID int) 
AS
BEGIN

	insert into @temp_platformid
	select	PlatformID, AreaID from [Area].[AreaAdminUser] with (nolock) where AdminID=@intAdminID


	RETURN
END
GO


