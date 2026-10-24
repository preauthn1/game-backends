

-- =============================================
-- Author:  harry
-- Create date: 2018/08/16
-- Description:	管理者直接新增渠道代理
--
-- =============================================
CREATE PROCEDURE [dbo].[PrWeb_AgentUser_Create_Area]
	@OperateUID		uniqueidentifier,  --操作者
	@chvLoginName		varchar(64),
	@intUserID			int = NULL,
	@chvEmail		varchar(50) = NULL,
	@chvTel		varchar(20) = NULL,
	@chvWeixin		varchar(50) = NULL,
	@chvQQ		varchar(50) = NULL,
	@chvPassword		varchar(200) = NULL,
	@chvEncrypt			varchar(64) = NULL,
	@chvInviteCode		varchar(20) = NULL,
	@chvQRUrl		varchar(200) = null,
	@chvVCode		varchar(20),
	@platformId	int=null,
	@chvErrMsg		Varchar(50) OUT
AS
BEGIN	
	DECLARE @chvOldData varchar(100) = '' --旧的资料
	DECLARE @chvNewData varchar(100) --旧的资料
	DECLARE @chvTitle varchar(50)
	DECLARE @chvContent varchar(100)
	DECLARE @UID uniqueidentifier

	DECLARE @generated_keys table([ID] uniqueidentifier)
	--渠道代理class=0
	DECLARE @tnyClass tinyint = 0
	DECLARE @tnyUserClass tinyint


	--转换大写,置换易混淆字元
	--IF @chvInviteCode IS NOT NULL
	--BEGIN
	--	SET @chvInviteCode = dbo.Fn_FormatInviteCode(@chvInviteCode)
	--END

	SELECT @tnyClass = Class From dbo.AgentUsers  (nolock) Where ID = @OperateUID
	SET @tnyUserClass = CASE @tnyClass WHEN 100 THEN 0 ELSE @tnyClass + 1 END

	--操作者身分不等於管理者
	IF @tnyClass != 100
	BEGIN

		SELECT UserID FROM [CenterDB].[Users].[UserInfo] with (nolock) WHERE UserID = @intUserID
		IF @@ROWCOUNT = 0 
		BEGIN
			SET @chvErrMsg = '游戏帐号不存在，请重新设定'
			RETURN -1
		END

		SELECT ID From dbo.AgentUsers  (nolock) Where UserID = @intUserID AND ParentID != @OperateUID
		IF @@ROWCOUNT > 0 
		BEGIN
			SET @chvErrMsg = '该玩家已是其他人的下线'
			RETURN -1
		END

		SELECT ID From dbo.AgentUsers  (nolock) Where (LoginName = @chvLoginName)
		IF @@ROWCOUNT > 0 
		BEGIN
			SET @chvErrMsg = '帐号名称重覆，请重新设定'
			RETURN -1
		END

	END
	ELSE 
	BEGIN
		SELECT ID From dbo.AgentUsers  (nolock) Where LoginName = @chvLoginName
		IF @@ROWCOUNT > 0 
		BEGIN
			SET @chvErrMsg = '帐号名称重覆，请重新设定'
			RETURN -1
		END
	END

	SELECT ID From dbo.AgentUsers  (nolock) Where InviteCode = @chvInviteCode
	IF @@ROWCOUNT > 0 
	BEGIN
		SET @chvErrMsg = '邀请码重覆，请重新发送'
		RETURN -1
	END


	--取得一個域名對應ID
	DECLARE @intDomainID int=0
	--Set @intDomainID = dbo.Fn_GetDomain()
	--If @intDomainID = 0
	--BEGIN
	--	SET @chvErrMsg = '无可分配的域名'
	--	RETURN -1
	--END
	--Update dbo.DomainConfig set Count=Count+1 where ID=@intDomainID

	--暂时忽略活码产生的位置,直接用官网的域名
	DECLARE @chvDomain varchar(200)
	SELECT @chvDomain=[OfficialSite] FROM [Club].[dbo].[SystemConfig] with (nolock)
	--Select @chvDomain=Domain from dbo.DomainConfig with (nolock) where ID=@intDomainID
	SET @chvQRUrl = @chvDomain + '/?a=' + @chvInviteCode


	--管理者新增渠道
	IF @tnyClass =100
		BEGIN
			SET @intUserID = 0


			if (@platformId is null or @platformId=0)
			begin
				SET @chvErrMsg = '请选择正确的平台'
				RETURN -1
			end


			INSERT INTO  dbo.AgentUsers(LoginName, UserID, Email, Tel, Weixin, QQ, Class, Password, Encrypt, InviteCode, DomainID, QRUrl, PlatformID) 
			OUTPUT inserted.ID INTO @generated_keys 
			Values (@chvLoginName, @intUserID, @chvEmail, @chvTel, @chvWeixin, @chvQQ, @tnyUserClass, @chvPassword, @chvEncrypt, @chvInviteCode, @intDomainID, @chvQRUrl, @platformId)

			SELECT  TOP 1 @UID =ID FROM @generated_keys;

			INSERT INTO  dbo.AgentRateRange(ID, DistributeRate, Class1RateRange, Class2RateRange, Class3RateRange, Class4RateRange) Values (@UID, 50, 0, 0, 0, 0)

			IF NOT EXISTS(select 1 from dbo.OfficialUserConfig where PlatformID=@platformId)
				INSERT INTO  dbo.OfficialUserConfig(PlatformID, User1Type,User2Type,User3Type,OfficialUser1,OfficialUser2,OfficialUser3)
				values(@platformId,1,1,1,'','','')

		END
	ELSE --新增下级代理
		BEGIN
			SELECT @UID = ID From dbo.AgentUsers with (nolock) Where ParentID = @OperateUID AND UserID = @intUserID
			IF @@ROWCOUNT = 0 
			BEGIN
				INSERT INTO  dbo.AgentUsers(ParentID, LoginName, UserID, Email, Tel, Weixin, QQ, Class, Level, Password, Encrypt, InviteCode, DomainID, QRUrl, PlatformID) 
				OUTPUT inserted.ID INTO @generated_keys 
				Values (@OperateUID, @chvLoginName, @intUserID, @chvEmail, @chvTel, @chvWeixin, @chvQQ, @tnyUserClass, 1, @chvPassword, @chvEncrypt, @chvInviteCode, @intDomainID, @chvQRUrl, @platformId)

				SELECT  TOP 1 @UID =ID FROM @generated_keys;
			END

			--根據上層代理的分成比例减5當成預設值
			DECLARE @ParentID uniqueidentifier = @OperateUID
			DECLARE @tnyParentClass tinyint
			DECLARE @intDefaultRate int
			SELECT @tnyParentClass = Class from dbo.AgentUsers with (nolock) where ID=@ParentID
			IF @tnyParentClass = 0 
				select @intDefaultRate=DistributeRate from AgentRateRange with (nolock) where ID=@ParentID
			ELSE 
				select @intDefaultRate=DistributeRate from GrandConfig with (nolock) where AgentID=@ParentID

			IF @intDefaultRate-5 > 0
				SET @intDefaultRate=@intDefaultRate-5


			--更新玩家Class
			UPDATE dbo.AgentUsers 
			SET Class = @tnyClass + 1, 
			Level = 1, 
			LoginName = @chvLoginName, 
			Password = [REDACTED],
			InviteCode = @chvInviteCode,
			[Status] = 1,
			DomainID = @intDomainID,
			QRUrl = @chvQRUrl
			WHERE ID =  @UID

			--新增预设阶级设定
			INSERT INTO  dbo.GrandConfig(ParentID, AgentID, Level, GradeName, DistributeRate, UpGradeLimit) Values (@OperateUID, @UID, 1, '代理', @intDefaultRate, 0)

			SET @chvTitle = '代理申请通知'
			SET @chvContent = '亲爱的玩家：恭喜您已经正式成为代理, 您的邀请码为' + @chvInviteCode

			--信件通知
			EXEC [CenterDB].dbo.PrPs_SendSysMail
			@intUserID,
			@chvTitle,
			@chvContent,
			0,
			1
		END

	--紀錄變更
	SET @chvNewData = 'LoginName=' + @chvLoginName + ' Class=' + Convert(varchar,@tnyUserClass) + ' UserID=' + Convert(varchar,@intUserID)	

	EXEC [dbo].[PrWeb_AddChangeLog] 
	@intChangeType = 7,
	@OperateUID = @OperateUID,
	@TargetUID = @UID,
	@chvOldData = @chvOldData,
	@chvNewData = @chvNewData,
	@chvMemo = ''
	
	DELETE FROM SMS WHERE VerifyCode = @chvVCode 
	SET @chvErrMsg = '新增代理成功'
	RETURN 1

END








