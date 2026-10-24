
-- =============================================
-- Author:  <xxxx>
-- Create date: <xxxx>
-- Description:	新增渠道管理者
-- =============================================
CREATE PROCEDURE [dbo].[PrWeb_AddPlatformAdminUser]
@intAdminID INT,
@intPlatformID INT,
@intAreaID INT = null

AS
BEGIN

	--忽略系统预设平台
	IF @intPlatformID = 999999999
		Return 1

	IF NOT EXISTS(select 1 from [CenterDB].[Area].[PlatformAdminUser] with(nolock) where AdminID=@intAdminID AND PlatformID=@intPlatformID)
	BEGIN
		insert into [CenterDB].[Area].[PlatformAdminUser] (AdminID,PlatformID) values(@intAdminID,@intPlatformID)
	END


	IF Not EXISTS(SELECT 1 from [CenterDB].[Area].[AreaAdminUser] with(nolock) where [AreaID]=@intAreaID and AdminID=@intAdminID and PlatformID=@intPlatformID) AND @intAreaID IS NOT NULL
	BEGIN
		insert into [CenterDB].[Area].[AreaAdminUser](AdminID,PlatformID,AreaID) values(@intAdminID,@intPlatformID,@intAreaID)
	END

	RETURN 1
END



GO
GRANT EXECUTE
    ON OBJECT::[dbo].[PrWeb_AddPlatformAdminUser] TO [GameUser]
    AS [dbo];


GO
GRANT EXECUTE
    ON OBJECT::[dbo].[PrWeb_AddPlatformAdminUser] TO [xbuser]
    AS [dbo];

