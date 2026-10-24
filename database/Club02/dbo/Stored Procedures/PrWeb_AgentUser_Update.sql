




-- =============================================
-- Author:  harry
-- Create date: 2018/08/14
-- Description:	更新代理基本资料
--
-- =============================================
CREATE PROCEDURE [dbo].[PrWeb_AgentUser_Update]
	@OperateUID			uniqueidentifier, --操作者
	@AgentUID			uniqueidentifier, --被更新者
	@intUerID		int = NULL,
	@chvEmail		varchar(50) = NULL,
	@chvTel		varchar(20) = NULL,
	@chvWeixin		varchar(50) = NULL,
	@chvQQ		varchar(50) = NULL,
	@intLevel		int = NULL,
	@chvQRUrl		varchar(200) = NULL,
	@intDomainID	int=null,
	@chvPassword		varchar(200) = NULL,
	@chvEncrypt			varchar(64) = NULL,
	@chvInviteCode		varchar(20) = NULL,
	@intDistributeRate	int = NULL,
	@tnyStatus tinyint = NULL,
	@intAllowWithdraw int = NULL,
	@intUser1Type tinyint=null,
	@intUser2Type tinyint=null,
	@intUser3Type tinyint=null,
	@chvOfficialUser1 varchar(50)=null,
	@chvOfficialUser2 varchar(50)=null,
	@chvOfficialUser3 varchar(50)=null,
	@chvErrMsg		Varchar(50) OUT
