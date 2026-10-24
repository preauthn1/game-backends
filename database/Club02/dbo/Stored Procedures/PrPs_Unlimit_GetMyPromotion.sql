
-- =============================================
-- Author:  Jason
-- Create date: 20190501
-- Description: 无极限-我的推广
-- =============================================
CREATE PROCEDURE [dbo].[PrPs_Unlimit_GetMyPromotion]
(
	@intUserID int
)
AS
BEGIN

	SET NOCOUNT ON
	--declare @intBossId int=100009--上级id
	--declare @intTeamMember int=100 --团队人数: 自己+直屬代理人數+代理招的人數(樹狀結構傘下的人數)
	--declare @intMyMember int=20 --直屬下線人數
	--declare @intTodayNewMember int=5 --今日新增的下線人數
	--declare @intTodayActiveMember int=33--今日有登入的下線人數
	--declare @chvQrURL varchar(200)='http://0.0.0.0/qr=ABCDEF' --我的推广二维码
	declare @intBossId int
	declare @intTeamMember int
	declare @intMyMember int
	declare @intTodayNewMember int
	declare @intTodayActiveMember int
	declare @chvQrURL varchar(200)
	declare @userstr varchar(max)
	declare @qrcode varchar(8)

	select	@intBossId = isnull(TopUserID,0),
			@userstr = [Relations]
	from [dbo].[Unlimit_UserInfo] with (nolock)
	where
		userid = @intUserID

	select	@qrcode = qrcode
	from [dbo].[UserAgentQRCode] with (nolock)
	where
		userid = @intUserID


	declare @nowtime datetime = dateadd(day,0,datediff(day,0,getdate()))
	declare @temp_users table (userid int,isnew int)
	insert into @temp_users
	select	userid,
			isnew = case when CreateTime >= @nowtime then 1 else 0 end
	from [dbo].[Unlimit_UserInfo] with (nolock)
	where
		[Relations] like '' + @userstr + '%'
		and userid <> @intUserID


	select	@intTodayNewMember = sum(isnew)
	from @temp_users

	set @intTodayNewMember = isnull(@intTodayNewMember,0)


	select	@intTodayActiveMember = count(distinct b.userid)
	from @temp_users as a
		inner join LogDB.[Users].[UserLogonLog] as b with (nolock) on b.[CreateTime] >= @nowtime and a.userid = b.userid

	set @intTodayActiveMember = isnull(@intTodayActiveMember,0)


	select	@intTeamMember = isnull([SubUser],0) + isnull([SubDirectUser],0),
			@intMyMember = isnull([SubDirectUser],0)
			--@intTodayNewMember = isnull(@nowtime,0),
			--@intTodayActiveMember = isnull([DAU],0)
	from [dbo].[Unlimit_Promotion] with (nolock)
	where
		userid = @intUserID

	declare @temp_userqrcode table (userid int,qrcode varchar(250),[InviteCode] varchar(8),UpperUserID int)

	insert into @temp_userqrcode
	exec [dbo].[PrPs_UserAgent_GetUserQRCode]
		@intUserID = @intUserID

	select	@chvQrURL = qrcode
	from @temp_userqrcode

	set @intBossId = isnull(@intBossId,0)
	set @intTeamMember = isnull(@intTeamMember,0)
	set @intMyMember = isnull(@intMyMember,0)
	set @intTodayNewMember = isnull(@intTodayNewMember,0)
	set @intTodayActiveMember = isnull(@intTodayActiveMember,0)
	set @chvQrURL = isnull(@chvQrURL,'')
	set @qrcode = isnull(@qrcode,'')

	
	/*  回傳資料  */
	select 
	@intBossId as BossId,
	@intTeamMember as TeamMember,
	@intMyMember as MyMember,
	@intTodayNewMember as TodayNewMember,
	@intTodayActiveMember as TodayActiveMember,
	@chvQrURL as QrURL,
	@qrcode as invitecode


	return 1
END
