-- =============================================
-- Author:  jason
-- Create date: 2019/04/18
-- Description: 全民代理后台 今日值兑详情(净利润)
-- =============================================
CREATE PROCEDURE [dbo].[PrWeb_NetProfitAgent_GetWithdrawDetail]
(
	@intAdminID		int = null,			--管理者
	@intUserID		int = null,			--用户 ID
	@dtStartTime		datetime = null,	--查询起始日期
	@dtEndTime		datetime = null,	--查询结束日期
	@chvMsg		varchar(100) OUTPUT
)
AS
BEGIN

	set nocount on

	declare @StartDate datetime = convert(varchar(10),isnull(@dtStartTime,'1970-01-01'),23)
	declare @EndDate datetime = dateadd(day,1,convert(varchar(10),isnull(@dtEndTime,'2100-01-01'),23))
	declare @LastDate datetime = dateadd(day,-1,convert(varchar(10),isnull(@dtStartTime,'1970-01-01'),23))
	declare @chvUserid varchar(20) = CAST(@intUserID as varchar)
	declare @LogDate datetime
	declare @LogTime int

	select top 1 @LogDate = LogDate, @LogTime=LogTime 
	from [dbo].[NetProfitAgent_UserTaxLog] 
	where LogDate >= @StartDate and LogDate < @EndDate	
	order by LogDate desc,LogTime desc

	--平台暂存
	DECLARE @Temp_PlatformID TABLE (PlatformID	INT,PlatformName varchar(50),AreaID int,AreaName varchar(50))

	declare @chargeamountlast bigint

	if(@intAdminID is null and @intUserID is null)
	begin
		set @chvMsg = '操作错误 : 用户不存在'
		return -1
	end
	
	if(@intAdminID is not null)
	begin

		--捞出该用户可读取的全部平台
		INSERT INTO @Temp_PlatformID
		Select	B.PlatformID,
				C.Name,
				B.AreaID,
				B.AreaName
		From [dbo].[AgentPlatformRelations] as a
			inner join CenterDB.area.area as b with (nolock) on a.PlatformID = B.PlatformID
			inner join CenterDB.area.[platform] as c with (nolock) on b.PlatformID = c.PlatformID
		where
			a.AdminID = @intAdminID

		/*  昨日充值  */
		select	@chargeamountlast = sum([ChargeAmount])
		from @Temp_PlatformID as a
			inner join [dbo].[NetProfitAgent_UserInfo] as b with (nolock) on a.PlatformID = b.PlatformID and a.AreaID = b.AreaID
			inner join [dbo].[NetProfitAgent_UserBonusLog] as c with (nolock) on b.userid = c.userid
		where
			(b.userid = @intUserID or @intUserID is null)
			and c.[CreateTime] >= @LastDate
			and c.[CreateTime] < @StartDate

		set @chargeamountlast = isnull(@chargeamountlast,0)

		/*  回传资料  */
		select	chargeamountlast = @chargeamountlast

		select	[LogDate] = convert(varchar(10),b.[CreateTime],23),
				b.UserID,
				[ChargeAmount] = sum(isnull(d.[ChargeAmount],0)),
				[ConvertAmount] = sum(isnull(d.[ConvertAmount],0)),
				[StorageAmount] = sum(isnull(d.[WalletAmount],0)) + sum(isnull(d.[BankAmount],0)),
				[BonusAmount] = sum(isnull(c.[BonusAmount],0))
		from @Temp_PlatformID as a
			inner join [dbo].[NetProfitAgent_UserInfo] as b with (nolock) on a.PlatformID = b.PlatformID and a.AreaID = b.AreaID
			left join [dbo].[NetProfitAgent_BonusTicket] as c with (nolock) on b.UserID = c.UserID and c.LogDate = @LogDate and c.LogHour = @LogTime
			inner join [dbo].[NetProfitAgent_UserTaxLog] as d with (nolock) on b.userid = d.userid and d.LogDate = @LogDate and d.LogTime = @LogTime
		where
			(b.userid = @intUserID or @intUserID is null)
			and d.[CreateTime] >= @StartDate
			and d.[CreateTime] < @EndDate
		group by convert(varchar(10),b.[CreateTime],23),b.UserID
		order by convert(varchar(10),b.[CreateTime],23) desc

		set @chvMsg = '操作完成 : 平台数据查询完成'
		return 1
	end


	if(@intUserID is not null)
	begin
		/*  昨日充值  */
		select	@chargeamountlast = sum([ChargeAmount])
		from [dbo].[NetProfitAgent_UserBonusLog] with (nolock)
		where
			userid = @intUserID
			and [CreateTime] >= @LastDate
			and [CreateTime] < @StartDate

		set @chargeamountlast = isnull(@chargeamountlast,0)

		/*  回传资料  */
		select	chargeamountlast = @chargeamountlast

		select	[LogDate] = convert(varchar(10),b.[CreateTime],23),
				b.UserID,
				[ChargeAmount] = sum(isnull(b.[ChargeAmount],0)),
				[ConvertAmount] = sum(isnull(b.[ConvertAmount],0)),
				[StorageAmount] = sum(isnull(b.[WalletAmount],0)) + sum(isnull(b.[BankAmount],0)),
				[BonusAmount] = sum(isnull(c.[BonusAmount],0))
		from [dbo].[NetProfitAgent_UserInfo] as a with (nolock)
			inner join [dbo].[NetProfitAgent_UserTaxLog] as b with (nolock) on a.UserID = b.UserID and b.LogDate = @LogDate and b.LogTime = @LogTime
			left join [dbo].[NetProfitAgent_BonusTicket] as c with (nolock) on a.UserID = c.UserID and c.LogDate = @LogDate and c.LogHour = @LogTime
		where
			a.Relations LIKE '%' + @chvUserid + '%'
			and b.[CreateTime] >= @StartDate
			and b.[CreateTime] < @EndDate
					group by convert(varchar(10),b.[CreateTime],23),b.UserID
		order by convert(varchar(10),b.[CreateTime],23) desc

		set @chvMsg = '操作完成 : 用户个人数据查询完成'
		return 1
	end
	
	return 1
END







