-- =============================================
-- Author:  jason
-- Create date: 2019/04/11
-- Description: USER 加入下线
-- =============================================
CREATE PROCEDURE [dbo].[PrWeb_UserAgent_AddUserRelation]
(
@intUserID		int,				--用户 ID
@chvInviteCode	VARCHAR(20),		--邀请码
@chvMsg			varchar(250)	OUTPUT
)
AS
BEGIN

	set nocount on

	EXEC Club02.[dbo].[PrWeb_UserAgent_AddUserRelation]
		@intUserID = @intUserID,
		@chvInviteCode = @chvInviteCode,
		@chvMsg = @chvMsg OUTPUT

	return 1
END







