-- =============================================
-- Author:  jason
-- Create date: 2019/04/13
-- Description: USER 查询领取收益提现页面
-- =============================================
CREATE PROCEDURE [dbo].[PrWeb_UserAgent_GetWithdrawList]
(
	@intAdminID	int = null,
	@intUserID	int = null,
	@dtStartTime	datetime = null,
	@dtEndTime		datetime = null
)
AS
BEGIN

	set nocount on

	declare @logdate	datetime = dateadd(day,0,datediff(day,0,getdate()))

	set @dtStartTime = isnull(@dtStartTime,'1970-01-01')
	set @dtEndTime = isnull(@dtEndTime,'2100-01-01')
	set @dtStartTime = dateadd(day,0,datediff(day,0,@dtStartTime))
	set @dtEndTime = dateadd(day,0,datediff(day,0,@dtEndTime))


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
	end

	if(@intUserID is not null)
	begin
		insert into @Temp_PlatformID
		Select	B.PlatformID,
				C.Name,
				B.AreaID,
				B.AreaName
		From [dbo].[UserAgentInfo] as a
			inner join CenterDB.area.area as b with (nolock) on a.PlatformID = B.PlatformID
			inner join CenterDB.area.[platform] as c with (nolock) on b.PlatformID = c.PlatformID
		where
			a.userid = @intUserID
	end


	/*  计算总收益/已提现收益/今日可提现  */
	declare @temp_totalTaxAmount bigint
	declare @temp_convertAmount	bigint
	declare @temp_performedAmount bigint

	select	@temp_totalTaxAmount = sum([SumTaxBonus]),
			@temp_convertAmount = sum([SumTaxConvert]),
			@temp_performedAmount = sum([UnWithDrawTotal])
	from [dbo].[UserAgentInfo] with (nolock)
	--where
	--	[UserID] = @intUserID

	set @temp_totalTaxAmount	= isnull(@temp_totalTaxAmount,0)
	set @temp_convertAmount	= isnull(@temp_convertAmount,0)
	set @temp_performedAmount	= isnull(@temp_performedAmount,0)
	
	/*  回传总收益/以提现收益/今日可提现  */
	select	TotalTaxAmount = @temp_totalTaxAmount, --总收益
			ConvertAmount = @temp_convertAmount,	--以提现收益
			PerformedAmount = @temp_performedAmount	--今日可提现

	/*  提现纪录  */
	SELECT	[LogDate] = [LogDate],
			[WithdrawAmount],
			[Status] = case	when [Status] = 0 then '审核中'
							when [Status] = 1 then '已退还'
							when [Status] = 2 then '已提取'
						else '未处理' end
	FROM [dbo].[UserAgentWithDraw] WITH (NOLOCK)
	WHERE
		--[UserID] = @intUserID
		[LogDate] >= @dtStartTime
		and [LogDate] < @dtEndTime
	order by convert(varchar(10),[LogDate],23),[Status]

	return 1
END






