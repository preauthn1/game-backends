-- =============================================
-- Author:  jason
-- Create date: 2019/04/15
-- Description: USER 新增玩家(净利润)
-- =============================================
CREATE PROCEDURE [dbo].[PrPs_NetProfitAgent_GetSubUserList]
(
	@intUserID		int,		--用户 ID
	@dtStatDate	datetime
)
AS
BEGIN

	set nocount on

	declare @StartDate	datetime
	declare @EndDate	datetime
	declare @LastDate	datetime

	set @StartDate = dateadd(day,0,datediff(day,0,isnull(@dtStatDate,'1970-01-01')))
	set @EndDate = dateadd(day,1,datediff(day,0,isnull(@dtStatDate,'1970-01-01')))
	set @LastDate = dateadd(day,-1,@StartDate)


	/*  取出用户识别  */
	declare @temp_relations varchar(max)

	select	@temp_relations = [Relations]
	from [dbo].[NetProfitAgent_UserInfo] with (nolock)
	where
		[UserID] = @intUserID


	/*  取出下线玩家  */
	declare @temp_Userinfo table (UserID int,RegDate datetime,[SumChargeAmount] bigint,[SumWithDrawAmount] bigint)

	insert into @temp_Userinfo
	select	UserID,
			RegDate = [CreateTime],
			[SumChargeAmount],
			[SumWithDrawAmount]
	from [dbo].[NetProfitAgent_UserInfo] with (nolock)
	where
		Relations like '' + @temp_relations + '%'
		and status = 1
		and [CreateTime] >= @StartDate
		and [CreateTime] < @EndDate


	/*  昨日人数  */
	declare @totaluserlast	int
	select	@totaluserlast = count(1)
	from [dbo].[NetProfitAgent_UserInfo] with (nolock)
	where
		Relations like '' + @temp_relations + '%'
		and status = 1
		and [CreateTime] >= @LastDate
		and [CreateTime] < @StartDate

	set @totaluserlast = isnull(@totaluserlast,0)


	/*  取出产生的净利润  */
	declare @temp_UserBonusAmount table (UserID int,[BonusAmount] bigint)

	insert into @temp_UserBonusAmount
	select	a.userid,
			[BonusAmount] = sum(isnull([UpdateAmount],0))
	from @temp_Userinfo as a
		left join [dbo].[NetProfitAgent_UserBonusLog] as b with (nolock) on a.userid = b.userid
	group by a.userid


	
	
	/*  回传资料  */
	select	totaluserlast = @totaluserlast

	select	a.UserID,
			RegDate,
			[SumChargeAmount],
			[SumWithDrawAmount],
			[BonusAmount]
	from @temp_Userinfo as a
		inner join @temp_UserBonusAmount as b on a.userid = b.userid

	return 1
END





