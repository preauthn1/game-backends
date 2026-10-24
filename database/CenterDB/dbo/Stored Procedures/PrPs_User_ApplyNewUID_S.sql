-- =============================================
-- Author:  Jason
-- Create date: 2018/04/27
-- Description:	直接取號
-- =============================================
CREATE PROCEDURE [dbo].[PrPs_User_ApplyNewUID_S]
(
	@intUserID		INT OUTPUT					--用户ID
)
AS
BEGIN

	SET NOCOUNT ON

	DECLARE	@Temp_UserID	TABLE (UserID INT)

	DELETE TOP (1) FROM [Users].[RanID] OUTPUT DELETED.UserID INTO @Temp_UserID WHERE Status = 0

	SET @intUserID = ISNULL((SELECT UserID FROM @Temp_UserID),0)

END

GO
GRANT EXECUTE
    ON OBJECT::[dbo].[PrPs_User_ApplyNewUID_S] TO [GameUser]
    AS [dbo];


GO
GRANT EXECUTE
    ON OBJECT::[dbo].[PrPs_User_ApplyNewUID_S] TO [xbuser]
    AS [dbo];

