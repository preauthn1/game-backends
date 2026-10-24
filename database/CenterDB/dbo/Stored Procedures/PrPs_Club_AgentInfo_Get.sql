


-- =============================================
-- Author:  alan
-- Create date: 20190329
-- Description:	CLUB 取得会员相關資訊
-- =============================================
CREATE PROCEDURE [dbo].[PrPs_Club_AgentInfo_Get]
	@intUserID			INT,	                    --用户ID	
	@chvErrMsg			VARCHAR(128) OUTPUT			--返回消息	
AS
BEGIN
	SET NOCOUNT ON;

	declare @intPlatformID int=0
	select @intPlatformID=PlatformID from Area.area with (nolock) where AreaID = 
	( select AreaID from Users.UserInfo with (nolock) where UserID = @intUserID )

	EXEC	CLUB.[dbo].[PrPs_AgentInfo_Get]
			@intUserID = @intUserID,
			@chvErrMsg = @chvErrMsg OUTPUT


	--if @intPlatformID in (select PlatformID from Area.UnionPlatform ) --聯運包
	--begin


	--	EXEC	CLUB02.[dbo].[PrPs_AgentInfo_Get]
	--			@intUserID = @intUserID,
	--			@chvErrMsg = @chvErrMsg OUTPUT
	--end
	--else
	--begin
	--	EXEC	CLUB.[dbo].[PrPs_AgentInfo_Get]
	--			@intUserID = @intUserID,
	--			@chvErrMsg = @chvErrMsg OUTPUT
	--end

	RETURN 1
END

GO
GRANT EXECUTE
    ON OBJECT::[dbo].[PrPs_Club_AgentInfo_Get] TO [GameUser]
    AS [dbo];

