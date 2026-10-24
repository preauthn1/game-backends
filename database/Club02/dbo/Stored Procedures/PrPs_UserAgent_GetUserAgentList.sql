-- =============================================
-- Author:  jason
-- Create date: 2019/04/11
-- Description: USER 推广人数页面说明
-- =============================================
CREATE PROCEDURE [dbo].[PrPs_UserAgent_GetUserAgentList]
(
	@intUserID	int
)
AS
BEGIN

	set nocount on

	declare @today datetime = dateadd(day,0,datediff(day,0,getdate()))

	/*  取出三层用户  */
	declare @temp_busers table (userid int)
	declare @temp_cusers table (userid int)
	declare @temp_dusers table (userid int)

	insert into @temp_busers
	select userid from [dbo].[UserAgentInfo] with (nolock) where TopUserID = @intUserID

	insert into @temp_cusers
	select userid from [dbo].[UserAgentInfo] with (nolock) where TopUserID in (select userid from @temp_busers)
	
	insert into @temp_dusers
	select userid from [dbo].[UserAgentInfo] with (nolock) where TopUserID in (select userid from @temp_cusers)
	
	/*  取得团队总人数  */
	declare @totaluser int
	
	select	@totaluser = count(userid)
	from (
			select userid from @temp_busers
			union
			select userid from @temp_cusers
			union
			select userid from @temp_dusers
		) as a


	/*  取得直属下级  */
	declare @temp_buser int
	select	@temp_buser = count([UserID])
	from @temp_busers


	/*  今日新增人数  */
	declare @todaynewuser int
	select	@todaynewuser = count(distinct b.userid)
	from (
			select userid from @temp_busers
			union
			select userid from @temp_cusers
			union
			select userid from @temp_dusers
		) as a
		inner join [dbo].[UserAgentInfo] as b with (nolock) on a.userid = b.userid
	where
		createtime >= @today

	/*  今日登入人数  */
	declare @todaylogin int
	select	@todaylogin = count(userid)
	from centerdb.[Users].[UserInfo] with (nolock)
	where
		userid in (
					select userid from @temp_busers
					union
					select userid from @temp_cusers
					union
					select userid from @temp_dusers
					)
		and [LastLogonTime] >= @today


	set @totaluser	= isnull(@totaluser,0)
	set @temp_buser	= isnull(@temp_buser,0)
	set @todaynewuser	= isnull(@todaynewuser,0)
	set @todaylogin	= isnull(@todaylogin,0)

	/*  回传资料  */
	select	totaluser = @totaluser,			--团队人数
			temp_buser = @temp_buser,		--直属下级
			todaynewuser = @todaynewuser,	--今日新增人数
			todaylogin = @todaylogin		--今日登录人数

	return 1
END






