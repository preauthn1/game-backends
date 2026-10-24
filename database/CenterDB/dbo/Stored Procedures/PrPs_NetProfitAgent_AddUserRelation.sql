-- =============================================
-- Author:  jason
-- Create date: 2019/04/15
-- Description: USER 加入下线(净利润)
-- =============================================
CREATE PROCEDURE [dbo].[PrPs_NetProfitAgent_AddUserRelation]
(
	@intUserID		int,				--用户 ID
	@chvInviteCode	VARCHAR(20),		--邀请码
	@chvMsg			varchar(250)	OUTPUT
)
AS
BEGIN

	set nocount on

	EXEC Club02.[dbo].[PrWeb_NetProfitAgent_AddUserRelation]
		@intUserID = @intUserID,				--用户 ID
		@chvInviteCode = @chvInviteCode,		--邀请码
		@chvMsg = @chvMsg	OUTPUT

		return 1
END







