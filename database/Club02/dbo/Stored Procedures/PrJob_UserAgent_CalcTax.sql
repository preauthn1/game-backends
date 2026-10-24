-- =============================================
-- Author:  jason
-- Create date: 2019/04/11
-- Description: USER 計算稅收/收益
-- =============================================
CREATE PROCEDURE [dbo].[PrJob_UserAgent_CalcTax]	
AS
BEGIN

	set nocount on

	declare @LastHour	datetime = dateadd(hour,datepart(hour,dateadd(hour,-1,getdate())),dateadd(day,0,datediff(day,0,getdate())))
	declare @ThisHour	datetime = dateadd(hour,datepart(hour,dateadd(hour,0,getdate())),dateadd(day,0,datediff(day,0,getdate())))
	declare @LogDate	datetime = dateadd(day,0,datediff(day,0,getdate()))
	declare @CountHour int = datepart(hour,@LastHour)


	/*  先取出所有有對應的稅收紀錄  */
	declare @temp_agentuser	table (Num int,userid int)
	insert into @temp_agentuser
	select	row_number() over (order by userid asc) as Num,
			[UserID]
	from [dbo].[UserAgentInfo] with (nolock)

	/*  取稅收紀錄  */
	declare @temp_moneylog	table (Num int,userid int,[TaxAmount] bigint)
	insert into @temp_moneylog
	select	row_number() over (order by a.userid asc) as Num, 
			a.[UserID],
			[TaxAmount] = SUM([TaxAmount])
	from logdb.[Money].[MoneyLog] as a with (nolock)
		inner join @temp_agentuser as b on a.UserID = b.userid
	where
		[CreateTime] >= @LastHour and [CreateTime] < @ThisHour
		and [SourceTypeID] < 10000
	group by a.[UserID]

	insert into [dbo].[UserAgentTaxLog]
		([LogDate],[LogTime],[UserID],[TaxAmount],[Status])
	select	@LogDate,@CountHour,userid,[TaxAmount],1
	from @temp_moneylog

	declare @temp_Usertaxlog	table (Num int,ID bigint,UserID int,TaxAmount bigint)

	insert into @temp_Usertaxlog
	select	row_number() over (order by ID DESC) AS Num,
			ID,
			UserID,
			[TaxAmount]
	from [dbo].[UserAgentTaxLog] with (nolock)
	where
		[LogDate] = @LogDate
		and [LogTime] = @CountHour


	declare @minno int
	declare @maxno int
		
	select	@minno = min(Num),
			@maxno = max(Num)
	from @temp_Usertaxlog


	--create table #T (temp_userid int,givebonus bigint)
			
	while(@minno <= @maxno)
	begin
		declare @relationid bigint = null
		declare @sourceuser int = null
		declare @countuser1 int = null
		declare @countuser2 int = null
		declare @countuser3 int = null
		declare @sourceamount bigint = null
		declare @rateuser1 tinyint = null
		declare @rateuser2 tinyint = null
		declare @rateuser3 tinyint = null
		declare @givebonus bigint = null
		declare @userlevel tinyint = null

		select	@sourceuser = userid,
				@relationid = ID,
				@sourceamount = [TaxAmount]
		from @temp_Usertaxlog
		where
			Num = @minno

		--找出3層ID
		select	@countuser1 = TopUserID				
		from [dbo].[UserAgentInfo] as a with (nolock)
		where
			a.userid = @sourceuser

		select	@rateuser1 = Stage1
		from [dbo].[UserAgentInfo] as a with (nolock)
			inner join [dbo].[UserAgentUserRate] as b with (nolock) on a.userid = b.userid and a.UserLevel = b.UserLevel
		where
			a.userid = @countuser1

		select	@countuser2 = TopUserID
		from [dbo].[UserAgentInfo] as a with (nolock)
		where
			a.userid = @countuser1

		select	@rateuser2 = Stage2
		from [dbo].[UserAgentInfo] as a with (nolock)
			inner join [dbo].[UserAgentUserRate] as b with (nolock) on a.userid = b.userid and a.UserLevel = b.UserLevel
		where
			a.userid = @countuser2

		select	@countuser3 = TopUserID
		from [dbo].[UserAgentInfo] as a with (nolock)
		where
			a.userid = @countuser2

		select	@rateuser3 = Stage3
		from [dbo].[UserAgentInfo] as a with (nolock)
			inner join [dbo].[UserAgentUserRate] as b with (nolock) on a.userid = b.userid and a.UserLevel = b.UserLevel
		where
			a.userid = @countuser3

		if(@countuser1 is not null)
		begin
			select @userlevel = UserLevel from [dbo].[UserAgentInfo] with (nolock) where userid = @countuser1
			set @givebonus = @sourceamount * @rateuser1 / 1000

			insert into [dbo].[UserAgentBonus]
				([LogDate],[LogHour],[UserID],[UserLevel],[Rate],[Amount],[RelationID],[FromuserID])
			values
				(@LogDate,@CountHour,@countuser1,@userlevel,@rateuser1,@givebonus,@relationid,@sourceuser)

			--insert into #T values (@countuser1,@givebonus)
		end

		if(@countuser2 is not null)
		begin
			select @userlevel = UserLevel from [dbo].[UserAgentInfo] with (nolock) where userid = @countuser2
			set @givebonus = @sourceamount * @rateuser2 / 1000

			insert into [dbo].[UserAgentBonus]
				([LogDate],[LogHour],[UserID],[UserLevel],[Rate],[Amount],[RelationID],[FromuserID])
			values
				(@LogDate,@CountHour,@countuser2,@userlevel,@rateuser2,@givebonus,@relationid,@sourceuser)

			--insert into #T values (@countuser1,@givebonus)

			--update [dbo].[UserAgentInfo]
			--set [SumTaxBonus] = [SumTaxBonus] + @givebonus,
			--	[UserLevel] = [dbo].[Fn_UserAgent_GetUserLevel]([SumTaxBonus] + @givebonus)
			--where
			--	userid = @countuser2
		end

		if(@countuser3 is not null)
		begin
			select @userlevel = UserLevel from [dbo].[UserAgentInfo] with (nolock) where userid = @countuser3
			set @givebonus = @sourceamount * @rateuser3 / 1000

			insert into [dbo].[UserAgentBonus]
				([LogDate],[LogHour],[UserID],[UserLevel],[Rate],[Amount],[RelationID],[FromuserID])
			values
				(@LogDate,@CountHour,@countuser3,@userlevel,@rateuser3,@givebonus,@relationid,@sourceuser)

			--insert into #T values (@countuser1,@givebonus)
		end

		update [dbo].[UserAgentInfo]
		set [SumTaxTotal] = [SumTaxTotal] + @sourceamount
		where
			userid = @sourceuser
		
		set @minno = @minno + 1
	end


	/*  取出當次計算總額做加總及升級判斷  */
	declare @temp_bonuscount table (temp_userid int,totalbonus bigint)

	insert into @temp_bonuscount
	select	userid,
			--bonusamount = sum(case when [LogDate] = @LogDate and [LogHour] = @CountHour then [Amount] else 0 end),	-- 當次計算獎勵
			totalbonus = sum([Amount])	
	from [dbo].[UserAgentBonus] with (nolock)
	group by userid


	/*  計算升級  */
	update [dbo].[UserAgentInfo]
	set [SumTaxBonus] = totalbonus,
		[UserLevel] = [dbo].[Fn_UserAgent_GetUserLevel](totalbonus)
	from (
			select temp_userid,totalbonus from @temp_bonuscount
			) as a
	where
		userid = a.temp_userid

	--drop table #T
END
GO


