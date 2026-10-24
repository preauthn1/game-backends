-- =============================================
-- Author:  jason
-- Create date: 2019/04/15
-- Description: USER 代理提现纪录(净利润)
-- =============================================
CREATE PROCEDURE [dbo].[PrPs_NetProfitAgent_GetAgentWithdrawAmount]
(
	@intUserID		int		--用户 ID
)
AS
BEGIN

	set nocount on

	
	/*  取出用户识别  */
	declare @temp_relations varchar(max)

	select	@temp_relations = [Relations]
	from [dbo].[NetProfitAgent_UserInfo] with (nolock)
	where
		[UserID] = @intUserID


	/*  取出下级代理  */
	declare @temp_agentwithdrawslog	table (userid int,CreateTime datetime,[WithDrawAmount] bigint,[Status] tinyint)

	insert into @temp_agentwithdrawslog
	select	a.userid,
			b.CreateTime,
			[WithDrawAmount] = [WithDrawAmount],
			b.[Status]
	from [dbo].[NetProfitAgent_UserInfo] as a with (nolock)
		inner join [dbo].[NetProfitAgent_WithDrawLog] as b with (nolock) on a.UserID = b.UserID
	where
		Relations like '' + @temp_relations + '%'
		and a.status = 2

	declare @totalamount varchar(20)


	select	@totalamount = cast(sum([WithDrawAmount]) as varchar(20))
	from @temp_agentwithdrawslog

	set @totalamount = isnull(@totalamount,0)
	

	if not exists (select top 1 1 from @temp_agentwithdrawslog)
	begin
		select top 0 * from @temp_agentwithdrawslog
		return 1
	end
	
	select	[UserID] = cast([UserID] as varchar(10)),
			[CreateTime] = convert(varchar(19),[CreateTime],121),
			[WithDrawAmount] = cast([WithDrawAmount] as varchar(20)),
			[Status] = case when [Status] = 0 then '未完成'
							when [Status] = 1 then '已提领'
							when [Status] = 2 then '提领失败退还'
						end
	from @temp_agentwithdrawslog
	union all
	select	'总计','',@totalamount,''
	
	return 1
END





