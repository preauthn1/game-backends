-- =============================================
-- Author:  alan
-- Create date: 20181005
-- Description: 取得渠道包是否有不正確資訊
-- Changelog:
-- =============================================
CREATE PROCEDURE [dbo].[PrWeb_GetPlatformDownloadIncorrectCount]
@intAdminId int=null,
@intCount int output
AS
BEGIN

	SET NOCOUNT ON

	--渠道篩選
	DECLARE @Temp_PlatformID TABLE (PlatformID	INT)
	INSERT INTO @Temp_PlatformID Select PlatformID From CenterDB.Area.PlatformAdminUser where AdminID=@intAdminID

	
	SELECT @intCount= count(1) from [Area].[PlatformDownload] D with (nolock)	
	WHERE Correct = 0
	AND PlatformID in (select PlatformID from @Temp_PlatformID)

	SET @intCount=ISNULL(@intCount,0)

END
GO
GRANT EXECUTE
    ON OBJECT::[dbo].[PrWeb_GetPlatformDownloadIncorrectCount] TO [GameUser]
    AS [dbo];


GO
GRANT EXECUTE
    ON OBJECT::[dbo].[PrWeb_GetPlatformDownloadIncorrectCount] TO [xbuser]
    AS [dbo];

