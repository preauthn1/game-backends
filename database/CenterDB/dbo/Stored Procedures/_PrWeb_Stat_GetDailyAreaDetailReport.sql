-- =============================================
-- Author:  Jason
-- Create date: 2018/06/26
-- Description:	每日報表 後台第三方帳號使用 #526
-- =============================================
CREATE PROCEDURE [dbo].[_PrWeb_Stat_GetDailyAreaDetailReport]
(
	@intAreaID	INT = NULL,
	@dtStatTime	DATETIME = NULL
)
AS
BEGIN
	
	SET NOCOUNT ON

	if (@intAreaID = 0)
	begin
		set @intAreaID = NULL
	end

	SET	@dtStatTime = ISNULL(@dtStatTime,GETDATE())

	declare @LogStartTime	DATETIME = DATEADD(DAY,0,DATEDIFF(DAY,0,@dtStatTime))
	declare @LogEndTime		DATETIME = DATEADD(DAY,1,DATEDIFF(DAY,0,@dtStatTime))
	declare @areaname varchar(32)

	select @areaname = [AreaName] from [Area].[Area] where areaid = @intAreaID
	set @areaname = isnull(@areaname,'')
	
	declare @dau int
	declare @agentsourcetypeid	int = 11008
	declare @intPageCount int

	select @dau = sum([TotalCount])
	from [Stat].[DAULog] with (nolock)
	where
		[StatTime] = @LogStartTime
		and platformid > 0
		and areaid = @intAreaID
		and gameid = 0
	set @dau = isnull(@dau,0)

	/*  充值  */
	-------------------------------------------------------------------------------------------
	--代理充值暫存
	declare @temp_agentcharge table (rowNum int,logid bigint,userid int,sourcetypeid int,currentamount bigint,updateamount bigint,taxamount bigint,ipaddress varchar(40),memo varchar(128),createtime datetime,areaid int,platformid int,tradeid bigint)
	
	insert into @temp_agentcharge
	exec logdb.[dbo].[PrWeb_Stat_GetMoneyLog]
		@beginTime = @LogStartTime,
		@endTime = @LogEndTime,
		@intSourceType = @agentsourcetypeid,
		@intAreaID = @intAreaID,
		@intPageIndex = 1,
		@intPageSize = 1000000,
		@intPageCount = @intPageCount output


	--在线充值統計
	declare @ChargeSuccessCount			int
	declare @ChargeSuccessUserCount		int
	declare @ChargeSuccessOldUserAmount	float
	declare @ChargeSuccessNewUserAmount	float
	declare @ChargeSuccessAmount	float
	declare @ChargeSuccessNewUserCount int

	select	@ChargeSuccessUserCount = SUM(A.[Total_OrderUser]),
			@ChargeSuccessOldUserAmount = SUM([OldCharge]) * 1.00,
			@ChargeSuccessNewUserAmount = SUM([NewCharge]) * 1.00,
			@ChargeSuccessAmount = (SUM([OldCharge]) + SUM([NewCharge])) * 1.00,
			@ChargeSuccessCount = SUM([TotalOrderCounts]),
			@ChargeSuccessNewUserCount = SUM([NewOrderCounts])
	from [CenterDB].[Stat].[AreaDataDay] as a
	where
		platformid > 0
		and isnull(@intAreaID,areaid) = areaid
		and gameid = 0 and serverid = 0
		and stattime = @LogStartTime

	set @ChargeSuccessCount = isnull(@ChargeSuccessCount,0)
	set @ChargeSuccessUserCount = isnull(@ChargeSuccessUserCount,0)
	set @ChargeSuccessOldUserAmount = isnull(@ChargeSuccessOldUserAmount,0)
	set @ChargeSuccessNewUserAmount = isnull(@ChargeSuccessNewUserAmount,0)
	set @ChargeSuccessAmount = isnull(@ChargeSuccessAmount,0)

	--代理充值統計
	declare @agentChargeSuccessCount			int
	declare @agentChargeSuccessUserCount		int
	declare @agentChargeSuccessOldUserAmount	float
	declare @agentChargeSuccessNewUserAmount	float
	declare @agentChargeSuccessAmount	float

	select	@agentChargeSuccessUserCount = count(DISTINCT A.UserID),
			@agentChargeSuccessOldUserAmount = SUM(CASE WHEN B.CreateTime < @LogStartTime THEN A.updateamount ELSE 0 END) * 1.00 / 100,
			@agentChargeSuccessNewUserAmount = SUM(CASE WHEN B.CreateTime > @LogStartTime THEN A.updateamount ELSE 0 END) * 1.00 / 100,
			@agentChargeSuccessAmount = SUM(A.updateamount) * 1.00 / 100,
			@agentChargeSuccessCount = count(1)
	from @temp_agentcharge as a
		inner join [Users].[UserInfo] as b on a.userid = b.userid
	where
		b.areaid = @intAreaID

	set @agentChargeSuccessCount = isnull(@agentChargeSuccessCount,0)
	set @agentChargeSuccessUserCount = isnull(@agentChargeSuccessUserCount,0)
	set @agentChargeSuccessOldUserAmount = isnull(@agentChargeSuccessOldUserAmount,0)
	set @agentChargeSuccessNewUserAmount = isnull(@agentChargeSuccessNewUserAmount,0)
	set @agentChargeSuccessAmount = isnull(@agentChargeSuccessAmount,0)

	--充值總數統計
	declare @totalchargesuccesscount int = @ChargeSuccessCount + @agentChargeSuccessCount
	declare @totalchargeusercount	int
	declare @olduserchargeamount float = @ChargeSuccessOldUserAmount + @agentChargeSuccessOldUserAmount
	declare @newuserchargeamount float = @ChargeSuccessNewUserAmount + @agentChargeSuccessNewUserAmount
	declare @olduserchargecount int
	declare @newuserchargecount int
	
	select	@totalchargeusercount = count(distinct a.userid) + @ChargeSuccessUserCount,
			@olduserchargecount = count(CASE WHEN B.CreateTime < @LogStartTime THEN NULL ELSE 1 END) + (@ChargeSuccessUserCount - @ChargeSuccessNewUserCount),
			@newuserchargecount = count(CASE WHEN B.CreateTime > @LogStartTime THEN NULL ELSE 1 END) + @ChargeSuccessNewUserCount
	from (
			select userid
			from @temp_agentcharge
		) as a
		inner join [Users].[UserInfo] as b on a.userid = b.userid
	where
		b.areaid = @intAreaID

	set @totalchargeusercount = isnull(@totalchargeusercount,0)
	set @olduserchargecount = isnull(@olduserchargecount,0)
	set @newuserchargecount = isnull(@newuserchargecount,0)


	declare @arpu float
	declare @arppu float
	declare @daupayrate float

	set @arpu = case when @dau > 0 then (@olduserchargeamount + @newuserchargeamount) / @dau else (@olduserchargeamount + @newuserchargeamount) end
	set @arppu = case when @totalchargeusercount > 0 then (@olduserchargeamount + @newuserchargeamount) / @totalchargeusercount else (@olduserchargeamount + @newuserchargeamount) end
	set @daupayrate = case when @dau > 1 then (@totalchargeusercount / @dau) * 1.00 * 100 when @dau = 1 then 100 else 0 end

	set @arpu = isnull(@arpu,0)
	set @arppu = isnull(@arppu,0)
	set @daupayrate = isnull(@daupayrate,0)


	declare @chargestring nvarchar(max)
	
	if (@intAreaID IS NOT NULL)
	begin
		set @chargestring = '{"充值成功筆數":' + cast(@totalchargesuccesscount as nvarchar)
							+ ',"充值總人數":' + cast(@totalchargeusercount as nvarchar)
							+ ',"老用戶充值金額":' + cast(@olduserchargeamount as nvarchar)
							+ ',"新用戶充值金額":' + cast(@newuserchargeamount as nvarchar)
							+ ',"老用戶充值人數":' + cast(@olduserchargecount as nvarchar)
							+ ',"新用戶充值人數":' + cast(@newuserchargecount as nvarchar)
							+ ',"在线充值金額":' + cast(@ChargeSuccessAmount as nvarchar)
							+ ',"在线充值筆數":' + cast(@ChargeSuccessCount as nvarchar)
							+ ',"在线充值人數":' + cast(@ChargeSuccessUserCount as nvarchar)
							+ ',"arpu":' + cast(@arpu as nvarchar)
							+ ',"arppu":' + cast(@arppu as nvarchar)
							+ ',"付費率":' + cast(@totalchargeusercount as nvarchar)
							+ ',"活躍用戶付費率":' + cast(@daupayrate as nvarchar)
							+ '}'
	end
	else
	begin
		set @chargestring = cast((@olduserchargeamount + @newuserchargeamount) as nvarchar)
	end
	-------------------------------------------------------------------------------------------

	
	/*  兌換  */
	-------------------------------------------------------------------------------------------
	declare @temp_convert table (userid int,converttype tinyint,[Amount] bigint)

	insert into @temp_convert
	select	userid,converttype,abs(amount)
	from logdb.[Money].[ConvertLog] with (nolock)
	where
		[CreateTime] >= @LogStartTime
		AND [CreateTime] < @LogEndTime
		and status = 3

	
	declare @olduserconvertamount float
	declare @newuserconvertamount float
	declare @olduserconvertcount int
	declare @newuserconvertcount int
	declare @alipayamount float
	declare @unionpayamount	float
	declare @alipayusercount int
	declare @unionpayusercount int


	select	@olduserconvertamount = sum(case when b.createtime < @LogStartTime then amount else 0 end) * 1.00 / 100,
			@newuserconvertamount = sum(case when b.createtime > @LogStartTime then amount else 0 end) * 1.00 / 100,
			@olduserconvertcount = count(case when b.createtime < @LogStartTime then a.userid else null end),
			@newuserconvertcount = count(case when b.createtime > @LogStartTime then a.userid else null end),
			@alipayamount = sum(case when converttype = 1 then amount else 0 end) * 1.00 / 100,
			@unionpayamount = sum(case when converttype = 2 then amount else 0 end) * 1.00 / 100,
			@alipayusercount = count(distinct case when converttype = 1 then a.userid else null end),
			@unionpayusercount = count(distinct case when converttype = 2 then a.userid else null end)
	from @temp_convert as a
		inner join [Users].[UserInfo] as b on a.userid = b.userid
	where
		b.areaid = @intAreaID

	set @olduserconvertamount = isnull(@olduserconvertamount,0)
	set @newuserconvertamount = isnull(@newuserconvertamount,0)
	set @olduserconvertcount = isnull(@olduserconvertcount,0)
	set @newuserconvertcount = isnull(@newuserconvertcount,0)
	set @alipayamount = isnull(@alipayamount,0)
	set @unionpayamount = isnull(@unionpayamount,0)
	set @alipayusercount = isnull(@alipayusercount,0)
	set @unionpayusercount = isnull(@unionpayusercount,0)

	declare @convertstring nvarchar(max)
	
	if (@intAreaID IS NOT NULL)
	begin
		set @convertstring = '{"老用戶兌換金幣":' + cast(@olduserconvertamount as nvarchar)
							+ ',"新用戶兌換金幣":' + cast(@newuserconvertamount as nvarchar)
							+ ',"老用戶兌換人數":' + cast(@olduserconvertcount as nvarchar)
							+ ',"新用戶兌換人數":' + cast(@newuserconvertcount as nvarchar)
							+ ',"支付寶兌換筆數":' + cast(@alipayamount as nvarchar)
							+ ',"銀行卡兌換筆數":' + cast(@unionpayamount as nvarchar)
							+ ',"支付寶兌換金幣":' + cast(@alipayusercount as nvarchar)
							+ ',"銀行卡兌換金幣":' + cast(@unionpayusercount as nvarchar)
							+ '}'
	end
	else
	begin
		set @convertstring = cast((@olduserconvertamount + @newuserconvertamount) as nvarchar)
	end
	-------------------------------------------------------------------------------------------

	
	/*  總金幣  */
	-------------------------------------------------------------------------------------------
	declare @temp_rejectconvert table (amount bigint)
	insert into @temp_rejectconvert
	select	amount
	from logdb.[Money].[ConvertLog] with (nolock)
	where
		[CreateTime] >= @LogStartTime
		AND [CreateTime] < @LogEndTime
		and status = 11

	declare @rejectconvertamount float

	select	@rejectconvertamount = sum(amount) * 1.00 / 100
	from @temp_rejectconvert

	declare @totalamountstring nvarchar(max)
	
	-------------------------------------------------------------------------------------------


	/*  註冊用戶數  */
	-------------------------------------------------------------------------------------------
	declare @reginfo	table (stattime datetime,areaid int,totalcount int,ios int,android int,iosbind int,androidbind int,ipcount int,newipcount int)

	insert into @reginfo
	exec logdb.[dbo].[PrWeb_Stat_GetRegCountByArea]
		@dtStatTime		= @LogStartTime,	
		@dtEndTime		= @LogEndTime

	declare @totalreguser int
	declare @userguest int
	declare @usernormal int
	declare @newip int
	declare @iosuser int
	declare @androiduser int

	select	@totalreguser = isnull(totalcount,0),
			@userguest = isnull(totalcount,0) - (isnull(iosbind,0) + isnull(androidbind,0)),
			@usernormal = (isnull(iosbind,0) + isnull(androidbind,0)),
			@newip = isnull(newipcount,0),
			@iosuser = isnull(ios,0),
			@androiduser = isnull(android,0)
	from @reginfo
	where
		areaid = @intAreaID

	set @totalreguser = isnull(@totalreguser,0)
	set @userguest = isnull(@userguest,0)
	set @usernormal = isnull(@usernormal,0)
	set @newip = isnull(@newip,0)
	set @iosuser = isnull(@iosuser,0)
	set @androiduser = isnull(@androiduser,0)

	declare @regstring nvarchar(max)

	if (@intAreaID IS NOT NULL)
	begin
		set @regstring = '{"注冊用戶去重數":' + cast(@totalreguser as nvarchar)
							+ ',"注冊用戶遊客數":' + cast(@userguest as nvarchar)
							+ ',"注冊用戶正式數":' + cast(@usernormal as nvarchar)
							+ ',"注冊用戶新IP數":' + cast(@newip as nvarchar)
							+ ',"注冊用戶IOS數":' + cast(@iosuser as nvarchar)
							+ ',"注冊用戶安卓數":' + cast(@androiduser as nvarchar)
							+ '}'
	end
	else
	begin
		set @regstring = cast(@totalreguser as nvarchar)
	end
	-------------------------------------------------------------------------------------------


	/*  登錄用戶數  */
	-------------------------------------------------------------------------------------------
	declare @logonstat table (stattime datetime,areaid int,totalcount int,ios int,android int,isbind int,isnew int,ipcount int)

	insert into @logonstat
	exec logdb.[dbo].[PrWeb_Stat_GetLogonCountByArea]
		@dtStatTime		= @LogStartTime,	
		@dtEndTime		= @LogEndTime

	declare @logonuser int
	declare @logonuserguest int
	declare @logonusernormal int
	declare @logonnewuser int
	declare @logonolduser int
	declare @logonipcount int
	declare @logonios int
	declare @logonandroid int
	declare @lastkeep int
	declare @thirdkeep int
	declare @weekkeep int

	select	@logonuser = totalcount,
			@logonuserguest = isnull(totalcount,0) - isnull(isbind,0),
			@logonusernormal = isnull(isbind,0),
			@logonnewuser = isnull(isnew,0),
			@logonolduser =  isnull(totalcount,0) - isnull(isnew,0),
			@logonipcount = ipcount,
			@logonios = ios,
			@logonandroid = android
	from @logonstat
	where
		areaid = @intAreaID

	select	@lastkeep = sum([OneDayKeep]),
			@thirdkeep = sum([Third]),
			@weekkeep = sum([WeekKeep])
	from [Stat].[KeepLog]
	where
		stattime = @LogStartTime
		and platformid > 0

	set @logonuser = isnull(@logonuser,0)
	set @logonuserguest = isnull(@logonuserguest,0)
	set @logonusernormal = isnull(@logonusernormal,0)
	set @logonnewuser = isnull(@logonnewuser,0)
	set @logonolduser = isnull(@logonolduser,0)
	set @logonipcount = isnull(@logonipcount,0)
	set @logonios = isnull(@logonios,0)
	set @logonandroid = isnull(@logonandroid,0)
	set @lastkeep = isnull(@lastkeep,0)
	set @thirdkeep = isnull(@thirdkeep,0)
	set @weekkeep = isnull(@weekkeep,0)

	declare @logonstring nvarchar(max)

	if (@intAreaID IS NOT NULL)
	begin
		set @logonstring = '{"玩過遊戲的人數":' + cast(@dau as nvarchar)
							+ ',"登錄用戶去重量":' + cast(@logonuser as nvarchar)
							+ ',"遊客帳號":' + cast(@logonuserguest as nvarchar)
							+ ',"正式帳號":' + cast(@logonusernormal as nvarchar)
							+ ',"登錄新用戶":' + cast(@logonnewuser as nvarchar)
							+ ',"登錄老用戶":' + cast(@logonolduser as nvarchar)
							+ ',"登錄IP數":' + cast(@logonipcount as nvarchar)
							+ ',"登錄IOS數":' + cast(@logonios as nvarchar)
							+ ',"登錄安卓數":' + cast(@logonandroid as nvarchar)
							+ ',"昨日留存":' + cast(@lastkeep as nvarchar)
							+ ',"三日留存":' + cast(@thirdkeep as nvarchar)
							+ ',"七日留存":' + cast(@weekkeep as nvarchar)
							+ '}'
	end
	else
	begin
		set @logonstring = cast(@logonuser as nvarchar)
	end
	-------------------------------------------------------------------------------------------


	/*  稅收  */
	-------------------------------------------------------------------------------------------
	declare @gametaxtotal float
	declare @gametax1 float
	declare @gametax2 float
	declare @gametax3 float
	declare @gametax4 float
	declare @gametax5 float
	declare @gametax6 float
	declare @gametax7 float
	declare @gametax8 float
	declare @gametax9 float
	declare @gametax10 float
	declare @gametax11 float
	declare @gametax12 float
	declare @gametax13 float
	declare @gametax14 float
	declare @gametax15 float
	declare @gametax16 float
	declare @gametax17 float
	declare @gametax18 float
	declare @gametax19 float

	select	@gametaxtotal = sum([UserTaxAmount]) * 1.00 / 100,
			@gametax1 = sum(case when gameid = 1 then [UserTaxAmount] else 0 end) * 1.00 / 100,
			@gametax2 = sum(case when gameid = 2 then [UserTaxAmount] else 0 end) * 1.00 / 100,
			@gametax3 = sum(case when gameid = 3 then [UserTaxAmount] else 0 end) * 1.00 / 100,
			@gametax4 = sum(case when gameid = 4 then [UserTaxAmount] else 0 end) * 1.00 / 100,
			@gametax5 = sum(case when gameid = 5 then [UserTaxAmount] else 0 end) * 1.00 / 100,
			@gametax6 = sum(case when gameid = 6 then [UserTaxAmount] else 0 end) * 1.00 / 100,
			@gametax7 = sum(case when gameid = 7 then [UserTaxAmount] else 0 end) * 1.00 / 100,
			@gametax8 = sum(case when gameid = 8 then [UserTaxAmount] else 0 end) * 1.00 / 100,
			@gametax9 = sum(case when gameid = 9 then [UserTaxAmount] else 0 end) * 1.00 / 100,
			@gametax10 = sum(case when gameid = 10 then [UserTaxAmount] else 0 end) * 1.00 / 100,
			@gametax11 = sum(case when gameid = 11 then [UserTaxAmount] else 0 end) * 1.00 / 100,
			@gametax12 = sum(case when gameid = 12 then [UserTaxAmount] else 0 end) * 1.00 / 100,
			@gametax13 = sum(case when gameid = 13 then [UserTaxAmount] else 0 end) * 1.00 / 100,
			@gametax14 = sum(case when gameid = 14 then [UserTaxAmount] else 0 end) * 1.00 / 100,
			@gametax15 = sum(case when gameid = 15 then [UserTaxAmount] else 0 end) * 1.00 / 100,
			@gametax16 = sum(case when gameid = 16 then [UserTaxAmount] else 0 end) * 1.00 / 100,
			@gametax17 = sum(case when gameid = 17 then [UserTaxAmount] else 0 end) * 1.00 / 100,
			@gametax18 = sum(case when gameid = 18 then [UserTaxAmount] else 0 end) * 1.00 / 100,
			@gametax19 = sum(case when gameid = 19 then [UserTaxAmount] else 0 end) * 1.00 / 100
	from [Stat].[GameAmountLog] with (nolock)
	where
		stattime = @LogStartTime
		and areaid = @intAreaID

	set @gametaxtotal = isnull(@gametaxtotal,0)
	set @gametax1 = isnull(@gametax1,0)
	set @gametax2 = isnull(@gametax2,0)
	set @gametax3 = isnull(@gametax3,0)
	set @gametax4 = isnull(@gametax4,0)
	set @gametax5 = isnull(@gametax5,0)
	set @gametax6 = isnull(@gametax6,0)
	set @gametax7 = isnull(@gametax7,0)
	set @gametax8 = isnull(@gametax8,0)
	set @gametax9 = isnull(@gametax9,0)
	set @gametax10 = isnull(@gametax10,0)
	set @gametax11 = isnull(@gametax11,0)
	set @gametax12 = isnull(@gametax12,0)
	set @gametax13 = isnull(@gametax13,0)
	set @gametax14 = isnull(@gametax14,0)
	set @gametax15 = isnull(@gametax15,0)
	set @gametax16 = isnull(@gametax16,0)
	set @gametax17 = isnull(@gametax17,0)
	set @gametax18 = isnull(@gametax18,0)
	set @gametax19 = isnull(@gametax19,0)

	declare @taxstring nvarchar(max)

	if (@intAreaID IS NOT NULL)
	begin
		set @taxstring = '{"斗地主玩家稅收":' + cast(@gametax1 as nvarchar)
							+ ',"抢庄牛牛玩家稅收":' + cast(@gametax2 as nvarchar)
							+ ',"诈金花玩家稅收":' + cast(@gametax3 as nvarchar)
							+ ',"德州扑克玩家稅收":' + cast(@gametax4 as nvarchar)
							+ ',"梭哈玩家稅收":' + cast(@gametax5 as nvarchar)
							+ ',"21点玩家稅收":' + cast(@gametax6 as nvarchar)
							+ ',"经典牛牛玩家稅收":' + cast(@gametax7 as nvarchar)
							+ ',"三公玩家稅收":' + cast(@gametax8 as nvarchar)
							+ ',"水果机玩家稅收":' + cast(@gametax9 as nvarchar)
							+ ',"十三水玩家稅收":' + cast(@gametax10 as nvarchar)
							+ ',"龙虎斗玩家稅收":' + cast(@gametax11 as nvarchar)
							+ ',"推筒子玩家稅收":' + cast(@gametax12 as nvarchar)
							+ ',"百家乐玩家稅收":' + cast(@gametax13 as nvarchar)
							+ ',"捕鱼玩家稅收":' + cast(@gametax14 as nvarchar)
							+ ',"四川麻将玩家稅收":' + cast(@gametax15 as nvarchar)
							+ ',"2人牛牛玩家稅收":' + cast(@gametax16 as nvarchar)
							+ ',"疯狂二八玩家稅收":' + cast(@gametax18 as nvarchar)
							+ ',"百人牛牛玩家稅收":' + cast(@gametax19 as nvarchar)
							+ '}'
	end
	else
	begin
		set @taxstring = cast(@gametaxtotal as nvarchar)
	end
	-------------------------------------------------------------------------------------------


	/*  遊戲紀錄  */
	-------------------------------------------------------------------------------------------
	declare @gamecountstat table (stattime datetime,areaid int,gameid int,gamecount int)

	insert into @gamecountstat
	exec logdb.[dbo].[PrWeb_Stat_GetGameCountByArea]
		@dtStatTime		= @LogStartTime,	
		@dtEndTime		= @LogEndTime


	declare @gamecounttotal int
	declare @gamecount1 int
	declare @gamecount2 int
	declare @gamecount3 int
	declare @gamecount4 int
	declare @gamecount5 int
	declare @gamecount6 int
	declare @gamecount7 int
	declare @gamecount8 int
	declare @gamecount9 int
	declare @gamecount10 int
	declare @gamecount11 int
	declare @gamecount12 int
	declare @gamecount13 int
	declare @gamecount14 int
	declare @gamecount15 int
	declare @gamecount16 int
	declare @gamecount17 int
	declare @gamecount18 int
	declare @gamecount19 int

	select	@gamecounttotal = sum(gamecount),
			@gamecount1 = sum(case when gameid = 1 then gamecount else 0 end),
			@gamecount2 = sum(case when gameid = 2 then gamecount else 0 end),
			@gamecount3 = sum(case when gameid = 3 then gamecount else 0 end),
			@gamecount4 = sum(case when gameid = 4 then gamecount else 0 end),
			@gamecount5 = sum(case when gameid = 5 then gamecount else 0 end),
			@gamecount6 = sum(case when gameid = 6 then gamecount else 0 end),
			@gamecount7 = sum(case when gameid = 7 then gamecount else 0 end),
			@gamecount8 = sum(case when gameid = 8 then gamecount else 0 end),
			@gamecount9 = sum(case when gameid = 9 then gamecount else 0 end),
			@gamecount10 = sum(case when gameid = 10 then gamecount else 0 end),
			@gamecount11 = sum(case when gameid = 11 then gamecount else 0 end),
			@gamecount12 = sum(case when gameid = 12 then gamecount else 0 end),
			@gamecount13 = sum(case when gameid = 13 then gamecount else 0 end),
			@gamecount14 = sum(case when gameid = 14 then gamecount else 0 end),
			@gamecount15 = sum(case when gameid = 15 then gamecount else 0 end),
			@gamecount16 = sum(case when gameid = 16 then gamecount else 0 end),
			@gamecount17 = sum(case when gameid = 17 then gamecount else 0 end),
			@gamecount18 = sum(case when gameid = 18 then gamecount else 0 end),
			@gamecount19 = sum(case when gameid = 19 then gamecount else 0 end)
	from @gamecountstat
	where
		areaid = @intAreaID

	set @gamecounttotal = isnull(@gamecounttotal,0)
	set @gamecount1 = isnull(@gamecount1,0)
	set @gamecount2 = isnull(@gamecount2,0)
	set @gamecount3 = isnull(@gamecount3,0)
	set @gamecount4 = isnull(@gamecount4,0)
	set @gamecount5 = isnull(@gamecount5,0)
	set @gamecount6 = isnull(@gamecount6,0)
	set @gamecount7 = isnull(@gamecount7,0)
	set @gamecount8 = isnull(@gamecount8,0)
	set @gamecount9 = isnull(@gamecount9,0)
	set @gamecount10 = isnull(@gamecount10,0)
	set @gamecount11 = isnull(@gamecount11,0)
	set @gamecount12 = isnull(@gamecount12,0)
	set @gamecount13 = isnull(@gamecount13,0)
	set @gamecount14 = isnull(@gamecount14,0)
	set @gamecount15 = isnull(@gamecount15,0)
	set @gamecount16 = isnull(@gamecount16,0)
	set @gamecount17 = isnull(@gamecount17,0)
	set @gamecount18 = isnull(@gamecount18,0)
	set @gamecount19 = isnull(@gamecount19,0)

	declare @gametimetotal int
	declare @gametime1 int
	declare @gametime2 int
	declare @gametime3 int
	declare @gametime4 int
	declare @gametime5 int
	declare @gametime6 int
	declare @gametime7 int
	declare @gametime8 int
	declare @gametime9 int
	declare @gametime10 int
	declare @gametime11 int
	declare @gametime12 int
	declare @gametime13 int
	declare @gametime14 int
	declare @gametime15 int
	declare @gametime16 int
	declare @gametime17 int
	declare @gametime18 int
	declare @gametime19 int

	select	@gametimetotal = sum([GameTimeAvg]),
			@gametime1 = sum(case when gameid = 1 then [GameTimeAvg] else 0 end),
			@gametime2 = sum(case when gameid = 2 then [GameTimeAvg] else 0 end),
			@gametime3 = sum(case when gameid = 3 then [GameTimeAvg] else 0 end),
			@gametime4 = sum(case when gameid = 4 then [GameTimeAvg] else 0 end),
			@gametime5 = sum(case when gameid = 5 then [GameTimeAvg] else 0 end),
			@gametime6 = sum(case when gameid = 6 then [GameTimeAvg] else 0 end),
			@gametime7 = sum(case when gameid = 7 then [GameTimeAvg] else 0 end),
			@gametime8 = sum(case when gameid = 8 then [GameTimeAvg] else 0 end),
			@gametime9 = sum(case when gameid = 9 then [GameTimeAvg] else 0 end),
			@gametime10 = sum(case when gameid = 10 then [GameTimeAvg] else 0 end),
			@gametime11 = sum(case when gameid = 11 then [GameTimeAvg] else 0 end),
			@gametime12 = sum(case when gameid = 12 then [GameTimeAvg] else 0 end),
			@gametime13 = sum(case when gameid = 13 then [GameTimeAvg] else 0 end),
			@gametime14 = sum(case when gameid = 14 then [GameTimeAvg] else 0 end),
			@gametime15 = sum(case when gameid = 15 then [GameTimeAvg] else 0 end),
			@gametime16 = sum(case when gameid = 16 then [GameTimeAvg] else 0 end),
			@gametime17 = sum(case when gameid = 17 then [GameTimeAvg] else 0 end),
			@gametime18 = sum(case when gameid = 18 then [GameTimeAvg] else 0 end),
			@gametime19 = sum(case when gameid = 19 then [GameTimeAvg] else 0 end)
	from [Stat].[AreaDataDay] with (nolock)
	where
		platformid > 0
		and isnull(@intAreaID,areaid) = areaid
		and gameid > 0 and serverid = 0
		and stattime = @LogStartTime

	set @gametimetotal = isnull(@gametimetotal,0)
	set @gametime1 = isnull(@gametime1,0)
	set @gametime2 = isnull(@gametime2,0)
	set @gametime3 = isnull(@gametime3,0)
	set @gametime4 = isnull(@gametime4,0)
	set @gametime5 = isnull(@gametime5,0)
	set @gametime6 = isnull(@gametime6,0)
	set @gametime7 = isnull(@gametime7,0)
	set @gametime8 = isnull(@gametime8,0)
	set @gametime9 = isnull(@gametime9,0)
	set @gametime10 = isnull(@gametime10,0)
	set @gametime11 = isnull(@gametime11,0)
	set @gametime12 = isnull(@gametime12,0)
	set @gametime13 = isnull(@gametime13,0)
	set @gametime14 = isnull(@gametime14,0)
	set @gametime15 = isnull(@gametime15,0)
	set @gametime16 = isnull(@gametime16,0)
	set @gametime17 = isnull(@gametime17,0)
	set @gametime18 = isnull(@gametime18,0)
	set @gametime19 = isnull(@gametime19,0)

	declare @gamelogstring nvarchar(max)

	if (@intAreaID IS NOT NULL)
	begin
		set @gamelogstring = '{"斗地主次數":' + cast(@gamecount1 as nvarchar)
							+ ',"抢庄牛牛次數":' + cast(@gamecount2 as nvarchar)
							+ ',"诈金花次數":' + cast(@gamecount3 as nvarchar)
							+ ',"德州扑克次數":' + cast(@gamecount4 as nvarchar)
							+ ',"梭哈次數":' + cast(@gamecount5 as nvarchar)
							+ ',"21点次數":' + cast(@gamecount6 as nvarchar)
							+ ',"经典牛牛次數":' + cast(@gamecount7 as nvarchar)
							+ ',"三公次數":' + cast(@gamecount8 as nvarchar)
							+ ',"水果机次數":' + cast(@gamecount9 as nvarchar)
							+ ',"十三水次數":' + cast(@gamecount10 as nvarchar)
							+ ',"龙虎斗次數":' + cast(@gamecount11 as nvarchar)
							+ ',"推筒子次數":' + cast(@gamecount12 as nvarchar)
							+ ',"百家乐次數":' + cast(@gamecount13 as nvarchar)
							+ ',"捕鱼次數":' + cast(@gamecount14 as nvarchar)
							+ ',"四川麻将次數":' + cast(@gamecount15 as nvarchar)
							+ ',"2人牛牛次數":' + cast(@gamecount16 as nvarchar)
							+ ',"疯狂二八次數":' + cast(@gamecount18 as nvarchar)
							+ ',"百人牛牛次數":' + cast(@gamecount19 as nvarchar)
							+ ',"斗地主平均時長":' + cast(@gametime1 as nvarchar)
							+ ',"抢庄牛牛平均時長":' + cast(@gametime2 as nvarchar)
							+ ',"诈金花平均時長":' + cast(@gametime3 as nvarchar)
							+ ',"德州扑克平均時長":' + cast(@gametime4 as nvarchar)
							+ ',"梭哈平均時長":' + cast(@gametime5 as nvarchar)
							+ ',"21点平均時長":' + cast(@gametime6 as nvarchar)
							+ ',"经典牛牛平均時長":' + cast(@gametime7 as nvarchar)
							+ ',"三公平均時長":' + cast(@gametime8 as nvarchar)
							+ ',"水果机平均時長":' + cast(@gametime9 as nvarchar)
							+ ',"十三水平均時長":' + cast(@gametime10 as nvarchar)
							+ ',"龙虎斗平均時長":' + cast(@gametime11 as nvarchar)
							+ ',"推筒子平均時長":' + cast(@gametime12 as nvarchar)
							+ ',"百家乐平均時長":' + cast(@gametime13 as nvarchar)
							+ ',"捕鱼平均時長":' + cast(@gametime14 as nvarchar)
							+ ',"四川麻将平均時長":' + cast(@gametime15 as nvarchar)
							+ ',"2人牛牛平均時長":' + cast(@gametime16 as nvarchar)
							+ ',"疯狂二八平均時長":' + cast(@gametime18 as nvarchar)
							+ ',"百人牛牛平均時長":' + cast(@gametime19 as nvarchar)
							+ '}'
	end
	else
	begin
		set @gamelogstring = cast(@gamecounttotal as nvarchar)
	end
	-------------------------------------------------------------------------------------------


	/*  營收  */
	-------------------------------------------------------------------------------------------
	declare @chargeconvertstring nvarchar(max)

	if (@intAreaID IS NOT NULL)
	begin
		set @chargeconvertstring = ' '
	end
	else
	begin
		set @chargeconvertstring = cast(((@newuserchargeamount + @olduserchargeamount) - (@newuserconvertamount + @olduserconvertcount)) as nvarchar)
	end
	-------------------------------------------------------------------------------------------


	/*  回傳資料  */
	-------------------------------------------------------------------------------------------
	select	[StatTime] = @LogStartTime,
			[Area] = @areaname,
			[ChargeConvert] = @chargeconvertstring,
			[Charge] = @chargestring,
			[Convert] = @convertstring,
			[GameAmount] = @totalamountstring,
			[RegUser] = @regstring,
			[LogonUser] = @logonstring,
			[Tax] = @taxstring,
			[GameLog] = @gamelogstring,
			[LastUpdate] = @LogStartTime
	-------------------------------------------------------------------------------------------
END
GO
GRANT EXECUTE
    ON OBJECT::[dbo].[_PrWeb_Stat_GetDailyAreaDetailReport] TO [GameUser]
    AS [dbo];


GO
GRANT EXECUTE
    ON OBJECT::[dbo].[_PrWeb_Stat_GetDailyAreaDetailReport] TO [xbuser]
    AS [dbo];

