

-- =============================================
-- Author:  harry
-- Create date: 2018/09/10
-- Description:	升级客户成为代理
--
-- =============================================
CREATE PROCEDURE [dbo].[PrWeb_CustomerUpgrade]
	@OperateUID		uniqueidentifier,  --操作者
	@intUserID			int = NULL,
	@chvInviteCode		varchar(20) = NULL,
	@chvQRUrl		varchar(200) = null,
	@chvErrMsg		Varchar(50) OUT
AS
BEGIN	
	DECLARE @TopUID uniqueidentifier --渠道代理uniqid
	DECLARE @ParentID uniqueidentifier --代理uniqid
	DECLARE @chvOldData varchar(100) = '' --旧的资料
	DECLARE @chvNewData varchar(100) --旧的资料
	DECLARE @chvTitle varchar(50)
	DECLARE @chvContent varchar(100)
	DECLARE @UID uniqueidentifier
	DECLARE @chvLoginName varchar(50)
	DECLARE @chvPassword varchar(500)

	DECLARE @tnyParentClass tinyint
	DECLARE @tnyUserClass tinyint

	SELECT @UID = ID, @ParentID = ParentID From dbo.AgentUsers  (nolock) Where UserID = @intUserID
	SELECT @tnyParentClass = Class From dbo.AgentUsers with  (nolock) Where ID = @ParentID

	SET @tnyUserClass = CASE @tnyParentClass WHEN 100 THEN 0 ELSE @tnyParentClass + 1 END

	SELECT TOP 1 @chvLoginName = UserID, @chvPassword = [REDACTED]  FROM [CenterDB].[Users].[UserInfo] with (nolock)  WHERE UserID = @intUserID
	IF @@ROWCOUNT = 0 
	BEGIN
		SET @chvErrMsg = '游戏帐号不存在，请重新设定'
		RETURN -1
	END

	SELECT ID From dbo.AgentUsers  (nolock) Where LoginName = @chvLoginName
	IF @@ROWCOUNT > 0 
	BEGIN
		SET @chvErrMsg = '游戏ID，帐号名称重覆，请重新设定'
		RETURN -1
	END



	SELECT ID From dbo.AgentUsers  (nolock) Where InviteCode = @chvInviteCode
	IF @@ROWCOUNT > 0 
	BEGIN
		SET @chvErrMsg = '邀请码重覆，请重新发送'
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


	--根據上層代理的分成比例减5當成預設值
	Declare @intDefaultRate int
	IF @tnyParentClass = 0 
		select @intDefaultRate=DistributeRate from AgentRateRange with (nolock) where ID=@ParentID
	ELSE 
		select @intDefaultRate=DistributeRate from GrandConfig with (nolock) where AgentID=@ParentID

	IF @intDefaultRate-5 > 0
		SET @intDefaultRate=@intDefaultRate-5


	--更新玩家Class
	UPDATE dbo.AgentUsers 
	SET Class = @tnyUserClass, 
	Level = 1, 
	LoginName = @chvLoginName, 
	Password = [REDACTED],
	InviteCode = @chvInviteCode,
	[Status] = 1,
	DomainID = @intDomainID,
	QRUrl = @chvQRUrl
	WHERE ID =  @UID

	--新增预设阶级设定
	INSERT INTO  dbo.GrandConfig(ParentID, AgentID, Level, GradeName, DistributeRate, UpGradeLimit) Values (@ParentID, @UID, 1, '代理', @intDefaultRate, 0)

	SET @chvTitle = '代理申请通知'
	SET @chvContent = '亲爱的玩家：恭喜您已经正式成为代理, 您的邀请码为' + @chvInviteCode

	--信件通知
	EXEC [CenterDB].dbo.PrPs_SendSysMail
	@intUserID,
	@chvTitle,
	@chvContent,
	0,
	1

	--紀錄變更
	SET @chvNewData = '登入帐号=' + @chvLoginName + ' 等级=' + Convert(varchar,@tnyUserClass) + ' UserID=' + Convert(varchar,@intUserID)	
	EXEC [dbo].[PrWeb_AddChangeLog] 
	@intChangeType = 7,
	@OperateUID = @OperateUID,
	@TargetUID = @UID,
	@chvOldData = @chvOldData,
	@chvNewData = @chvNewData,
	@chvMemo = ''
	
	SET @chvErrMsg = '升级代理成功'
	RETURN 1
END



