-- =============================================
-- Author:  <xxx>
-- Create date: <xxx>
-- Description:	<获取平台FAQ信息>
-- =============================================
CREATE PROCEDURE [dbo].[PrPs_GetFAQ]	
@intPlatformID		int,				--平台ID
@intAreaID			int=null,
@chvContent			varchar(max) OUTPUT	--FAQ内容
AS
BEGIN

IF 	@intAreaID is null
	SELECT top 1 @chvContent = FAQContent from Area.Area where PlatformID = @intPlatformID

ELSE
	SELECT @chvContent = FAQContent from Area.Area where PlatformID=@intPlatformID and AreaID=@intAreaID

SET @chvContent = ISNULL(@chvContent,'')

return 1
END




GO
GRANT EXECUTE
    ON OBJECT::[dbo].[PrPs_GetFAQ] TO [GameUser]
    AS [dbo];


GO
GRANT EXECUTE
    ON OBJECT::[dbo].[PrPs_GetFAQ] TO [xbuser]
    AS [dbo];

