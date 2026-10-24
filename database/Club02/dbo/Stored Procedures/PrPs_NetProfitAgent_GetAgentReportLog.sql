-- =============================================
-- Author:  jason
-- Create date: 2019/04/15
-- Description: USER 新增代理(净利润)
-- =============================================
CREATE PROCEDURE [dbo].[PrPs_NetProfitAgent_GetAgentReportLog]
(
	@intUserID		int,		--用户 ID
	@dtStatTime		datetime = null
)
AS
BEGIN

	set nocount on

	declare @StartDate	datetime = isnull(@dtStatTime,'1970-01-01')
	declare @EndDate	datetime

	set @StartDate = dateadd(day,0,datediff(day,0,@dtStatTime))
	set @EndDate = dateadd(day,1,datediff(day,0,@dtStatTime))


	/*  取出用户识别  */
	declare @temp_relations varchar(max)

	select	@temp_relations = [Relations]
	from [dbo].[NetProfitAgent_UserInfo] with (nolock)
	where
		[UserID] = @intUserID

	
	/*  取出下级代理  */
	declare @temp_agentuser	table (userid int,CreateTime datetime,[SumSubUsers] int,[SumChargeAmount] bigint,[WithdrawAmount] bigint,Counts int)

	insert into @temp_agentuser
	select	userid,
			CreateTime,
			[SumSubUser],
			[SumChargeAmount],
			[WithdrawAmount] = [SumWithDrawAmount] + [SumSubWithDrawAmount],
			Counts = [CountWithdraw] + [CountSubwithdraw]
	from [dbo].[NetProfitAgent_UserInfo] with (nolock)
	where
		Relations like '' + @temp_relations + '%'
		and topuserid = @intUserID
		and status = 2


	/*  昨日新增代理/代理总数  */
	declare @totaluserlast int
	declare @agenttotal int

	select	@totaluserlast = sum(case when [AgentCreateTime] >= @StartDate and [AgentCreateTime] < @EndDate then 1 else 0 end),
			@agenttotal = sum(case when userid <> @intUserID then 1 else 0 end)
	from [dbo].[NetProfitAgent_UserInfo] with (nolock)
	where
		status = 2
		and relations like '' + @temp_relations + '%'
		and [AgentCreateTime] >= @StartDate
		and [AgentCreateTime] < @EndDate

	set @totaluserlast = isnull(@totaluserlast,0)
	set @agenttotal = isnull(@agenttotal,0)


	/*  回传资料  */
	select	[totallastdate] = @totaluserlast,
			[totalagentuser] = @agenttotal

	select	a.userid,
			CreateTime,
			[SumSubUsers],
			[SumChargeAmount] = isnull([SumChargeAmount],0),
			[WithdrawAmount] = isnull([WithdrawAmount],0),
			Counts = isnull(Counts,0)
	from @temp_agentuser as a


	return 1
END





