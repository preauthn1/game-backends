-- =============================================
-- Author:  alan
-- Create date: 20180906
-- Description:	用户登出體驗場房间
-- =============================================
CREATE PROCEDURE [dbo].[PrGs_ExperienceUserLogout]
(
	@intUserID			INT,					--用户ID
	@lngUpdateAmount	BIGINT					--变化的金币数量 (丢进来的值有负有正)
)
AS
BEGIN

	SET NOCOUNT ON
	DECLARE @intCurrentAmount bigint
	SELECT @intCurrentAmount = ExperienceCash  From Users.UserInfo with (nolock) where USERID = @intUserID

	IF @intCurrentAmount + @lngUpdateAmount < 0
		SET @lngUpdateAmount = -@intCurrentAmount

	UPDATE Users.UserInfo SET ExperienceCash = ExperienceCash + @lngUpdateAmount where USERID = @intUserID

	RETURN 1
END
GO
GRANT EXECUTE
    ON OBJECT::[dbo].[PrGs_ExperienceUserLogout] TO [GameUser]
    AS [dbo];


GO
GRANT EXECUTE
    ON OBJECT::[dbo].[PrGs_ExperienceUserLogout] TO [xbuser]
    AS [dbo];

