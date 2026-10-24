-- =============================================
-- Author:  jason
-- Create date: 2019/04/15
-- Description: 全民代理后台 玩家/代理信息(淨利潤)
-- =============================================
CREATE PROCEDURE [dbo].[PrWeb_NetProfitAgent_GetAgentInfo]
(
	@intAdminID		int = null,
	@intUserID		int = null,			--用户 ID
	@dtStartTime	datetime = null,	--查詢註冊起始日期
	@dtEndTime		datetime = null,	--查詢註冊結束日期
	@chvMsg			varchar(100)		OUTPUT
)
AS
BEGIN

	set nocount on

	declare @StartDate datetime = convert(varchar(10),isnull(@dtStartTime,'1970-01-01'),23)
	declare @EndDate datetime = dateadd(day,1,convert(varchar(10),isnull(@dtEndTime,'2100-01-01'),23))


	if(@intAdminID is null and @intUserID is null)
	begin
		set @chvMsg = '操作錯誤 : 用戶不存在'
		return -1
	end


	--平台暂存
	DECLARE @Temp_PlatformID TABLE (PlatformID	INT,PlatformName varchar(50),AreaID int,AreaName varchar(50))


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


		/*  取出所有用戶  */
		declare @temp_alltopuser table (rowno int,userid int,platformid int,areaid int)

		insert into @temp_alltopuser
		select	row_number() over (order by userid asc) as rowno,
				userid,
				platformid,
				areaid
		from [dbo].[NetProfitAgent_UserInfo] with (nolock)
		where
			topuserid is null and status = 2

		select	b.userid,
				b.CreateTime,
				chargeamount = isnull([SumChargeAmount],0) + isnull([SumSubChargeAmount],0),
				WithDrawAmount = isnull([SumWithDrawAmount],0) + isnull([SumSubWithDrawAmount],0),
				[Status] = case when b.status < 2 then '否' else '是' end,
				[TotalUsers] = isnull([SumSubUser],0) + isnull([SumSubAgent],0),
				[SumSubUsers] = isnull([SumSubUser],0),
				[SumSubAgent] = isnull([SumSubAgent],0),
				[SumSubWithDrawAmount] = isnull([SumWithDrawAmount],0)
		from @Temp_PlatformID as a
			inner join [dbo].[NetProfitAgent_UserInfo] as b with (nolock) on a.platformid = b.PlatformID and a.AreaID = b.AreaID
		where
			b.CreateTime >= @StartDate
			and b.CreateTime < @EndDate


		set @chvMsg = '操作完成 : 平台数据查询完成'
		return 1
	end

	if(@intUserID is not null)
	begin

		select	b.userid,
				b.CreateTime,
				chargeamount = isnull([SumChargeAmount],0) + isnull([SumSubChargeAmount],0),
				WithDrawAmount = isnull([SumWithDrawAmount],0) + isnull([SumSubWithDrawAmount],0),
				[Status] = case when b.status < 2 then '否' else '是' end,
				[TotalUsers] = isnull([SumSubUser],0) + isnull([SumSubAgent],0),
				[SumSubUsers] = isnull([SumSubUser],0),
				[SumSubAgent] = isnull([SumSubAgent],0),
				[SumSubWithDrawAmount] = isnull([SumWithDrawAmount],0)
		from [dbo].[NetProfitAgent_UserInfo] as b with (nolock)
		where
			b.userid = @intUserID
			and b.CreateTime >= @StartDate
			and b.CreateTime < @EndDate
		union all
		select	b.userid,
				b.CreateTime,
				chargeamount = isnull([SumChargeAmount],0) + isnull([SumSubChargeAmount],0),
				WithDrawAmount = isnull([SumWithDrawAmount],0) + isnull([SumSubWithDrawAmount],0),
				[Status] = case when b.status < 2 then '否' else '是' end,
				[TotalUsers] = isnull([SumSubUser],0) + isnull([SumSubAgent],0),
				[SumSubUsers] = isnull([SumSubUser],0),
				[SumSubAgent] = isnull([SumSubAgent],0),
				[SumSubWithDrawAmount] = isnull([SumWithDrawAmount],0)
		from [dbo].[NetProfitAgent_UserInfo] as b with (nolock)
		where
			b.topuserid = @intUserID
			and b.CreateTime >= @StartDate
			and b.CreateTime < @EndDate


		set @chvMsg = '操作完成 : 用户个人数据查询完成'
		return 1
	end

	set @chvMsg = '操作完成 : 查询完成'
	return 1
END





