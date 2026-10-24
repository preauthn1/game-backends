-- =============================================
-- Author:  jason
-- Create date: 2019/04/11
-- Description: USER 查詢今日收益
-- =============================================
CREATE PROCEDURE [dbo].[PrPs_UserAgent_GetAgentTaxBonusLog]
(
	@intUserID	int
)
AS
BEGIN

	set nocount on

	--declare @StatTime	datetime = dateadd(day,0,convert(varchar(10),@dtStattime,23))

	/*  取出三層用戶  */
	declare @temp_buser table (userid int)
	declare @temp_cuser table (buser int ,userid int)
	declare @temp_duser table (buser int, userid int)

	insert into @temp_buser
	select userid from [dbo].[UserAgentInfo] with (nolock) where TopUserID = @intUserID

	insert into @temp_cuser
	select	buser = b.userid,
			a.userid
	from [dbo].[UserAgentInfo] as a with (nolock)
		inner join @temp_buser as b on a.TopUserID = b.userid

	insert into @temp_duser
	select	buser = c.buser,
			a.userid
	from [dbo].[UserAgentInfo] as a with (nolock)
		inner join @temp_cuser as c on a.TopUserID = c.userid


	/*  計算B級稅收收益  */
	declare @bonus table (logdate datetime,userid int,bonus bigint)
	insert into @bonus
	select	b.[LogDate],
			a.userid,
			amount = sum(amount)
	from @temp_buser as a
		inner join [dbo].[UserAgentBonus] as b with (nolock) on a.userid = b.fromuserid
	where
		b.userid = @intUserID
	group by b.[LogDate],a.userid

	
	/*  CD稅收收益  */
	declare @temp_bcduser table (userid int,cduserid int)
	declare @temp_cdbonus table (logdate datetime,userid int,bonus bigint)

	insert into @temp_cdbonus
	select	b.[LogDate],
			a.buser,
			amount = sum(amount)
	from (
			select buser,userid from @temp_cuser
			union
			select buser,userid from @temp_duser
		) as a
		inner join [dbo].[UserAgentBonus] as b with (nolock) on a.userid = b.fromuserid
	where
		b.userid = @intUserID
	group by b.[LogDate],a.buser


	/*  暫存統計  */
	declare @temp_totaldata table (logdate datetime,B_TaxBonusAmount bigint,CD_TaxBonusAmount bigint,TotalTaxBonusAmount bigint)
	insert into @temp_totaldata
	select	distinct
			a.logdate,
			B_TaxBonusAmount = sum(case when a.logdate = e.logdate then isnull(e.bonus,0) else 0 end),
			CD_TaxBonusAmount = sum(case when a.logdate = f.logdate  then isnull(f.bonus,0) else 0 end),
			TotalTaxBonusAmount = sum(isnull(e.bonus,0)) + sum(isnull(f.bonus,0))
	from (
			select distinct logdate,userid from @bonus
			union
			select distinct logdate,userid from @temp_cdbonus
		) as a
		left join @bonus as e on a.logdate = e.logdate and a.userid = e.userid
		left join @temp_cdbonus as f on a.logdate = f.logdate and a.userid = f.userid
	group by a.logdate


	/*  回傳明細  */
	select	distinct
			[LogDate],
			B_TaxBonusAmount = isnull(B_TaxBonusAmount,0),
			CD_TaxBonusAmount = isnull(CD_TaxBonusAmount,0),
			[TotalTaxBonusAmount] = isnull(B_TaxBonusAmount,0) + isnull(CD_TaxBonusAmount,0)
	from @temp_totaldata
	order by [LogDate] desc

	return 1
END






