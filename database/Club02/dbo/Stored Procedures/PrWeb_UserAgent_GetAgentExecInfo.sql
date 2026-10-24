-- =============================================
-- Author:  jason
-- Create date: 2019/04/13
-- Description: 全民代理后台 查询推广等级/今日税收/今日收益/推广人数
-- =============================================
CREATE PROCEDURE [dbo].[PrWeb_UserAgent_GetAgentExecInfo]
(
	@intAdminID int = null,
	@intUserID	int = null,
	@chvMsg		varchar(100)	OUTPUT
)	
AS
BEGIN

	set nocount on

	declare @startdate datetime = dateadd(day,0,datediff(day,0,getdate()))
	declare @AgentAgentUser bigint
	declare @withdrawAmount bigint


	--捞出该用户可读取的全部平台
	declare @Temp_PlatformID table (PlatformID int)
	INSERT INTO @Temp_PlatformID Select PlatformId From [dbo].[AgentPlatformRelations] where AdminID=@intAdminID

	declare @Temp_userplatformid int
	select	@Temp_userplatformid = [PlatformID] from [dbo].[UserAgentInfo] with (nolock) where userid = @intUserID

	if (@intAdminID is not null)
	begin
		declare @AgentTaxAmount bigint
		declare @AgentTaxBonus bigint
		declare @AgentTotalUser bigint
		

		select	@AgentTaxAmount = sum([TaxAmount])
		from [dbo].[UserAgentTaxLog] with (nolock)

		select	@AgentTaxBonus = sum([Amount])
		from [dbo].[UserAgentBonus] with (nolock)

		select	@AgentTotalUser = count(1)
		from [dbo].[UserAgentInfo] with (nolock)

		select	@AgentAgentUser = count(1)
		from [dbo].[UserAgentInfo] with (nolock)
		where
			[CreateTime] >= @startdate

		select	@withdrawAmount = sum([WithdrawAmount])
		from [dbo].[UserAgentWithDraw] with (nolock)
		where status = 1
	
		set @AgentTaxAmount = isnull(@AgentTaxAmount,0)
		set @AgentTaxBonus = isnull(@AgentTaxBonus,0)
		set @AgentTotalUser = isnull(@AgentTotalUser,0)
		set @AgentAgentUser = isnull(@AgentAgentUser,0)
		set @withdrawAmount = isnull(@withdrawAmount,0)

		
		/*  回传资料  */
		select	AgentAmount = cast(0 as bigint),
				AgentUserAmount = cast(0 as bigint),
				AgentLevel = cast(0 as int),
				AgentTaxAmount = @AgentTaxAmount,
				AgentTaxBonus = @AgentTaxBonus,
				AgentTotalUser = @AgentTotalUser,
				AgentAgentUser = @AgentAgentUser,
				withdrawAmount = @withdrawAmount
	end	
	else if((@intAdminID is null) or exists (select top 1 1 from @Temp_PlatformID where platformid = @Temp_userplatformid)) and (@intUserID is not null)
	begin

		declare @temp_tablea table (AgentAmount bigint,AgentUserAmount bigint,AgentLevel int,AgentTaxAmount bigint,AgentTaxBonus bigint,AgentTotalUser bigint)

		insert into @temp_tablea
		EXEC [dbo].[PrPs_UserAgent_GetAgentExecInfo]
			@intUserID = @intUserID

		declare @temp_buser table (userid int)
		declare @temp_cuser table (userid int)
		declare @temp_duser table (userid int) 

		insert into @temp_buser
		select userid from [dbo].[UserAgentInfo] with (nolock) where topuserid = @intUserID

		insert into @temp_cuser
		select userid from [dbo].[UserAgentInfo] with (nolock) where topuserid in (select userid from @temp_buser)

		insert into @temp_duser
		select userid from [dbo].[UserAgentInfo] with (nolock) where topuserid in (select userid from @temp_buser union select userid from @temp_cuser)

		select	@AgentAgentUser = count(1)
		from (
				select userid from @temp_buser
				union
				select userid from @temp_cuser
				union
				select userid from @temp_duser
			) as a
			inner join [dbo].[UserAgentInfo] as b with (nolock) on a.userid = b.userid
		where
			[CreateTime] >= @startdate

		select	@withdrawAmount = sum([WithdrawAmount])
		from (
				select userid from @temp_buser
				union
				select userid from @temp_cuser
				union
				select userid from @temp_duser
			) as a
			inner join [dbo].[UserAgentInfo] as b with (nolock) on a.userid = b.userid
			inner join [dbo].[UserAgentWithDraw] as c with (nolock) on a.userid = c.userid
		where c.status = 1

		set @AgentAgentUser = isnull(@AgentAgentUser,0)
		set @withdrawAmount = isnull(@withdrawAmount,0)


		select	*,@AgentAgentUser as AgentAgentUser,@withdrawAmount as withdrawAmount
		from @temp_tablea

		set @chvMsg	= '操作成功'
		return 1
	end
	else
	begin
		set @chvMsg	= '操作失败 : 管理者无权限'
		return -1
	end

	return 1
END






