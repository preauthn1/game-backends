-- =============================================
-- Author:  jason
-- Create date: 2019/04/17
-- Description: USER 今日值兑详情(净利润)
-- =============================================
CREATE PROCEDURE [dbo].[PrPs_NetProfitAgent_GetWithdrawDetail]
(
	@intUserID		int,		--用户 ID
	@dtStatTime		datetime	--查询日期
)
AS
BEGIN

	set nocount on

	declare @StartDate datetime = convert(varchar(10),@dtStatTime,23)
	declare @EndDate datetime = dateadd(day,1,convert(varchar(10),@dtStatTime,23))
	declare @LastDate datetime = dateadd(day,-1,convert(varchar(10),@dtStatTime,23))


	/*  取出用户识别  */
	declare @temp_relations varchar(max)

	select	@temp_relations = [Relations]
	from [dbo].[NetProfitAgent_UserInfo] with (nolock)
	where
		[UserID] = @intUserID


	/*  取出下线玩家  */
	declare @temp_Userinfo table (UserID int)

	insert into @temp_Userinfo
	select	UserID
	from [dbo].[NetProfitAgent_UserInfo] with (nolock)
	where
		Relations like '' + @temp_relations + '%'
		and [CreateTime] >= @StartDate
		and [CreateTime] < @EndDate

	
	/*  昨日充值  */
	declare @chargeamountlast bigint
	select	@chargeamountlast = sum([ChargeAmount])
	from @temp_Userinfo as a
		inner join [dbo].[NetProfitAgent_UserBonusLog] as b with (nolock) on a.userid = b.userid
	where
		[CreateTime] >= @LastDate
		and [CreateTime] < @StartDate

	set @chargeamountlast = isnull(@chargeamountlast,0)


	/*  回传资料  */
	select	chargeamountlast = @chargeamountlast

	select	[LogDate] = convert(varchar(10),b.[CreateTime],23),
			b.UserID,
			[ChargeAmount] = sum(isnull(b.[ChargeAmount],0)),
			[ConvertAmount] = sum(isnull(b.[ConvertAmount],0)),
			[StorageAmount] = sum(isnull(b.[WalletAmount],0)) + sum(isnull(b.[BankAmount],0)),
			[BonusAmount] = sum(isnull([BonusAmount],0))
	from @temp_Userinfo as a
		left join [dbo].[NetProfitAgent_UserBonusLog] as b with (nolock) on a.userid = b.userid
		left join [dbo].[NetProfitAgent_UserTaxLog] as c with (nolock) on b.id = c.RelationID
	where
		b.[CreateTime] >= @StartDate
		and b.[CreateTime] < @EndDate
	group by convert(varchar(10),b.[CreateTime],23),b.UserID
	
	return 1
END





