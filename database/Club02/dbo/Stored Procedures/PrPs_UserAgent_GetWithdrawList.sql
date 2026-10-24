-- =============================================
-- Author:  jason
-- Create date: 2019/04/11
-- Description: USER 查询领取收益提现页面
-- =============================================
CREATE PROCEDURE [dbo].[PrPs_UserAgent_GetWithdrawList]
(
	@intUserID	int
)
AS
BEGIN

	set nocount on

	/*  计算总收益/已提现收益/剩馀收益  */
	declare @temp_totalTaxAmount bigint
	declare @temp_convertAmount	bigint
	declare @temp_remainAmount bigint


	select	@temp_totalTaxAmount = [SumTaxBonus],
			@temp_convertAmount = [SumTaxConvert]
	from [dbo].[UserAgentInfo] with (nolock)
	where
		[UserID] = @intUserID

	
	/*  今日可提现  */
	declare @temp_performedAmount bigint

	select	@temp_performedAmount = @temp_totalTaxAmount - SUM([WithdrawAmount])
	from [dbo].[UserAgentWithDraw] with (nolock)
	where
		userid = @intUserID

	set @temp_totalTaxAmount	= isnull(@temp_totalTaxAmount,0)
	set @temp_convertAmount	= isnull(@temp_convertAmount,0)
	set @temp_performedAmount	= isnull(@temp_performedAmount,0)
	set @temp_remainAmount = @temp_totalTaxAmount- @temp_convertAmount

	/*  回传总收益/以提现收益/今日可提现  */
	select	TotalTaxAmount = @temp_totalTaxAmount, --总收益
			ConvertAmount = @temp_convertAmount,	--以提现收益
			PerformedAmount = @temp_performedAmount, --今日可提现
			RemainAmount = @temp_remainAmount  --剩馀收益
			

	/*  提现纪录  */
	SELECT	[LogDate] = convert(varchar(10),[LogDate],23),
			[WithdrawAmount],
			[Status] = case	when [Status] = 0 then '审核中'
							when [Status] = 1 then '已退还'
							when [Status] = 2 then '已提取'
						else '未处理' end
	FROM [dbo].[UserAgentWithDraw] WITH (NOLOCK)
	WHERE
		[UserID] = @intUserID
		--and [Status] = 0
	--union all
	--SELECT	[LogDate] = convert(varchar(10),[LogDate],23),
	--		[WithdrawAmount],
	--		[Status] = case	when [Status] = 0 then '审核中'
	--						when [Status] = 1 then '已退还'
	--						when [Status] = 2 then '已提取'
	--					else '未处理' end
	--FROM [dbo].[UserAgentWithDraw] WITH (NOLOCK)
	--WHERE
	--	[UserID] = @intUserID
	--	and [Status] <> 0
	order by convert(varchar(10),[LogDate],23),[Status]

	return 1
END






