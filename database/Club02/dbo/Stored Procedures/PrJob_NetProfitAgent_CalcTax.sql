
-- =============================================
-- Author:  jason
-- Create date: 2019/04/25
-- Description:	全民代理 淨利潤計算
-- Changelog:
--	2019/06/18	CCC		增加計算代理充值
-- =============================================
CREATE PROCEDURE [dbo].[PrJob_NetProfitAgent_CalcTax]
(
	@dtStatTime	datetime = null
)
AS
BEGIN
	SET NOCOUNT ON


	declare @startdate datetime = dateadd(day,0,datediff(day,0,isnull(@dtStatTime,getdate()-1)))
	declare @enddate datetime = dateadd(day,0,datediff(day,0,isnull(@dtStatTime,getdate())))
	declare @logdate datetime = convert(varchar(10),@startdate,23)
	declare @loghour int = datepart(hour,isnull(@dtStatTime,getdate()))

	/*  取所有淨利潤用戶ID  */
	declare @temp_netprofituser table (userid int,rate int,stagelevel tinyint)
	insert into @temp_netprofituser
	select userid,
			rate,
			stagelevel
	from [dbo].[NetProfitAgent_UserInfo] with (nolock)
	where
		status in (1,2)

	/*  寫入當下用戶暫存  */
	declare @temp_userlog table (LogDate datetime,userid int,walletamount bigint,bankamount bigint)

	insert into @temp_userlog
	select	@logdate,
			a.userid,
			walletamount = isnull(d.amount,0),
			bankamount = isnull(e.amount,0)
	from @temp_netprofituser as a
		left join centerdb.money.cash as d with (nolock) on a.userid = d.userid
		left join centerdb.money.bank as e with (nolock) on a.userid = e.userid

	if exists (select top 1 1 from [dbo].[NetProfitAgent_UserLog] where logdate = @logdate)
	begin
		delete from [dbo].[NetProfitAgent_UserLog] where logdate = @logdate
	end
	insert into [dbo].[NetProfitAgent_UserLog]
	select * from @temp_userlog


	/*  淨利潤所有用戶ID所產生的產值  從最後階層回來 */
	declare @temp_usertaxlog table (rowno int,userid int,lv int,chargeamount bigint,convertamount bigint,walletamount bigint,bankamount bigint,totalvalues bigint)
	
	--用户充值总计
	;WITH chargeamount as
	(
	select userid, Amount = sum(Amount)
	 FROM (select userid,Amount = sum(Amount) * 100 from centerdb.pay.orderlist with (nolock)
		   where status = 2 and LastUpdateTime >= '2019-04-27'
		   group by userid
		   union all
		   select userid, Amount = sum(UpdateAmount) from [LogDB].[Money].[MoneyLog] with (nolock) 
		   where SourceTypeID = 11008 and CreateTime >= '2019-04-27'
		   group by userid) a
	group by userid
	)
	
	insert into @temp_usertaxlog
	select	row_number() over (order by a.stagelevel desc) as rowno,
			a.userid,
			lv = a.stagelevel,
			chargeamount = sum(isnull(b.Amount,0)),
			convertamount = sum(abs(isnull(c.UpdateAmount,0))),
			walletamount = sum(isnull(d.walletamount,0)),
			bankamount = sum(isnull(d.bankamount,0)),
			totalvalues = sum(isnull(b.Amount,0)) - (sum(abs(isnull(c.UpdateAmount,0))) + sum(isnull(d.walletamount,0)) + sum(isnull(d.bankamount,0)))
	from @temp_netprofituser as a
		left join chargeamount as b on a.userid = b.userid
		left join (
					select userid,UpdateAmount = SUM(updateamount)
					from logdb.money.moneylog as c with (nolock)
					where c.SourceTypeID in (14000,14002) and c.CreateTime >= '2019-04-27'
					group by userid
				) as c on a.userid = c.userid
		left join @temp_userlog as d on a.userid = d.userid
	group by a.userid,a.stagelevel


	/*  取淨利潤每筆玩家產值計算到暫存表  紀錄每筆要反饋給上層的  */
	declare @minno int
	declare @maxno int

	select	@minno = min(rowno),
			@maxno = max(rowno)
	from @temp_usertaxlog

	create table #temp_tablea (userid int,id int,rate int)
	create table #temp_tableb (userid int,id int,rate int)
	create table #lowerusers (userid int)
	create table #everyuseramount (userid int,rate int,fromuser int,fromuserrate int,diffrate int,fromamount bigint,fromamountwithoutrate bigint)

	while(@minno <= @maxno)
	begin
		/*  取階層紀錄 從第二筆開始取  */
		declare @countuserid int = null
		declare @bonusvalue bigint = null
		declare @lowrelations varchar(max) = null
		declare @upperrelations varchar(max) = null

		select	@countuserid = userid,
				@bonusvalue = totalvalues
		from @temp_usertaxlog
		where
			rowno = @minno
	
		if(@minno >= 1)	--第一筆是最底層 不用再計算下層 所以略過
		begin
			select	--@lowrelations = [Relations],
					@upperrelations = [Relations]
			from [dbo].[NetProfitAgent_UserInfo] with (nolock)
			where
				userid = @countuserid

			insert into #temp_tablea
			select cast(value as int),
					cast(id as int),
					rate
			from [dbo].[fn_Split](@upperrelations,',') as a
				inner join @temp_netprofituser as b on a.value = b.userid
			order by id asc

			insert into #temp_tableb
			select cast(value as int),
					cast(id as int),
					rate
			from [dbo].[fn_Split](@upperrelations,',') as a
				inner join @temp_netprofituser as b on a.value = b.userid
			order by id asc


			--取下層用戶 下層總收入
			insert into #lowerusers
			select	distinct userid
			from [dbo].[NetProfitAgent_UserInfo] with (nolock)
			where
				relations like '' + @upperrelations + '%'


			--計算下層需分潤給哪些上層
			insert into #everyuseramount (userid,rate,fromuser,fromuserrate,diffrate,fromamount,fromamountwithoutrate)
			select	distinct
					a.userid,
					a.rate,
					fromuser = @countuserid,
					b.rate,
					diffrate = (case when b.userid = @countuserid then a.rate else (a.rate - b.rate) end),
					fromamount = @bonusvalue,
					fromamountwithoutrate = @bonusvalue * (case when b.userid = @countuserid then a.rate else (a.rate - b.rate) end)
			from #temp_tablea as a
				left join #temp_tableb as b on (a.id + 1) = b.id
			where
				a.userid <> @countuserid

			truncate table #temp_tablea
			truncate table #temp_tableb
			truncate table #lowerusers
		end

		set @minno = @minno + 1
	end

	/*  最後處理bonuslog 及寫入對應  */
	insert into [dbo].[NetProfitAgent_UserTaxLog]([LogDate],[LogTime],[UserID],[ChargeAmount],[ConvertAmount],[WalletAmount],[BankAmount],[BonusAmount],[CreateTime])
	select @logDate,@loghour,userid,chargeamount,convertamount,walletamount,bankamount,totalvalues,getdate()
	from @temp_usertaxlog


	/*  寫入BonusTicket  */
	declare @temp_bonusticket table (rowno int,userid int,total bigint,owners varchar(max))
	insert into @temp_bonusticket
	select	row_number() over (order by userid asc) as rowno,
			userid,
			total = round(cast(sumfromamountwithoutrate as float) * 1.00 / 1000,0,1)
			,Owners
	from (
			select	userid,
					--diffrate,
					sumfromamount = sum(fromamount),
					sumfromamountwithoutrate = sum(fromamountwithoutrate)
					, STUFF((
									SELECT ',' + cast(b.fromuser as varchar)
									FROM #everyuseramount as b
									WHERE a.userid = b.userid
									FOR XML PATH('')
						), 1, 1, '') AS Owners
			from #everyuseramount as a
			group by userid
	) as a

	insert into [dbo].[NetProfitAgent_BonusTicket]([LogDate],[LogHour],[UserID],[BonusAmount],[Relations])
	select	@LogDate,@loghour,userid,total,owners
	from @temp_bonusticket

	/*  寫入每個用戶轉換貢獻  */
	insert into [dbo].[NetProfitAgent_TaxTransferLog]([LogDate],[LogHour],[UserID],[Rate],[FromUserID],[FromUserRate],[Diffrate],[FromAmount],[FromAmountWithoutRate])
	select @logdate,@LogHour,* from #everyuseramount
	order by userid

	select	@minno = min(rowno),
			@maxno = max(rowno)
	from @temp_bonusticket

	while(@minno <= @maxno)
	begin
		declare @pooluser int = null
		declare @poolamount bigint = null

		select	@pooluser = userid,
				@poolamount = total
		from @temp_bonusticket
		where
			rowno = @minno

		if(@pooluser is not null)
		begin
			update [dbo].[NetProfitAgent_Pool]
			set [PoolAmount] = isnull(@poolamount,0),
				--[PoolAmount] = [PoolAmount] + isnull(@poolamount,0),
				[UpdateTime] = getdate()
			where
				userid = @pooluser
		end
	
		set @minno = @minno + 1
	end


	select	@minno = min(rowno),
			@maxno = max(rowno)
	from @temp_usertaxlog

	create table #Temp_chargeconvertuser (rowno int,userid int,sourceuserid int)
	create table #temp_countchargeconvert (temp_userid int,charge bigint,converts bigint)

	insert into #temp_countchargeconvert
	select distinct userid,0,0
	from [dbo].[NetProfitAgent_UserInfo] with (nolock)

	while(@minno <= @maxno)
	begin
		declare @temp_relations varchar(max) = null
		declare @chargeamount bigint = 0
		declare @convertamount bigint = 0
		declare @sourceuserid int = null

		select	@temp_relations = Relations,
				@chargeamount = a.chargeamount,
				@convertamount = a.convertamount,
				@sourceuserid = a.userid
		from @temp_usertaxlog as a
			inner join [dbo].[NetProfitAgent_UserInfo] as b with (nolock) on a.userid = b.userid
		where
			rowno = @minno

		if(@temp_relations is not null)
		begin
			insert into #Temp_chargeconvertuser
			select	cast(id as int),
					cast(value as int),
					@sourceuserid
			from [dbo].[fn_Split](@temp_relations,',') as a
			order by id asc


			--select * from #Temp_chargeconvertuser

			update #temp_countchargeconvert
			set charge = charge + @chargeamount,
				converts = converts + @convertamount
			from #Temp_chargeconvertuser as a
			where
				temp_userid = a.userid
				and temp_userid <> @sourceuserid

			--declare @subminno int
			--declare @submaxno int

			--select	@subminno = min(rowno),
			--		@submaxno = max(rowno)
			--from #Temp_chargeconvertuser

			--while(@subminno <= @submaxno)
			--begin
			--	declare @updateuserid int = null
			--	declare @targeruserid int = null
			--	select	@updateuserid = userid,
			--			@targeruserid = sourceuserid
			--	from #Temp_chargeconvertuser
			--	where
			--		rowno = @subminno


			--	update [dbo].[NetProfitAgent_UserInfo]
			--	set [SumSubChargeAmount] = case when @targeruserid <> @updateuserid and @updateuserid = userid then [SumSubChargeAmount] + @chargeamount else [SumSubChargeAmount] end,
			--		[SumSubConvertAmount] = case when @targeruserid <> @updateuserid and @updateuserid = userid then [SumSubConvertAmount] + abs(@convertamount) else [SumSubConvertAmount] end,
			--		[SumChargeAmount] = case when @targeruserid = @updateuserid then [SumChargeAmount] + @chargeamount else [SumChargeAmount] end,
			--		[SumConvertAmount] = case when @targeruserid = @updateuserid then [SumConvertAmount] + abs(@convertamount) else [SumConvertAmount] end
			--	where
			--		userid = @updateuserid

				--set @subminno = @subminno + 1
			--end
		end

		truncate table #Temp_chargeconvertuser
		set @minno = @minno + 1
	end

	update [dbo].[NetProfitAgent_UserInfo]
	set [SumSubChargeAmount] = a.charge,
		[SumSubConvertAmount] = a.converts
	from #temp_countchargeconvert as a
	where
		userid = a.temp_userid

	update [dbo].[NetProfitAgent_UserInfo]
	set [SumChargeAmount] = a.chargeamount,
		[SumConvertAmount] = a.convertamount
	from (select uid = userid,chargeamount,convertamount
			from [dbo].[NetProfitAgent_UserTaxLog] with (nolock)
			where logdate = @startdate
		) as a
	where
		userid = a.uid


	drop table #temp_tablea
	drop table #temp_tableb
	drop table #lowerusers
	drop table #everyuseramount
	drop table #Temp_chargeconvertuser
END

