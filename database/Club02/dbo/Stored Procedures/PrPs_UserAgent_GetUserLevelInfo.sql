-- =============================================
-- Author:  jason
-- Create date: 2019/04/11
-- Description: USER 推广等级明细页(我的等级)
-- =============================================
CREATE PROCEDURE [dbo].[PrPs_UserAgent_GetUserLevelInfo]
(
	@intUserID	int
)
AS
BEGIN

	set nocount on

	declare @UserLevel int
	declare @NextLevel int
	declare @UserAmount	bigint
	declare @NextLevelAmount bigint
	declare @NeedAmount bigint

	
	/*  取得用户当前等级  */
	select	@UserLevel = UserLevel,
			@UserAmount = [SumTaxBonus]
	from [dbo].[UserAgentInfo] with (nolock)
	where
		userid = @intUserID

	select	@NextLevelAmount = [TaxAmount]
	from [dbo].[UserAgentSystemLevelRate] with (nolock)
	where
		[Level] = (@UserLevel + 1)

	/*  取得用户税收比例(上级所设定)  */
	select	[Stage1],
			[Stage2],
			[Stage3]
	from [dbo].[UserAgentUserRate] with (nolock)
	where
		userid = @intUserID
		and [UserLevel] = @UserLevel

	return 1
END






