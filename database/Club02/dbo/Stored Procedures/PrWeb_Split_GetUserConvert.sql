
-- =============================================
-- Author:  jason
-- Create date: 20190506
-- Description: 裂变 - 提现纪录
-- =============================================
CREATE PROCEDURE [dbo].[PrWeb_Split_GetUserConvert]
(
	@intAdminID int,
	@intTargetID int = null,
	@dtStartTime		datetime = null,	--查询用户加入 开始时间
	@dtEndTime			datetime = null,	--查询用户加入 结束时间
	@intPageIndex	INT	= 1,
	@intPageSize	INT = 20,
	@intPageCount	INT = 0		OUTPUT
)
AS
BEGIN

	SET NOCOUNT ON
	

	set @dtStartTime = dateadd(day,0,isnull(@dtStartTime,'1970-01-01'))
	set @dtEndTime = dateadd(day,0,isnull(@dtEndTime,'2100-01-01'))
	

	/*  取得管理者可读取平台  */
	declare @adminplatformid table (platformid int,platformname varchar(32))

	insert into @adminplatformid
	select	distinct a.platformid,b.name
	from [dbo].[AgentPlatformRelations] as a with (nolock)
		inner join centerdb.area.platform as b with (nolock) on a.PlatformID = b.PlatformID
	where adminid = @intAdminID


	declare @temp_table table (UserID int,LogDate varchar(10),ConvertAmount bigint)


	insert into @temp_table
	select	UserID = b.userid,
			LogDate = convert(varchar(10),c.[CreateTime],23),
			ConvertAmount = [WithDrawAmount]
	from @adminplatformid as a
		inner join [dbo].[Split_UserInfo] as b with (nolock) on a.platformid = b.PlatformID
		inner join [dbo].[Split_WithDrawLog] as c with (nolock) on b.userid = c.userid
	where
		(@intTargetID is null or @intTargetID = b.userid)
		and c.[CreateTime] >= @dtStartTime
		and c.[CreateTime] < @dtEndTime

	select @intPageCount = count(1) from @temp_table
	set @intPageCount = isnull(@intPageCount,0)



	;WITH CTE
	AS (
			select	CAST(ROW_NUMBER() OVER (ORDER BY LogDate DESC) as int) AS rowNum,
					UserID,
					LogDate,
					ConvertAmount
			from @temp_table
			)
	SELECT * FROM CTE WHERE rowNum BETWEEN @intPageSize * (@intPageIndex -1) + 1 AND @intPageSize * @intPageIndex
	ORDER BY rowNum ASC

	return 1
END


