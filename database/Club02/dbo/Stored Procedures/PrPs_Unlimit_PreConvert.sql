
-- =============================================
-- Author:  jason
-- Create date: 20190501
-- Description: 无极限-代理预提
-- =============================================
CREATE PROCEDURE [dbo].[PrPs_Unlimit_PreConvert]
(
	@intUserID int,
	@intPageIndex	INT	= 1,
	@intPageSize	INT = 20,
	@intPageCount	INT = 0		OUTPUT
)
AS
BEGIN

	SET NOCOUNT ON


	declare @logsdate datetime = dateadd(day,-1,dateadd(week,datediff(week,0,getdate()),0))
	declare @logedate datetime = dateadd(day,7,@logsdate)
	DECLARE	@StartRow			INT = @intPageSize * (@intPageIndex -1) + 1
	DECLARE	@EndRow				INT = @intPageSize * @intPageIndex

	declare @bonusamount bigint
	declare @preconvertamount bigint
	declare @withdrawlimitamount bigint


	/*  取已小计结算总金额  */
	select	@bonusamount = sum([TeamBonusAmount]) + sum([DirectBonusAmount])
	from [dbo].[Unlimit_BonusReport] with (nolock)
	where
		userid = @intUserID
		and [LogDate] >= @logsdate
		and [LogDate] <= @logedate

	set @bonusamount = isnull(@bonusamount,0)


	select @bonusamount,@logsdate,@logedate


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


	declare @temp_preconvertlist table (rowno int,LogDate datetime,ID bigint,Amount bigint,ConvertType varchar(30),Account varchar(32),Status varchar(10))


	insert into @temp_preconvertlist
	select	row_number() over (order by createtime desc) as rowno,
			[CreateTime],
			[ID],
			[PoolAmount],
			ConvertType = '游戏内',
			Account = cast(UserID as varchar(10)),
			Status = case	when Status = 0 then '未发放'
							when Status = 1 then '已发放'
							when Status = 2 then '拒绝'
						end
	from [dbo].[Unlimit_WithDrawLog] with (nolock)
	where
		userid = @intUserID
		and [WithdrawType] = 1	-- 1:预提 2:提领
		and [CreateTime] >= @logsdate
		and [CreateTime] <= @logedate


	select	@intPageCount = count(1) from @temp_preconvertlist
	set @intPageCount = isnull(@intPageCount,0)


	select	@bonusamount as bonusamount,
			@withdrawlimitamount as AmountLimit,
			@preconvertamount as PreConvertAmount,
			@bonusamount - @preconvertamount


	SELECT	LogDate,
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


