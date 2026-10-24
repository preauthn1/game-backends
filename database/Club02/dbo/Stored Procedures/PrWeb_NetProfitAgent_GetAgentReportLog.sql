-- =============================================
-- Author:  jason
-- Create date: 2019/04/18
-- Description: 全民代理后台 新增代理(净利润)
-- =============================================
CREATE PROCEDURE [dbo].[PrWeb_NetProfitAgent_GetAgentReportLog]
(
	@intAdminID		int = null,
	@intUserID		int = null,		--用户 ID
	@dtStartTime	datetime = null,
	@dtEndTime		datetime = null,
	@chvMsg			varchar(100)		OUTPUT
)
AS
BEGIN

	set nocount on

	declare @StartDate datetime = convert(varchar(10),isnull(@dtStartTime,convert(varchar(10),getdate(),23)),23)
	declare @EndDate datetime = dateadd(day,1,convert(varchar(10),isnull(@dtEndTime,'2100-01-01'),23))


	if(@intAdminID is null and @intUserID is null)
	begin
		set @chvMsg = '操作错误 : 用户不存在'
		return -1
	end


	declare @temp_agentuser	table (userid int,CreateTime datetime,[SumSubUsers] int,[SumChargeAmount] bigint,[WithdrawAmount] bigint)


	--平台暂存
	DECLARE @Temp_PlatformID TABLE (PlatformID	INT,PlatformName varchar(50),AreaID int,AreaName varchar(50))
	/*  取出代理身份  */
	declare @temp_totalagentuser table (userid int,CreateTime datetime,[SumSubUsers] int,[SumChargeAmount] bigint,[WithdrawAmount] bigint,[WithdrawCount] int,QRURL varchar(250))

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


		declare @temp_totalagent int

		insert into @temp_totalagentuser
		select	a.userid,
				CreateTime,
				[SumSubUsers],
				[SumChargeAmount],
				[WithdrawAmount],
				[WithdrawCount] = isnull(withdrawcount,0),
				QRURL = isnull(QRURL,'')
		from (
				select	userid,
						CreateTime,
						[SumSubUsers] = isnull([SumSubUser],0) + isnull([SumSubAgent],0),
						[SumChargeAmount] = isnull([SumChargeAmount],0) + isnull([SumChargeAmount],0),
						[WithdrawAmount] = isnull([SumWithDrawAmount],0) + isnull([SumSubWithDrawAmount],0)
				from @Temp_PlatformID as a
					inner join [dbo].[NetProfitAgent_UserInfo] as b with (nolock) on a.platformid = b.platformid and a.areaid = b.areaid
				where
					topuserid is null and status = 2
					and AgentCreateTime >= @StartDate
					and AgentCreateTime < @EndDate
				union
				select	userid,
						CreateTime,
						[SumSubUsers] = isnull([SumSubUser],0) + isnull([SumSubAgent],0),
						[SumChargeAmount] = isnull([SumChargeAmount],0) + isnull([SumChargeAmount],0),
						[WithdrawAmount] = isnull([SumWithDrawAmount],0) + isnull([SumSubWithDrawAmount],0)
				from @Temp_PlatformID as a
					inner join [dbo].[NetProfitAgent_UserInfo] as b with (nolock) on a.platformid = b.platformid and a.areaid = b.areaid
				where
					topuserid is not null and status = 2
					and AgentCreateTime >= @StartDate
					and AgentCreateTime < @EndDate
			) as a
			left join (
						select userid,withdrawcount = count(1)
						from [dbo].[NetProfitAgent_WithDrawLog] with (nolock)
						group by userid
					) as b on a.userid = b.userid
			left join (
						select userid,QRURL = b.Domain + '?a=' + a.QRCode
						from [dbo].[UserAgentQRCode] as a with (nolock)
							inner join [dbo].[DomainSetting] as b with (nolock) on a.domainid = b.id
					) as c on a.userid = c.userid

		select	@temp_totalagent = count(1)
		from @temp_totalagentuser

		select *
		from @temp_totalagentuser


		set @chvMsg = '操作完成 : 平台数据查询完成'
		return 1
	end


	if(@intUserID is not null)
	begin

		insert into @temp_agentuser
		select	userid,
				CreateTime = [AgentCreateTime],
				[SumSubUser],
				[SumChargeAmount],
				[WithdrawAmount] = [SumWithDrawAmount] + [SumSubWithDrawAmount]
		from [dbo].[NetProfitAgent_UserInfo] with (nolock)
		where
			TopUserID = @intUserID
			and [AgentCreateTime] >= @StartDate
			and [AgentCreateTime] < @EndDate


		/*  取出下线玩家  */
		declare @temp_subuser table (userid int,subuserid int)

		insert into @temp_subuser
		select	a.userid,
				subuserid = b.userid
		from @temp_agentuser as a
			inner join [dbo].[NetProfitAgent_UserInfo] as b with (nolock) on a.userid = b.TopUserID
		union all
		select	userid,
				subuserid = userid
		from @temp_agentuser


		/*  取出提领纪录  */
		declare @temp_withdraw table (userid int,counts int)

		insert into @temp_withdraw
		select	a.userid,
				counts = count(b.userid)
		from @temp_subuser as a
			inner join [dbo].[NetProfitAgent_WithDrawLog] as b on a.subuserid = b.userid
		group by a.userid

		/*  取出充值金额  */
		declare @temp_charge table (userid int,amount bigint)
		insert into @temp_charge
		select	a.userid,
				amount = sum(ChargeAmount)
		from @temp_subuser as a
			inner join [dbo].[NetProfitAgent_UserTaxLog] as b with (nolock) on a.userid = b.userid
		group by a.userid


		/*  取昨日新增多少人  */
		declare @totaluserlast int
		select	@totaluserlast = count(1)
		from [dbo].[NetProfitAgent_UserInfo] with (nolock)
		where
			topuserid = @intUserID
			and status = 2
			and CreateTime >= @StartDate
			and CreateTime < @EndDate

		set @totaluserlast = isnull(@totaluserlast,0)

		/*  代理总人数  */
		declare @agenttotal int
		select	@agenttotal = count(1)
		from [dbo].[NetProfitAgent_UserInfo] with (nolock)
		where
			status = 2 and topuserid = @intUserID

		set @agenttotal = isnull(@agenttotal,0)

		/*  回传资料  */
		--select	[totallastdate] = @totaluserlast,
		--		[totalagentuser] = @agenttotal

		select	a.userid,
				CreateTime,
				[SumSubUsers],
				[SumChargeAmount] = isnull(amount,0),
				[WithdrawAmount] = isnull([WithdrawAmount],0),
				[WithdrawCount] = isnull(b.counts,0),
				QRURL = isnull(QRURL,'')
		from @temp_agentuser as a
			left join @temp_withdraw as b on a.userid = b.userid
			left join @temp_charge as c on a.userid = c.userid
			left join (
						select userid,QRURL = b.Domain + '?a=' + a.QRCode
						from [dbo].[UserAgentQRCode] as a with (nolock)
							inner join [dbo].[DomainSetting] as b with (nolock) on a.domainid = b.id
					) as d on a.userid = d.userid


		set @chvMsg = '操作完成 : 用户个人数据查询完成'
		return 1
	end

	set @chvMsg = '操作完成 : 查询完成'
	return 1
END





