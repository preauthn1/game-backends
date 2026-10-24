-- =============================================
-- Author:  jason
-- Create date: 2019/04/13
-- Description: 全民代理后台 用户登入取得資訊
-- =============================================
CREATE PROCEDURE [dbo].[PrWeb_UserAgent_UserLoginInfo_Get]
(
	@tnyAdminType	tinyint = 1,			--1:用戶 2:管理者
	@intUserID		int = null,				--用户 ID / 管理者ID
	@chvAdminAccount	varchar(32) = null, --管理者帳號
	@chvMsg			varchar(250)	OUTPUT
)
AS
BEGIN

	set nocount on


	declare @userid int
	declare @status tinyint
	declare @nickname varchar(32)
	declare @password varchar(32)
	declare @encrypt varchar(10)
	declare @Mode1 int
	declare @Mode2 int
	declare @Mode3 int
	declare @Mode4 int
	declare @Mode5 int
	declare @Mode6 int
	declare @Admintype int

	if(@chvAdminAccount is null and @intUserID is null) or (@tnyAdminType not in (1,2))
	begin
		set @chvMsg	= '操作错误 : 用户不存在'
		return -1
	end


	declare @qrurl varchar(250)
	if(@tnyAdminType = 1)
	begin
		select	@qrurl = b.Domain + '?a=' + a.QRCode
		from [dbo].[UserAgentQRCode] as a with (nolock)
			inner join [dbo].[DomainSetting] as b with (nolock) on a.domainid = b.id
		where userid = @intUserID
	end
	set @qrurl = isnull(@qrurl,'')


	/*  取出淨利潤玩家是否可以登入  */
	declare @mode1userstatus int = 0
	declare @mode2userstatus int = 0
	declare @mode3userstatus int = 0
	declare @mode4userstatus int = (select case when status = 2 then 1 else 0 end from [dbo].[NetProfitAgent_UserInfo] with (nolock) where userid = @intUserID)
	declare @mode5userstatus int = (select case when status = 2 then 1 else 0 end from [dbo].[Unlimit_UserInfo] with (nolock) where userid = @intUserID)
	declare @mode6userstatus int = 0

	if(@tnyAdminType = 1 and (@mode1userstatus + @mode2userstatus + @mode3userstatus + @mode4userstatus + @mode5userstatus + @mode6userstatus) = 0)
	begin
		set @chvMsg	= '操作错误 : 您的權限不足'
		return -1
	end
	
	
	--平台暫存
	DECLARE @Temp_PlatformID TABLE (PlatformID	INT,PlatformName varchar(50),AreaID int,AreaName varchar(50))


	if(@tnyAdminType = 1)
	begin
		select	@userid = a.userid,
				@status = a.[Status],
				@nickname = a.[NickName],
				@password = [REDACTED],
				@encrypt = a.[Encrypt],
				@Mode1 = 0, --cast(case when b.permission = 1 then 1 else 0 end as int),
				@Mode2 = 0,--[Mode2],
				@Mode3 = 0,--[Mode3],
				@Mode4 = cast(case when e.permission = 1 then 1 else 0 end as int),
				@Mode5 = cast(case when f.permission = 1 then 1 else 0 end as int),
				@Mode6 = 0,--[Mode6],
				@Admintype = 2
		from [dbo].[UserAgentQRCode] as a with (nolock)
			--left join [dbo].[UserAgentInfo] as b with (nolock) on a.userid = b.userid
			--left join c:2
			--left join [dbo].[Split_UserInfo] as d with (nolock) on a.userid = d.userid
			left join [dbo].[NetProfitAgent_UserInfo] as e with (nolock) on a.userid = e.userid
			left join [dbo].[Unlimit_UserInfo] as f with (nolock) on a.userid = f.userid
			--left join g:6
		where
			a.userid = @intUserID

		--撈出該用戶可讀取的全部平台
		INSERT INTO @Temp_PlatformID
		Select	B.PlatformID,
				C.Name,
				B.AreaID,
				B.AreaName
		From CenterDB.[Users].[UserInfo] as a
			inner join CenterDB.area.area as b with (nolock) on a.AreaID = B.AreaID
			inner join CenterDB.area.[platform] as c with (nolock) on b.PlatformID = c.PlatformID
		where
			a.userid = @userid
	end

	if(@tnyAdminType = 2)
	begin

		select	@userid = id,
				@status = [Status],
				@nickname = [Account],
				@password = [REDACTED],
				@encrypt = isnull([Encrypt],''),
				@Mode1 = [Mode1],
				@Mode2 = [Mode2],
				@Mode3 = [Mode3],
				@Mode4 = [Mode4],
				@Mode5 = [Mode5],
				@Mode6 = [Mode6],
				@Admintype = AdminType
		from [dbo].[AgentAdminUser] with (nolock)
		where
			[Account] = @chvAdminAccount

		--撈出該用戶可讀取的全部平台
		INSERT INTO @Temp_PlatformID
		Select	B.PlatformID,
				C.Name,
				B.AreaID,
				B.AreaName
		From [dbo].[AgentPlatformRelations] as a
			inner join CenterDB.area.area as b with (nolock) on a.PlatformID = B.PlatformID
			inner join CenterDB.area.[platform] as c with (nolock) on b.PlatformID = c.PlatformID
		where
			a.AdminID = @userid
	end

	if (@status <> 1) or (@userid is null)
	begin
		set @chvMsg	= '操作错误 : 用户暫時無法登入,請連繫客服'
		return -1
	end


	set @Mode1 = isnull(@Mode1,0)
	set @Mode2 = isnull(@Mode2,0)
	set @Mode3 = isnull(@Mode3,0)
	set @Mode4 = isnull(@Mode4,0)
	set @Mode5 = isnull(@Mode5,0)
	set @Mode6 = isnull(@Mode6,0)
	set @Admintype = isnull(@Admintype,2)


	/*  回傳前端資料  */
	select	userid = @userid,
			nickname = @nickname,
			pwd = [REDACTED],
			encrypt = @encrypt,
			Mode1 = @Mode1,
			Mode2 = @Mode2,
			Mode3 = @Mode3,
			Mode4 = @Mode4,
			Mode5 = @Mode5,
			Mode6 = @Mode6,
			QRURL = @qrurl,
			Admintype = @Admintype


	/*  回傳平台對應表 提供前端選單使用  */
	select	PlatformID,
			PlatformName,
			AreaID,
			AreaName 
	from @Temp_PlatformID
	

	set @chvMsg	= '操作完成 : 用户修改数据完成'
	return 1
END



