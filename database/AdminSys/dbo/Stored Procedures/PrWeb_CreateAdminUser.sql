-- =============================================
-- Author:  Jason
-- Create date: 2018/06/21
-- Description:	#498 優化: 新增後台使用者時, 改用storedprocedure處理
-- ChangeLog:
--		2018/7/2 alan 判斷是否為聯運帳號, 非聯運帳號預設配置所有平台
--		2018/7/4  emily #567 新增後台用戶時, 同時設定用戶顯示名稱/電報編號/用戶角色
--		2018/7/26 emily #625 後台改用GoogleAuthenticator 驗證登入 - 增加欄位SecretKey
-- =============================================
CREATE PROCEDURE [dbo].[PrWeb_CreateAdminUser]
(
	@chvUserName	varchar(32),
	@chvUserPW		varchar(256),
	@tnyUserType	tinyint,
	@chvIPAddress	varchar(15),
	@chvDisplayName	varchar(20) = NULL,
	@intTgchatid    int = 0,
	@intSecretKey   int = 0,
	@intRtnCode		int = 0		output
)
AS
BEGIN 
	
	set nocount on
	declare @intAdminID int = 0
	declare @status tinyint = 1
	declare @nowtime datetime = getdate()

	set @chvDisplayName = ISNULL(@chvDisplayName,'')

	if exists (select top 1 1 from [Admin].[Users] with (nolock) where [LoginName] = @chvUserName)
	begin
		set @intRtnCode	= -1
		return
	end

	insert into [Admin].[Users]
		([LoginName],[Password],[Status],[CreateTime],[LastLogonTime],[LastLogonIPAddress],[UserType],[DisplayName],[TGChatid],[SecretKey])
	values
		(@chvUserName,@chvUserPW,@status,@nowtime,@nowtime,@chvIPAddress,@tnyUserType,@chvDisplayName,@intTgchatid,@intSecretKey)

    SET @intAdminID = @@IDENTITY

	if @intAdminID = 0
	begin
		set @intRtnCode	= -2
		return
	end
	
	--判斷是否為聯運帳號, 非聯運帳號預設配置所有平台
	IF @tnyUserType <> 10 
	BEGIN
		Insert into CenterDB.Area.PlatformAdminUser(AdminID,PlatformID)
		Select @intAdminID as AdminID, PlatformID From CenterDB.Area.Platform with (nolock)
	END

	set @intRtnCode	= 1
	return

END
