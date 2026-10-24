-- =============================================
-- Author:  jason
-- Create date: 2019/04/30
-- Description: USER 加入下线(无限级)
-- =============================================
CREATE PROCEDURE [dbo].[PrPs_Unlimit_AddUserRelation]
(
	@intUserID		int,				--用户 ID
	@chvInviteCode	VARCHAR(20) = null,		--邀请码
	@chvMsg			varchar(250)	OUTPUT
)
AS
BEGIN

	set nocount on

	EXEC Club02.[dbo].[PrWeb_Unlimit_AddUserRelation]
		@intUserID = @intUserID,
		@chvInviteCode = @chvInviteCode,
		@chvMsg = @chvMsg OUTPUT

		--return 1
END









