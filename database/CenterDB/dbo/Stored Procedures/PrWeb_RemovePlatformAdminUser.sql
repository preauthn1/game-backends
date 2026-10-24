
-- =============================================
-- Author:  <xxxx>
-- Create date: <xxxx>
-- Description:	移除渠道管理者
-- =============================================
CREATE PROCEDURE [dbo].[PrWeb_RemovePlatformAdminUser]
@intAdminID INT
AS
BEGIN

	delete from  [Area].[AreaAdminUser] where AdminID = @intAdminID
	delete from  [Area].[PlatformAdminUser] where AdminID = @intAdminID
	
	RETURN 1
END


GO
GRANT EXECUTE
    ON OBJECT::[dbo].[PrWeb_RemovePlatformAdminUser] TO [GameUser]
    AS [dbo];


GO
GRANT EXECUTE
    ON OBJECT::[dbo].[PrWeb_RemovePlatformAdminUser] TO [xbuser]
    AS [dbo];