AS
BEGIN	

	DECLARE @chvOldInviteCode varchar(20) --
	DECLARE @chvOldPassword varchar(200) --
	DECLARE @tnyOldStatus tinyint --
	DECLARE @tnyClass tinyint
	DECLARE @ParentUID uniqueidentifier
	DECLARE @intPlatformID int=0

	SELECT @chvOldPassword = [REDACTED], @tnyClass=Class, @ParentUID=ParentID,
	@chvOldInviteCode = InviteCode , @tnyOldStatus = Status, @intPlatformID=ISNULL(PlatformID,0)
	FROM dbo.AgentUsers with (nolock) WHERE ID = @AgentUID
	

	--是否有变更邀请码
	IF @chvInviteCode is not null
	BEGIN

		--检查邀请码是否重复	
		IF EXISTS (SELECT ID From dbo.AgentUsers with (nolock) Where InviteCode = @chvInviteCode)
		BEGIN
			SET @chvErrMsg = '邀请码重覆，请重新发送'
			RETURN -1
		END
	END


	--檢查比例是否合規, 不能大於上級比例, 不能小於所有下級的最大值
	IF @intDistributeRate IS NOT NULL
	BEGIN
		--從渠道代理檢查
		IF  @tnyClass=1 and @intDistributeRate >= (select DistributeRate from AgentRateRange with (nolock) where ID = @ParentUID)
		BEGIN
			SET @chvErrMsg = '错误：分成比例不能大于等于上级代理'
			RETURN -1
		END
		--從一般代理檢查
		IF  @tnyClass in (2,3,4) and @intDistributeRate >= (select DistributeRate from GrandConfig with (nolock) where AgentID = @ParentUID)
		BEGIN
			SET @chvErrMsg = '错误：分成比例不能大于等于上级代理'
			RETURN -1
		END
		--從一般代理檢查
		IF  @tnyClass in (0,1,2,3) and @intDistributeRate <= (select top 1 Max(DistributeRate) from GrandConfig with (nolock) where ParentID = @AgentUID)
		BEGIN
			SET @chvErrMsg = '错误：分成比例不能小于等于下级代理'
			RETURN -1
		END

	END



	UPDATE dbo.AgentUsers 
	SET UserID = ISNULL(@intUerID,UserID), 
	InviteCode = ISNULL(@chvInviteCode,InviteCode), 
	Password = [REDACTED],Password), 
	Encrypt = ISNULL(@chvEncrypt,Encrypt), 
	Status = ISNULL(@tnyStatus,Status), 
	AllowWithdraw = ISNULL(@intAllowWithdraw,AllowWithdraw), 
	Email = ISNULL(@chvEmail,Email), 
	Tel = ISNULL(@chvTel,Tel), 
	Weixin = ISNULL(@chvWeixin,Weixin), 
	QQ = ISNULL(@chvQQ,QQ), 
	QRUrl = ISNULL(@chvQRUrl,QRUrl)
	WHERE ID = @AgentUID

	--只有渠道代理才能更新
	if @tnyClass=0
	begin
		UPDATE dbo.OfficialUserConfig
		SET
		User1Type=ISNULL(@intUser1Type,User1Type),
		User2Type=ISNULL(@intUser2Type,User2Type),
		User3Type=ISNULL(@intUser3Type,User3Type),
		OfficialUser1=ISNULL(@chvOfficialUser1,OfficialUser1),
		OfficialUser2=ISNULL(@chvOfficialUser2,OfficialUser2),
		OfficialUser3=ISNULL(@chvOfficialUser3,OfficialUser3)
		where PlatformID=@intPlatformID

		if @@ROWCOUNT=0
			INSERT INTO  dbo.OfficialUserConfig(PlatformID, User1Type,User2Type,User3Type,OfficialUser1,OfficialUser2,OfficialUser3)
			values(@intPlatformId,ISNULL(@intUser1Type,1),ISNULL(@intUser2Type,1),ISNULL(@intUser3Type,1), ISNULL(@chvOfficialUser1,''),ISNULL(@chvOfficialUser2,''),ISNULL(@chvOfficialUser3,''))
	end


	IF @intDistributeRate IS NOT NULL 
	BEGIN

		--獲取變更前資料
		Declare @chvOldData varchar(200),@chvNewData varchar(200)

		SELECT @chvOldData = 
			'总代比例=' + Convert(varchar,DistributeRate) 
		FROM dbo.AgentRateRange with (nolock) WHERE ID = @AgentUID

		UPDATE dbo.AgentRateRange 
		SET DistributeRate = @intDistributeRate 
		WHERE ID = @AgentUID

		
		--變更後資料
		SET @chvNewData = '分成比例=' + Convert(varchar,@intDistributeRate) 
		--紀錄變更
		EXEC [dbo].[PrWeb_AddChangeLog] @intChangeType = 4,	@OperateUID = @OperateUID,	@TargetUID = @AgentUID,	@chvOldData = @chvOldData, @chvNewData = @chvNewData, @chvMemo = ''

	END

	--紀錄變更	
	IF @chvInviteCode IS NOT NULL 
		EXEC [dbo].[PrWeb_AddChangeLog] @intChangeType = 3,	@OperateUID = @OperateUID,	@TargetUID = @AgentUID,	@chvOldData = @chvOldInviteCode, @chvNewData = @chvInviteCode, @chvMemo = ''

	IF @chvPassword IS NOT NULL 
	BEGIN
		SET @chvOldPassword = [REDACTED]
		SET @chvPassword = [REDACTED]
		EXEC [dbo].[PrWeb_AddChangeLog] @intChangeType = 1,	@OperateUID = @OperateUID,	@TargetUID = @AgentUID,	@chvOldData = @chvOldPassword, @chvNewData = @chvPassword, @chvMemo = ''
	END	

	IF @tnyOldStatus <> @tnyStatus 
	BEGIN
		SET @chvOldData =  CASE WHEN @tnyOldStatus = 1 THEN '启用' ELSE '停权' END
		SET @chvNewData =  CASE WHEN @tnyStatus = 1 THEN '启用' ELSE '停权' END
		EXEC [dbo].[PrWeb_AddChangeLog] @intChangeType = 2,	@OperateUID = @OperateUID,	@TargetUID = @AgentUID,	@chvOldData = @chvOldData, @chvNewData = @chvNewData, @chvMemo = ''
	END

	SET @chvErrMsg = '更新成功'
	RETURN 1
END



