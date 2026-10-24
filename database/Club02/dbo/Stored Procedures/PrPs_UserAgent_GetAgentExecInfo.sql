-- =============================================
-- Author:  jason
-- Create date: 2019/04/11
-- Description: USER 查询推广等级/今日税收/今日收益/推广人数
-- =============================================
CREATE PROCEDURE [dbo].[PrPs_UserAgent_GetAgentExecInfo]
(
	@intUserID	int
)	
AS
BEGIN

	set nocount on

	declare @logdate datetime = dateadd(day,0,datediff(day,0,getdate()))
	declare @AgentLevel int
	declare @LevelAmount bigint
	declare @UserAmount bigint
	declare @AgentTaxAmount bigint
	declare @AgentTaxBonus bigint
	declare @AgentTotalUser int

	/*  取出三层用户  */
	declare @temp_buser table (userid int)
	declare @temp_cuser table (userid int)
	declare @temp_duser table (userid int)

	insert into @temp_buser
	select userid from [dbo].[UserAgentInfo] with (nolock) where TopUserID = @intUserID

	insert into @temp_cuser
	select userid from [dbo].[UserAgentInfo] with (nolock) where TopUserID in (select userid from @temp_buser)
	
	insert into @temp_duser
	select userid from [dbo].[UserAgentInfo] with (nolock) where TopUserID in (select userid from @temp_cuser)


	/* 用户等级/升级距离  */
	select	@AgentLevel = [UserLevel],
			@UserAmount = [SumTaxBonus]
	from [dbo].[UserAgentInfo] with (nolock)
	where
		UserID = @intUserID

	set @AgentLevel	= ISNULL(@AgentLevel,0)

	select	@LevelAmount = [TaxAmount]
	from [dbo].[UserAgentSystemLevelRate]
	where
		[Level] = (@AgentLevel + 1)

	set @LevelAmount	= ISNULL(@LevelAmount,0)
	set @UserAmount		= isnull(@UserAmount,0)

	/*  今日税收  */
	select	@AgentTaxAmount = SUM([TaxAmount])
	from [dbo].[UserAgentTaxLog] with (nolock)
	where
		Userid in (
						select userid from @temp_buser
						union
						select userid from @temp_cuser
						union
						select userid from @temp_duser
					)
	--	and LogDate = @logdate

	set @AgentTaxAmount	= ISNULL(@AgentTaxAmount,0)

	/*  今日收益  */	
	select	@AgentTaxBonus = SUM([Amount])
	from [dbo].[UserAgentBonus] with (nolock)
	where
		Userid = @intUserID
		--and LogDate = @logdate

	set @AgentTaxBonus	= ISNULL(@AgentTaxBonus,0)

	/*  推广人数  */	
	select @AgentTotalUser = count(userid)
	from (
			select userid from @temp_buser
			union
			select userid from @temp_cuser
			union
			select userid from @temp_duser
		) as a

	set @AgentTotalUser	= ISNULL(@AgentTotalUser,0)

	/*  回传资料  */
	select	AgentAmount = @LevelAmount,
			AgentUserAmount = @UserAmount,
			AgentLevel = @AgentLevel,
			AgentTaxAmount = @AgentTaxAmount,
			AgentTaxBonus = @AgentTaxBonus,
			AgentTotalUser = @AgentTotalUser

	return 1
END






