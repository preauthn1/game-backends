-- =============================================
-- Author:  jason
-- Create date: 2019/04/18
-- Description: 全民代理后台 代理提现纪录(净利润)
-- =============================================
CREATE PROCEDURE [dbo].[PrWeb_NetProfitAgent_GetAgentWithdrawLog]
(
	@intAdminID		int = null,			--管理者
	@intUserID		int = null,			--用户 ID
	@tnyStatus		tinyint = null,		--0:审核中 1:已完成 2:拒绝兑换
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

	--平台暂存
	DECLARE @Temp_PlatformID TABLE (PlatformID	INT,PlatformName varchar(50),AreaID int,AreaName varchar(50))
	declare @temp_agentwithdrawslog	table (num int,userid int,createtime datetime,[WithDrawAmount] bigint,[Status] tinyint,Memo varchar(100))

	declare @totalamount bigint = 0

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


		insert into @temp_agentwithdrawslog
		select	row_number() over (order by c.createtime desc) as num,
				b.userid,
				c.[CreateTime],
				[WithDrawAmount],
				c.[Status],
				c.[Memo]
		from @Temp_PlatformID as a
			inner join [dbo].[NetProfitAgent_UserInfo] as b with (nolock) on a.PlatformID = b.PlatformID and a.areaid = b.areaid
			inner join [dbo].[NetProfitAgent_WithDrawLog] as c with (nolock) on b.userid = c.userid
		where
			(b.userid = @intUserID or @intUserID is null)
			and b.status = 2
			and C.CreateTime >= @StartDate
			and C.CreateTime < @EndDate
			and (@tnyStatus = C.Status or @tnyStatus is null)

	
		select	@totalamount = sum([WithDrawAmount])
		from @temp_agentwithdrawslog

		set @totalamount = isnull(@totalamount,0)

		select	totalamount = @totalamount

		select	[UserID] = [UserID],
				[CreateTime] = convert(varchar(19),[CreateTime],121),
				[WithDrawAmount] = [WithDrawAmount],
				[Status] = case when [Status] = 0 then '审核中'
								when [Status] = 1 then '已提领'
								when [Status] = 2 then '提领失败退还'
							end
		from @temp_agentwithdrawslog

		set @chvMsg = '操作完成 : 平台数据查询完成'
		return 1
	end


	if(@intUserID is not null)
	begin
		insert into @temp_agentwithdrawslog
		select	row_number() over (order by b.createtime desc) as num,
				a.userid,
				b.[CreateTime],
				b.[WithDrawAmount],
				b.[Status],
				b.[Memo]
		from [dbo].[NetProfitAgent_UserInfo] as a with (nolock)
			inner join [dbo].[NetProfitAgent_WithDrawLog] as b with (nolock) on a.userid = b.userid
		where
			a.TopUserID = @intUserID
			and a.Status = 2
			and b.CreateTime >= @StartDate
			and b.CreateTime < @EndDate
			and (@tnyStatus = b.Status or @tnyStatus is null)

	
		select	@totalamount = sum([WithDrawAmount])
		from @temp_agentwithdrawslog

		set @totalamount = isnull(@totalamount,0)

		select	totalamount = @totalamount

		select	[UserID],
				[CreateTime],
				[WithDrawAmount],
				[Status],
				[Memo]
		from (
				select	num,
						userid,
						[CreateTime] = convert(varchar(19),[CreateTime],121),
						[WithDrawAmount] = [WithDrawAmount],
						[Status] = case when [Status] = 0 then '审核中'
										when [Status] = 1 then '已提领'
										when [Status] = 2 then '提领失败退还'
									end,
						[Memo]
				from @temp_agentwithdrawslog
			) as a

		set @chvMsg = '操作完成 : 用户个人数据查询完成'
		return 1
	end

	set @totalamount = isnull(@totalamount,0)
	select	@totalamount
	set @chvMsg = '操作完成 : 查询完成'
	return 1
END







