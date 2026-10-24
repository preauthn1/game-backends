-- =============================================
-- Description:	建立新平台
-- =============================================
CREATE procedure [dbo].[PrSys_CreateNewPlatform]
@intPlatformid varchar(10), --指定平台號
@chvPlatformName varchar(32), --平台名稱
@intAreaNum int=10, --包数量
@intUnionPlatform tinyint --是否是聯運平台 1=Yes
As
BEGIN


DECLARE	@return_value int,
		@chvErrMsg varchar(50)

EXEC	@return_value = [dbo].[PrWeb_CreateNewPlatform]
		@intPlatformID = @intPlatformid,
		@chvPlatformName = @chvPlatformName,
		@intUnionPlatform = @intUnionPlatform,
		@intAreaNum = @intAreaNum,
		@chvErrMsg = @chvErrMsg OUTPUT

SELECT @chvErrMsg

RETURN 1

END



GO
GRANT EXECUTE
    ON OBJECT::[dbo].[PrSys_CreateNewPlatform] TO [GameUser]
    AS [dbo];


GO
GRANT EXECUTE
    ON OBJECT::[dbo].[PrSys_CreateNewPlatform] TO [xbuser]
    AS [dbo];

