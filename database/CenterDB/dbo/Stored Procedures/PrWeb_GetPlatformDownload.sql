-- =============================================
-- Author:  alan
-- Create date: 20181005
-- Description: 取得渠道包下載资讯
-- Changelog:
-- =============================================
CREATE PROCEDURE [dbo].[PrWeb_GetPlatformDownload]
@intAdminID			INT=null,
@intPlatformID 		INT=null
AS
BEGIN

	SET NOCOUNT ON
	
	--渠道篩選
	DECLARE @Temp_PlatformID TABLE (PlatformID	INT)
	INSERT INTO @Temp_PlatformID Select PlatformID From CenterDB.Area.PlatformAdminUser where AdminID=@intAdminID


	SELECT P.Name,D.* from [Area].[PlatformDownload] D with (nolock)
	LEFT JOIN 
	Area.Platform P with (nolock) on D.PlatformID=P.PlatformID
	WHERE (@intPlatformID IS NULL OR D.PlatformID=@intPlatformID)
	AND D.PlatformID in (select PlatformID from @Temp_PlatformID)
END
GO
GRANT EXECUTE
    ON OBJECT::[dbo].[PrWeb_GetPlatformDownload] TO [GameUser]
    AS [dbo];


GO
GRANT EXECUTE
    ON OBJECT::[dbo].[PrWeb_GetPlatformDownload] TO [xbuser]
    AS [dbo];

