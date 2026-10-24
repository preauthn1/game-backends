-- =============================================
-- Author:  <xxx>
-- Create date: <xxx>
-- Description:	<大厅登录-快速登录>
-- Changelog:
--			2018/4/20 alan 檢查渠道是否存在
--	2018/5/14 alan 取得現有用戶ID加上渠道篩選條件
--  2018/6/3  alan 檢查IP是否超量
-- =============================================
CREATE PROCEDURE [dbo].[PrPs_UserLogon_Fast]
@chvVersion			varchar(16),                --客户端版本号
@tnyHDType			tinyint,					--硬件类型
@chvHDCode			varchar(64),                --硬件码
@chvIPAddress		VARCHAR(15),				--IP地址
@sintPlazaServerID	smallint,					--广场ID
@intAreaID			int,						--渠道ID
@chvInviteCode		varchar(10)=null,			--邀請碼
@intUserID			INT OUTPUT,					--用户ID
@chvCertification	VARCHAR(32) OUTPUT,			--证书
@chvErrMsg			VARCHAR(128) OUTPUT         --错误消息
AS
SET NOCOUNT ON

SET @intUserID = 0
SET @chvCertification = ''
SET @chvErrMsg = ''

--檢查IP是否正確
IF (dbo.Fn_CheckValidIP(@chvIPAddress) = 0)
BEGIN
	SET @chvIPAddress = '未知IP'
END


--檢查areaID是否存在
IF not exists(select 1 from Area.Area where AreaID = @intAreaID)
BEGIN
	SET @chvErrMsg = '渠道编号不存在'
	RETURN -1
END

	
--登录方式
DECLARE @tnyLogonType TINYINT
SET @tnyLogonType = 2

--获取用户ID
SELECT @intUserID = (select top 1 UserID FROM Users.UserInfo with (nolock)
WHERE HDType = @tnyHDType --1 PC, 2 Android, 3 IOS
AND HDCode = @chvHDCode
AND AreaID = @intAreaID
AND UserType = 3) --访客帐号



--用户不存在
IF @intUserID IS NULL
BEGIN

	IF not exists (select IPAddress from log.IPRegWhitelist where IPAddress=@chvIPAddress) --白名單
	BEGIN
	
		/*  --写注册记录  */
		-------------------------------------------------------------------------------------------
		EXEC [dbo].[PrPs_UpdateRegIPHDTypeLog]
			@chvIPAddress	= @chvIPAddress,
			@tnyHDType		= @tnyHDType,
			@chvHDCode		= @chvHDCode
		-------------------------------------------------------------------------------------------
	
		--檢查IP是否超量
		IF (SELECT TOP 1 [RegCount] FROM [CenterDB].[Log].[IPReg] where IPAddress = @chvIPAddress
		 and LogDate = CAST(CONVERT(VARCHAR(8),GETDATE(),112) AS INT)) >= 50
		 
		BEGIN
			SET @intUserID=0
			SET @chvErrMsg = '此IP注册过量，请勿使用机器人操作'
			RETURN -1
		END
			
		--檢查HDCode是否超量
		IF (SELECT [RegCount] FROM [CenterDB].[Log].[HDReg] where HDCode = @chvHDCode
		 and LogDate = CAST(CONVERT(VARCHAR(8),GETDATE(),112) AS INT)) >= 3
		
		BEGIN
			SET @intUserID=0
			SET @chvErrMsg = '此设备注册过量，请勿使用机器人操作'
			RETURN -1
		END


		IF exists ( select 1 from [Game].[ForbidIPAddress] with (nolock) where IPAddress=@chvIPAddress)		
		BEGIN
			SET @intUserID=0
			SET @chvErrMsg = '此IP被封锁，请联系客服'
			RETURN -1
		END
	END

	--2018/5/10 alan 臨時關閉
	if @intAreaID in (7500)
	Begin
		SET @intUserID=0
		SET @chvErrMsg = '游戏关闭进行维护中'
		RETURN -1
	End


	--檢查是否需要邀请
	DECLARE @intClub tinyint=0
	IF (Select PlatformID from area.area with (nolock) where areaid=@intAreaID) in (74) --黑桃娱乐
	BEGIN
		--check club invitecode
		IF @chvInviteCode is null or not exists(select top 1 1 from club.dbo.AgentUsers with (nolock) where InviteCode=@chvInviteCode)
		BEGIN
			SET @intUserID = 0
			SET @chvErrMsg = '邀请码不正确'
			RETURN -2
		END
		SET @intClub=1
	END


	--注册用户
	EXEC dbo.PrPs_CreateAccount_Fast
	@tnyHDType,
	@chvHDCode,
	@chvIPAddress,	
	@intAreaID,
	@intUserID output
	

	--绑定推广邀请 
	IF @intClub = 1
	BEGIN
		--推廣代理
		IF(LEN(@chvInviteCode) = 5)
		BEGIN
			EXEC [Club].[dbo].[PrPs_JoinUser]
				@intUserID = @intUserID,
				@chvInviteCode = @chvInviteCode,
				@chvErrMsg = ''
		END

	END
		
END


--產QR CODE
	declare @platformid int
	select	@platformid = platformid from area.area with (nolock) where areaid = @intAreaID
	if exists (select top 1 1 from Club02.[dbo].[DomainSetting] with (nolock) where platformid = @platformid)
	begin
		EXEC [Club02].[dbo].[PrWeb_UserAgent_AddUserQRCode]
			@intUserID = @intUserID,
			@intplatformid = @platformid,
			@intAreaID = @intAreaID,
			@chvMsg = ''		
	end

IF(LEN(@chvInviteCode) = 8)
BEGIN
	
	
	
	--全民代理/淨利潤/無限級關聯
	EXEC [Club02].[dbo].[PrWeb_UserAgent_CheckAgentMode]
		@intUserID = @intUserID,
		@chvInviteCode = @chvInviteCode,
		@chvMsg = ''
END


--执行公共登录逻辑
DECLARE @intLogonRet INT
EXEC @intLogonRet = dbo.PrPs_UserLogon_Common 
    @intAreaID,
	@intUserID,
    @tnyLogonType,
    @tnyHDType,
    @chvHDCode,
    @chvVersion,
    @sintPlazaServerID,
    @chvIPAddress,
    @chvCertification OUTPUT,
    @chvErrMsg OUTPUT
   
RETURN @intLogonRet






GO
GRANT EXECUTE
    ON OBJECT::[dbo].[PrPs_UserLogon_Fast] TO [GameUser]
    AS [dbo];


GO
GRANT EXECUTE
    ON OBJECT::[dbo].[PrPs_UserLogon_Fast] TO [xbuser]
    AS [dbo];

