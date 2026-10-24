-- =============================================
-- Author:  jason
-- Create date: 2019/05/06
-- Description: USER 加入下线(裂变)
-- =============================================
CREATE PROCEDURE [dbo].[PrPs_Split_AddUserRelation]
(
	@intUserID		int,				--用户 ID
	@chvInviteCode	VARCHAR(20) = null,		--邀请码
	@chvMsg			varchar(250)	OUTPUT
)
AS
BEGIN

	set nocount on


	EXEC Club02.[dbo].[PrWeb_Split_AddUserRelation]
		@intUserID = @intUserID,				--用户 ID
		@chvInviteCode = @chvInviteCode,		--邀请码
		@chvMsg = @chvMsg	OUTPUT

	return 1
END










GO
GRANT EXECUTE
    ON OBJECT::[dbo].[PrPs_Split_AddUserRelation] TO [GameUser]
    AS [dbo];

