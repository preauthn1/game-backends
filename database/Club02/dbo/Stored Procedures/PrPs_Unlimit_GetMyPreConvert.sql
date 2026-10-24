
-- =============================================
-- Author:  jason
-- Create date: 20190501
-- Description: 无极限-代理预提
-- =============================================
CREATE PROCEDURE [dbo].[PrPs_Unlimit_GetMyPreConvert]
(
	@intUserID int,
	@intPageIndex	INT	= 1,
	@intPageSize	INT = 20,
	@intPageCount	INT = 0		OUTPUT
)
AS
BEGIN

	SET NOCOUNT ON


	declare @logsdate datetime = dbo.Fn_StartOfWeek1(getdate())
	--declare @logsdate datetime = dateadd(day,-7,getdate())
	declare @logedate datetime = dateadd(day,7,@logsdate)
	declare @loglastsdate datetime = dateadd(day,-7,@logsdate)
	declare @loglastedate datetime = dateadd(day,0,@logsdate)
	DECLARE	@StartRow			INT = @intPageSize * (@intPageIndex -1) + 1
	DECLARE	@EndRow				INT = @intPageSize * @intPageIndex

	declare @bonusamount bigint
	declare @preconvertamount bigint
	declare @withdrawlimitamount bigint
	declare @lastweekwithdrawamount bigint

	/*  取已小计结算总金额  */
	select	@bonusamount = sum([TeamBonusAmount]) + sum([DirectBonusAmount])
	from [dbo].[Unlimit_BonusReport] with (nolock)
	where
		userid = @intUserID
		and [LogDate] >= @logsdate
		and [LogDate] <= @logedate

	set @bonusamount = isnull(@bonusamount,0)


	/*  上周已预提金额  */
	select	@lastweekwithdrawamount = sum([PoolAmount])
	from [dbo].[Unlimit_WithDrawLog] with (nolock)
	where
		userid = @intUserID
		and [WithdrawType] = 1	-- 1:预提 2:提领
		and Status = 1
		and [CreateTime] >= @loglastsdate
		and [CreateTime] <= @loglastedate

	set @lastweekwithdrawamount = isnull(@lastweekwithdrawamount,0)


	/*  取当周已预提金额  */
	select	@preconvertamount = sum([PoolAmount])
	from [dbo].[Unlimit_WithDrawLog] with (nolock)
	where
		userid = @intUserID
		and [WithdrawType] = 1	-- 1:预提 2:提领
		and [CreateTime] >= @logsdate
		and [CreateTime] <= @logedate

	set @preconvertamount = isnull(@preconvertamount,0)
	
	--当前可提
	set @withdrawlimitamount = round(cast(cast(@bonusamount - @preconvertamount as float) * 0.9 as bigint),0)
	set @withdrawlimitamount = isnull(@withdrawlimitamount,0)


	declare @temp_preconvertlist table (rowno int,LogDate datetime,ID bigint,Amount bigint,ConvertType varchar(30),Account varchar(32),Status varchar(20))


	insert into @temp_preconvertlist
	select	row_number() over (order by createtime desc) as rowno,
			[CreateTime],
			[ID],
			[PoolAmount],
			ConvertType = '游戏内',
			Account = cast(UserID as varchar(10)),
			[Status] = case	when Status = 0 then '未发放'
							when Status = 1 then '已发放'
							when Status = 2 then '拒绝'
						end
	from [dbo].[Unlimit_WithDrawLog] with (nolock)
	where
		userid = @intUserID
		and [WithdrawType] = 1	-- 1:预提 2:提领
		--and [CreateTime] >= @logsdate
		--and [CreateTime] <= @logedate


	select	@intPageCount = count(1) from @temp_preconvertlist


	select	@bonusamount as bonusamount,
			@withdrawlimitamount as AmountLimit,
			@preconvertamount as PreConvertAmount,
			@bonusamount - @preconvertamount as RemainAmount,
			@lastweekwithdrawamount as withdrawlastweek


	SELECT	LogDate = convert(varchar(10),LogDate,23),
			ID,
			Amount,
			ConvertType,
			Account,
			Status
	FROM (
			SELECT	rowno,
					LogDate,
					ID,
					Amount,
					ConvertType,
					Account,
					Status
			FROM @temp_preconvertlist
		) AS A
	WHERE
		rowno BETWEEN @StartRow AND @EndRow


	return 1
END


