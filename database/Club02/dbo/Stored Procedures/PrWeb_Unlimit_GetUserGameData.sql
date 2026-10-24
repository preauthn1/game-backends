-- =============================================
-- Author:  jason
-- Create date: 2019/05/07
-- Description: 無限級后台 取得用户基本信息
-- =============================================
CREATE PROCEDURE [dbo].[PrWeb_Unlimit_GetUserGameData]
(
	@intAdminID int = null,
	@intUserID	int = null,
	@intTargetID	int,
	@chvMsg		varchar(100) OUTPUT
)
AS
BEGIN

	set nocount on
	
	declare	@intPageCount bigint
	declare @Topuserrate int
	declare @topstagelevel int
	declare @targetagentapplystatus int
	


	declare @userplatformid int

	select @userplatformid = b.platformid
	from centerdb.users.userinfo as a
		inner join centerdb.area.area as b on a.areaid = b.areaid
	where
		a.userid = @intTargetID

	
	if(@intAdminID is null and @intUserID is null)
	begin
		set @chvMsg = '操作错误 : 用户不存在'
		return -1
	end


	/*  取资料暂存  */
	declare @temp_userinfo table ([ID] int,[UserID] int,[LoginName] varchar(32),[Password] varchar(32),[BankPassword] varchar(32),[FaceID] int,[NickName] varchar(32),
									[Sex] tinyint,[HDType] tinyint,[HDCode] varchar(64),[UserType] tinyint,[CreateTime] datetime,[CreateIPAddress] varchar(15),
									[LastLogonTime] datetime,[LastLogonIPAddress] varchar(15),[LastLogonHDType] tinyint,[LastLogonHDCode] varchar(64),
									[RealName] varchar(64) NULL,[AlipayAccount] varchar(64),[UnionpayBank] varchar(5),[UnionpayAccount] varchar(64),
									[UnionpayRealName] varchar(64),[SumCharge] bigint,[SumConvert] bigint,[SumConvertTimes] int,[SumTax] bigint,[SumLoginTimes] int,
									[QuestionCount] int,[GradeID] int,[AreaID] int,[SumAgentCharge] bigint,[AdPlan] int,[AdPlanTime] datetime,[ExperienceCash] bigint,
									[SumConvertTax] bigint,[SumFreeMoney] bigint,[SumClubMoney] bigint,[SumAdminMoney] bigint,SumFlowAmount bigint,[SumCouponAmount] bigint, [AvatarFrame] int,
									cashamount bigint,platformname varchar(100),areaname varchar(100),block int,rowid int
								)
								
	insert into @temp_userinfo
	EXEC CenterDB.[dbo].[PrWeb_User_GetUserInfo]
		@intUserID = @intTargetID,
		@intPageCount = @intPageCount OUTPUT


	DECLARE @Temp_PlatformID TABLE (PlatformID	INT,PlatformName varchar(50),AreaID int,AreaName varchar(50))
	

	if(@intAdminID is not null)
	begin
		--捞出该用户可读取的全部平台
		INSERT INTO @Temp_PlatformID
		Select	B.PlatformID,
				C.Name,
				B.AreaID,
				B.AreaName
		From dbo.AgentPlatformRelations as a
			inner join CenterDB.area.area as b with (nolock) on a.PlatformID = B.PlatformID
			inner join CenterDB.area.[platform] as c with (nolock) on b.PlatformID = c.PlatformID
		where
			a.AdminID = @intAdminID
	end


	if(@intUserID is not null)
	begin
	
		declare @userrelations varchar(max)
		select	@userrelations = [Relations]
		from [dbo].[Unlimit_UserInfo] with (nolock)
		where userid = @intUserID
		
		--捞出该用户可读取的全部平台
		INSERT INTO @Temp_PlatformID
		select	distinct B.PlatformID,
						C.Name,
						B.AreaID,
						B.AreaName
		from [dbo].[Unlimit_UserInfo] as a with (nolock)
			inner join CenterDB.area.area as b with (nolock) on a.PlatformID = B.PlatformID
			inner join CenterDB.area.[platform] as c with (nolock) on b.PlatformID = c.PlatformID
		where
			[Relations] like '' + @userrelations + '%'

	end	


	if exists (select top 1 1 from @Temp_PlatformID)	
	begin
		select	[ID],a.[UserID],[LoginName],a.[Password],[BankPassword],[FaceID],a.[NickName],
				[Sex],[HDType],[HDCode],[UserType],a.[CreateTime],[CreateIPAddress],
				[LastLogonTime],[LastLogonIPAddress],[LastLogonHDType],[LastLogonHDCode],
				[RealName],[AlipayAccount],[UnionpayBank],[UnionpayAccount],
				[UnionpayRealName],[SumCharge],[SumConvert],[SumConvertTimes],[SumTax],[SumLoginTimes],
				[QuestionCount],[GradeID],a.[AreaID],[SumAgentCharge],[AdPlan],[AdPlanTime] = isnull([AdPlanTime],0),[ExperienceCash],
				[SumConvertTax],[SumFreeMoney],[SumClubMoney],[SumAdminMoney],cashamount,
				PlatformName,AreaName,rate = isnull(b.rate,0),userstatus = cast(Permission as int)
		from @temp_userinfo as a
			inner join [dbo].[Unlimit_UserInfo] as b with (nolock) on a.userid = b.userid

		SELECT [UserID],a.[GameID],b.GameName,[WinCount],[LoseCount],[DrawCount],[Score],[GameTime],[LastWinLose]
		FROM [CenterDB].[Game].[UserGameInfo] as a with (nolock)
			inner join CenterDB.[ServerInfo].[GameList] as b with (nolock) on a.gameid = b.GameID
		where
			userid = @intTargetID
	end
	else
	begin
		select top 0 *,0 as rate,0 as topuserrate,0 as stagelevel,0 as modifystatus,0 as userstatus,0 as agentapplystatus
		from @temp_userinfo
		
		SELECT top 0 [UserID],a.[GameID],b.GameName,[WinCount],[LoseCount],[DrawCount],[Score],[GameTime],[LastWinLose]
		FROM [CenterDB].[Game].[UserGameInfo] as a with (nolock)
			inner join CenterDB.[ServerInfo].[GameList] as b with (nolock) on a.gameid = b.GameID
		where
			userid = @intTargetID
	end

	set @chvMsg = '操作完成 : 查询完成'
	return 1
END




