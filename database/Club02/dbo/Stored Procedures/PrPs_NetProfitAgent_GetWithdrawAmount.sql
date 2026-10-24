-- =============================================
-- Author:  jason
-- Create date: 2019/04/15
-- Description: USER 提現紀錄(淨利潤)
-- =============================================
CREATE PROCEDURE [dbo].[PrPs_NetProfitAgent_GetWithdrawAmount]
(
	@intUserID		int		--用户 ID
)
AS
BEGIN

	set nocount on
		
	declare @temp_agentwithdrawslog	table (num int,createtime datetime,[WithDrawAmount] bigint,[Status] tinyint,Memo varchar(100))
	insert into @temp_agentwithdrawslog
	select	row_number() over (order by createtime desc) as num,
			[CreateTime],
			[WithDrawAmount],
			[Status],
			[Memo]
	from [dbo].[NetProfitAgent_WithDrawLog] with (nolock)
	where
		userid = @intUserID

	
	declare @totalamount varchar(20)
	select	@totalamount = cast(sum([WithDrawAmount]) as varchar(20))
	from @temp_agentwithdrawslog

	if not exists (select top 1 1 from @temp_agentwithdrawslog)
	begin
		select top 0 createtime,[WithDrawAmount],[Status],Memo from @temp_agentwithdrawslog
		return 1
	end

	select	[CreateTime],
			[WithDrawAmount],
			[Status],
			[Memo]
	from (
			select	num,
					[CreateTime] = convert(varchar(19),[CreateTime],121),
					[WithDrawAmount] = cast([WithDrawAmount] as varchar(20)),
					[Status] = case when [Status] = 0 then '未完成'
									when [Status] = 1 then '已提领'
									when [Status] = 2 then '提领失败退还'
								end,
					[Memo]
			from @temp_agentwithdrawslog
			union all
			select	9999,'总金额',@totalamount,'',''
		) as a
	
	return 1
END





