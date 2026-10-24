-- =============================================
-- Author:  jason
-- Create date: 2019/04/11
-- Description: USER 取得用户qr code
-- =============================================
CREATE PROCEDURE [dbo].[PrPs_UserAgent_GetUserQRCode]	
@intUserID		int				--用户 ID
AS
BEGIN

	set nocount on

	declare @status tinyint = 1

	declare @qrcode varchar(8)
	declare @chvMsg varchar(100)

	--取得用户所属平台
	declare @PlatformID int
	declare @areaid int

	select	@areaid = areaid
	from CenterDB.[Users].[UserInfo] WITH (NOLOCK)
	WHERE
		UserID = @intUserID

	select @platformid = platformid
	from centerdb.area.area with (nolock)
	where
		areaid = @areaid


	--if not exists (select top 1 1 from [dbo].[UserAgentQRCode] with (nolock) where userid = @intUserID)
	--begin
			
	--	EXEC [dbo].[PrWeb_GenderQRCode]
	--		@chvCode = @qrcode OUTPUT

	--	while exists(select top 1 1 from [dbo].[UserAgentQRCode] with (nolock) where QRCode = @qrcode)
	--	begin
	--		EXEC [dbo].[PrWeb_GenderQRCode]
	--			@chvCode = @qrcode OUTPUT
	--	end

	--	exec [dbo].[PrWeb_UserAgent_AddUserQRCode]
	--		@intUserID		= @intUserID,				--用户 ID
	--		@intplatformid = @PlatformID,
	--		@intareaid = @areaid,
	--		@chvQrCode		= @qrcode,		--用户 qrcode
	--		@chvMsg			= @chvMsg	OUTPUT
	--end

	declare @chvQRUrl varchar(250)
	declare @UpperUserID int

	select	top 1 @chvQRUrl = [Domain] + '/?a=' + QRCode,
			@qrcode = QRCode
	from [dbo].[UserAgentQRCode] as a with (nolock)
		inner join [dbo].[DomainSetting] as b with (nolock) on a.domainid = b.id
	where
		userid = @intUserID

	select	@UpperUserID = TopUserID
	from [dbo].[UserAgentInfo] with (nolock)
	where
		userid = @intUserID
	
	set @chvQRUrl = isnull(@chvQRUrl,'')
	set @qrcode = isnull(@qrcode,'')
	set @UpperUserID = isnull(@UpperUserID,0)

	select	[UserID],
			[QRCode] = @chvQRUrl,
			[InviteCode] = @qrcode,
			UpperUserID = @UpperUserID
	from [dbo].[UserAgentQRCode] with (nolock)
	where
		userid = @intUserID
		and [Status] = @status

	return 1
END
