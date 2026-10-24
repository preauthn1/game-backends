
-- =============================================
-- Author:  jason
-- Create date: 20190506
-- Description: 裂变 - 用户列表
--	2019/06/04	Jason		新增BCD層級欄位
-- =============================================
CREATE PROCEDURE [dbo].[PrWeb_Split_GetUserInfo]
(
	@intAdminID int,
	@intTargetID int = null,
	@dtStartTime		datetime = null,	--查询用户加入 开始时间
	@dtEndTime			datetime = null,	--查询用户加入 结束时间
	@chvMsg			varchar(250)	OUTPUT
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
		
	select	b.userid,
			b.NickName,
			b.platformid,
			a.platformname,
			b.StageLevel,
			b.status,
			topuserid,
			[UpperUserID],
			b.CreateTime,
			B_User = isnull(c.SuccessCountB,0),
			C_User = isnull(c.SuccessCountC,0),
			D_User = isnull(c.SuccessCountD,0)
	from @adminplatformid as a
		inner join [dbo].[Split_UserInfo] as b with (nolock) on a.platformid = b.PlatformID
		left join [dbo].[Split_RecruitSuccess] as c with (nolock) on b.userid = c.userid
	where
		(@intTargetID is null or @intTargetID = b.UserID)
		and b.CreateTime >= @dtStartTime
		and b.CreateTime < @dtEndTime



	return 1
END


