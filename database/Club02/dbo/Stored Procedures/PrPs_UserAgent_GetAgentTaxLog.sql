-- =============================================
-- Author:  jason
-- Create date: 2019/04/11
-- Description: USER 查询今日税收
-- =============================================
CREATE PROCEDURE [dbo].[PrPs_UserAgent_GetAgentTaxLog]
(
	@intUserID	int,
	@dtStattime	datetime
)
AS
BEGIN

	set nocount on

	declare @StatTime	datetime = dateadd(day,0,convert(varchar(10),@dtStattime,23))


	/*  取出三层用户  */
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


	/*  计算B层总人数  */
	declare @temp_bpeople int
	select	@temp_bpeople = count(userid)
	from @temp_buser

	/*  计算B层级产生税收  */
	declare @temp_btaxamount table (userid int,[B_taxAmount] bigint,B_bonusamount bigint)

	insert into @temp_btaxamount
	select	a.userid,
			[B_taxAmount] = sum([TaxAmount]),0
	from @temp_buser as a
		inner join [dbo].[UserAgentTaxLog] as b with (nolock) on a.userid = b.userid
	where
		b.[LogDate] = @StatTime
	group by a.userid


	/*  计算B级税收收益  */
	declare @bonus table (userid int,bonus bigint)
	insert into @bonus
	select	a.userid,
			amount = sum(amount)
	from @temp_buser as a
		inner join [dbo].[UserAgentBonus] as b with (nolock) on a.userid = b.fromuserid
	where
		b.userid = @intUserID

		and b.[LogDate] = @StatTime
	group by a.userid
	

	/*  计算C D级产生税收  */
	declare @temp_cdtaxamount table (buser int,[CD_taxAmount] bigint)

	insert into @temp_cdtaxamount
	select	buser,
			CD_taxAmount = sum([TaxAmount])
	from (
			select buser,userid from @temp_cuser
			union
			select buser,userid from @temp_duser
		) as a
		inner join [dbo].[UserAgentTaxLog] as b with (nolock) on a.userid = b.userid
	where
		b.[LogDate] = @StatTime
	group by buser


	/*  CD税收收益  */
	declare @temp_bcduser table (userid int,cduserid int)
	declare @temp_cdbonus table (userid int,bonus bigint)

	insert into @temp_cdbonus
	select	a.buser,
			amount = sum(amount)
	from (
			select buser,userid from @temp_cuser
			union
			select buser,userid from @temp_duser
		) as a
		inner join [dbo].[UserAgentBonus] as b with (nolock) on a.userid = b.fromuserid
	where
		b.userid in (	select userid from @temp_buser
						union
						select userid from @temp_cuser
						union
						select @intUserID)
		and b.[LogDate] = @StatTime
	group by a.buser


	/*  暂存统计  */
	declare @temp_totaldata table (userid int,nickname varchar(32),B_taxAmount bigint,CD_taxAmount bigint,[TotalTaxAmount] bigint)
	insert into @temp_totaldata
	select	userid,
			nickname,
			B_taxAmount,
			CD_taxAmount,
			[TotalTaxAmount] = [TotalTaxAmount]
	from (
			select	distinct
					a.userid,
					nickname = centerdb.[dbo].[Fn_GetUserNickName](d.nickname),
					B_taxAmount = sum(case when a.userid = b.userid then isnull(b.B_taxAmount,0) else 0 end),
					CD_taxAmount = sum(case when a.userid = c.buser then isnull(c.CD_taxAmount,0) else 0 end),
					[TotalTaxAmount] = sum(isnull(e.bonus,0)) + sum(isnull(f.bonus,0))
			from (
					select distinct userid from @temp_btaxamount
				) as a
				left join @temp_btaxamount as b on a.userid = b.userid
				left join @temp_cdtaxamount as c on a.userid = c.buser
				left join @bonus as e on a.userid = e.userid
				left join @temp_cdbonus as f on a.userid = f.userid
				left join [dbo].[UserAgentInfo] as d with (nolock) on a.userid = d.userid
			group by a.userid,d.nickname
		) as a


	/*  计算税收总额  */
	declare @temp_totalTaxAmount bigint

	select	@temp_totalTaxAmount = sum(isnull(B_taxAmount,0) + isnull(CD_taxAmount,0))
	from @temp_totaldata

	set @temp_totalTaxAmount = isnull(@temp_totalTaxAmount,0)
	
	/*  计算税收总奖励  */
	declare @temp_totalTaxBonus bigint

	select	@temp_totalTaxBonus = sum([TotalTaxAmount])
	from @temp_totaldata

	set @temp_totalTaxBonus = isnull(@temp_totalTaxBonus,0)

	/*  回传 税收总额/税收总奖励/总人数  */
	select	totalTaxAmount = @temp_totalTaxAmount,
			totalTaxBonus = @temp_totalTaxBonus,
			temp_bpeople = @temp_bpeople


	/*  回传资料  */
	select	userid,
			nickname,
			B_taxAmount,
			CD_taxAmount,
			[TotalTaxAmount] = [TotalTaxAmount]
	from @temp_totaldata







	return 1
END






