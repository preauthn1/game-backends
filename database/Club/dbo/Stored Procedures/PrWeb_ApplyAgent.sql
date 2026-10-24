
-- =============================================
-- Author:  harry
-- Create date: 2018/08/9
-- Description:	审核成为代理, 包含审核成为一级代理（渠道代理审核）或二级代理（一级代理审核）
--
-- =============================================
CREATE PROCEDURE [dbo].[PrWeb_ApplyAgent]
	@ApplyUID			uniqueidentifier,
	@tnyStatus			tinyint,
	@chvInviteCode		varchar(20) = NULL,
	@chvQRUrl		varchar(200) = null,
	@chvErrMsg			Varchar(50) OUT
AS
BEGIN
	DECLARE @TopUID uniqueidentifier --渠道代理uniqid
	DECLARE @ParentUID uniqueidentifier --父级代理uniqid
	DECLARE @UserID int --玩家id
	DECLARE @LoginName varchar(80) --玩家name
	DECLARE @UID uniqueidentifier --玩家uniqid
	DECLARE @tnyUserType tinyint --玩家身分 100: 系統管理者 0:渠道代理 1:一級代理  2:二級代理 99:一般玩家  
	DECLARE @tnyParentClass tinyint
	DECLARE @chvTitle varchar(50)
	DECLARE @chvContent varchar(100)
	DECLARE @chvUserPassword varchar(200)
	DECLARE @chvTel			VARCHAR(20)
	DECLARE @chvWeixin		VARCHAR(50)
	DECLARE @chvQQ			VARCHAR(50)
	DECLARE @chvEmail		VARCHAR(50)


	--檢查申请单
	SELECT @UserID=UserID, @LoginName=LoginName, @chvTel=Tel, @chvQQ=QQ, @chvWeixin=Weixin, @chvEmail=Email From dbo.AgentApply  (nolock) Where ApplyID = @ApplyUID
	IF @@ROWCOUNT = 0 
	BEGIN
		SET @chvErrMsg = '申请单不存在'
		RETURN -1
	END


	--審核未通過
	IF @tnyStatus = 2
	BEGIN

		SET @chvTitle = '代理申请通知'
		SET @chvContent = '亲爱的玩家：很抱歉, 您的代理申请审核未通过, 请重新申请'

		--信件通知
		EXEC [CenterDB].dbo.PrPs_SendSysMail
		@UserID,
		@chvTitle,
		@chvContent,
		0,
		1


		--更新申请单
		UPDATE dbo.AgentApply SET Status = @tnyStatus
		WHERE ApplyID =  @ApplyUID

		SET @chvErrMsg = '送出成功!'
		RETURN 1
	END

	--转换大写, 置换易混淆字元
	IF @chvInviteCode IS NOT NULL
	BEGIN

		--检查邀请码是否重复	
		IF EXISTS (SELECT ID From dbo.AgentUsers with (nolock) Where InviteCode = @chvInviteCode)
		BEGIN
			SET @chvErrMsg = '邀请码重覆，请重新发送'
			RETURN -1
		END
	END


	--检查游戏帐号
	SELECT @chvUserPassword = [REDACTED] From [CenterDB].[Users].[UserInfo] With (nolock) Where UserID = @UserID
	IF @@ROWCOUNT = 0 
	BEGIN
		SET @chvErrMsg = '游戏帐号不存在'
		RETURN -1
	END

	--检查申请者身分
	SELECT @UID = ID, @tnyUserType = Class, @ParentUID = ParentID From dbo.AgentUsers With (nolock) Where UserID = @UserID
	IF @@ROWCOUNT = 0 
	BEGIN
		SET @chvErrMsg = '帐号不存在'
		RETURN -1
	END
 
	--检查是否已经是代理
	IF @tnyUserType <> 99
	BEGIN
		SET @chvErrMsg = '此帐号已经是代理，无法申请'
		RETURN -1
	END

	--检查上层代理
	SELECT @tnyParentClass = Class From dbo.AgentUsers With (nolock) Where ID = @ParentUID
	IF @@ROWCOUNT = 0 
	BEGIN
		SET @chvErrMsg = '上层代理并不存在'
		RETURN -1
	END

	--找出上層的父節點
	SELECT @TopUID = Id from [fn_RecursiveUp](@UID) Where Class = 0
	IF @@ROWCOUNT = 0 
	BEGIN
		SET @chvErrMsg = '渠道代理并不存在，请通知客服'
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


	--根據上層代理的分成比例減5當成預設值
	Declare @intDefaultRate int
	IF @tnyParentClass = 0 
		select @intDefaultRate=DistributeRate from AgentRateRange with (nolock) where ID=@ParentUID
	ELSE 
		select @intDefaultRate=DistributeRate from GrandConfig with (nolock) where AgentID=@ParentUID

	IF @intDefaultRate-5 > 0
		SET @intDefaultRate=@intDefaultRate-5


	--更新玩家Class
	UPDATE dbo.AgentUsers 
	SET Class = @tnyParentClass + 1, 
	Level = 1, 
	LoginName = @LoginName, 
	Password = [REDACTED],
	InviteCode = @chvInviteCode,
	[Status] = 1,
	Weixin=@chvWeixin,
	QQ=@chvQQ,
	Tel=@chvTel,
	Email=@chvEmail,
	DomainID=@intDomainID,
	QRUrl = @chvQRUrl
	WHERE ID =  @UID

	--新增预设阶级设定
	INSERT INTO  dbo.GrandConfig(ParentID, AgentID, Level,GradeName, DistributeRate, UpGradeLimit) Values (@ParentUID, @UID, 1, '代理', 0, 0)


	SET @chvTitle = '代理申请通知'
	SET @chvContent = '亲爱的玩家：恭喜您已经正式成为代理, 您的邀请码为' + @chvInviteCode

	--更新申请单
	UPDATE dbo.AgentApply SET Status = @tnyStatus
	WHERE ApplyID =  @ApplyUID
	
	--信件通知
	EXEC [CenterDB].dbo.PrPs_SendSysMail
	@UserID,
	@chvTitle,
    @chvContent,
	0,
    1


	SET @chvErrMsg = '送出成功!'
	RETURN 1
END


