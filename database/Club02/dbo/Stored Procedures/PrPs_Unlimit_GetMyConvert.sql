
-- =============================================
-- Author:  jason
-- Create date: 20190430
-- Description: 无极限-我的提现
-- =============================================
CREATE PROCEDURE [dbo].[PrPs_Unlimit_GetMyConvert]
(
	@intUserID int,
	@intPageIndex	INT	= 1,
	@intPageSize	INT = 20,
	@intPageCount	INT = 0		OUTPUT,
	@intSumReward bigint OUTPUT, --总奖励金额 : 从过去到现在已经发放的奖励金累计
	@intSumConvert bigint OUTPUT, --已提现金额 : 从过去到现在已经提现的金额累计
	@intRemainConvert bigint OUTPUT --可提现金额 : 总奖励金额 – 可提现金额
)
AS
BEGIN

	SET NOCOUNT ON

	----1.	提现纪录栏:
	--declare @intOrderId int = 111111 --单号 : 申请的流水号
	--declare @intAmount bigint = 20000 --金额 : 申请提现的金额
	--declare @intType tinyint = 1 --提现类型 : 透过那种支付方式 1=支付宝 2=银联
	--declare @chvAccount varchar(50) = '2000111222' --提现账号 : 打钱的账号纪录
	--declare @tnyStatus  int=1 --状态 : 0=未发放 1=已发放

	DECLARE	@StartRow			INT = @intPageSize * (@intPageIndex -1) + 1
	DECLARE	@EndRow				INT = @intPageSize * @intPageIndex


	select	@intSumReward = [PoolAmount],
			@intSumConvert = [WithDrawAmount],
			@intRemainConvert = [PoolAmount] - [WithDrawAmount]
	from [dbo].[Unlimit_Pool] with (nolock)
	where
		userid = @intUserID

	set @intSumReward = isnull(@intSumReward,0)
	set @intSumConvert = isnull(@intSumConvert,0)
	set @intRemainConvert = isnull(@intRemainConvert,0)
	

	declare @temp_withdrawlog table (rowno int,StatTime varchar(10),OrderId bigint,Amount bigint,[Type] varchar(20),Account varchar(150),Status int)

	insert into @temp_withdrawlog
	select	row_number() over (order by [CreateTime] desc) as rowno,
			StatTime = convert(varchar(10),[CreateTime],23),
			OrderId = [ID],
			Amount = [WithDrawAmount],
			[Type] = case when [Type] = 1 then '支付宝'
							when [Type] = 2 then '银联卡' end,
			Account = case	when [Type] = 1 then '支付宝姓名 : ' + [RealName] + ' , 账号 :' + [AlipayAccount]
							when [Type] = 2 then '银联卡账号 : ' + [UnionpayBank] + ' - ' + [UnionpayAccount] + ' , ' + '银联卡姓名 :' + [UnionpayRealName]
						end,
			Status = [Status]
	from [dbo].[Unlimit_WithDrawLog] with (nolock)
	where
		userid = @intUserID
		and [WithdrawType] = 2


	/*  回传资料  */
	select	@intPageCount = max(rowno) from @temp_withdrawlog
	set @intPageCount = isnull(@intPageCount,0)


	SELECT	StatTime,
			OrderId,
			Amount,
			[Type],
			Account,
			Status
	FROM (
			SELECT	rowno,
					StatTime,
					OrderId,
					Amount = isnull(Amount,0),
					[Type] = isnull([Type],''),
					Account = isnull(Account,''),
					Status = isnull(Status,0)
			FROM @temp_withdrawlog
		) AS A
	WHERE
		rowno BETWEEN @StartRow AND @EndRow


	return 1
END




