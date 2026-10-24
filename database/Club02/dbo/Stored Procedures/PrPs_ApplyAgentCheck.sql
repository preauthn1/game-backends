





-- =============================================
-- Author:  alan
-- Create date: 2018/08/20
-- Description:	检查申请状态
--		玩家可以申請成為一級(渠道代理审核) 二级代理(一级代理审核) 依此類推
--		已经绑定為四级代理下線的玩家不能升级代理(四級代理底下的玩家無法申請)
-- =============================================
CREATE PROCEDURE [dbo].[PrPs_ApplyAgentCheck]
	@intUserID			INT, 
	@chvErrMsg			Varchar(50) OUT
AS
BEGIN
 
	DECLARE @chvLoginName varchar(32)
	DECLARE @intUserType TINYINT
	DECLARE @ParentUID uniqueidentifier --父级代理uniqid
	DECLARE @tnyParentClass tinyint --父级代理等级
	DECLARE @UID uniqueidentifier --玩家uniqid
	DECLARE @tnyClass tinyint --玩家身分 100: 系統管理者 0:渠道代理 1 2 3 4級 99:一般玩家  
	DECLARE @intPlatformID INT
	DECLARE @intAreaID INT
	DECLARE @intTotalCharge bigint --總充值金額
	DECLARE @intChargeFeeLimit	bigint = 0 --升級充值限制

	SELECT @chvLoginName = LoginName, @intUserType=UserType, @intPlatformID = A.PlatformID, @intAreaID = U.AreaID  From CenterDB.Users.UserInfo U 	WITH (NOLOCK)
	JOIN CenterDB.Area.Area A ON A.AreaID = U.AreaID
	WHERE UserID = @intUserID

	IF @@ROWCOUNT = 0 
	BEGIN
		SET @chvErrMsg = '帐号不存在'
		RETURN -1
	END

	IF @intUserType <> 1
	BEGIN
		SET @chvErrMsg = '申请无效,请先进行绑定正式帐号'
		RETURN -2
	END

	--检查是否已经提出申请 (status 0待審核 1審核通過 2审核未通过)
	--审核未通过, 还可以继续申请
	IF EXISTS(Select 1 from dbo.AgentApply with (nolock) WHERE UserID=@intUserID and [Status] = 0)
	BEGIN
		SET @chvErrMsg = '您的申请尚在审核中'
		RETURN -3
	END

	--检查最后一次的申请是否是被拒绝
	Declare @intLastApplyStatus tinyint
	Select Top 1 @intLastApplyStatus=[Status] from dbo.AgentApply with (nolock) WHERE UserID=@intUserID Order by CreateTime desc
	IF @intLastApplyStatus = 2
	BEGIN
		SET @chvErrMsg = '本次申请未通过'
		RETURN -6
	END


	--檢查申請者身分
	SELECT @UID=ID, @tnyClass=Class, @ParentUID=ParentID From dbo.AgentUsers With (nolock) Where UserID = @intUserID
	IF @@ROWCOUNT = 0 
	BEGIN
		SET @chvErrMsg = '帐号不存在'
		RETURN -1
	END
 
	--检查是否已经是代理
	IF @tnyClass <> 99
	BEGIN
		SET @chvErrMsg = '此帐号已经是代理,无法申请'
		RETURN -4
	END

	--取得上級資料
	SELECT @tnyParentClass=Class From dbo.AgentUsers With (nolock) Where ID = @ParentUID
	
	--如果上級已經是4級代理, 就無法申請成為代理 (四級代理底下的玩家無法申請)
	IF @tnyParentClass = 4
	BEGIN
		SET @chvErrMsg = '此帐号无法申请為代理'
		RETURN -1
	END

	--取得申請條件
	SELECT @intChargeFeeLimit = 
	CASE 
	WHEN @tnyParentClass+1 = 1 THEN Class1ChargeFee
	WHEN @tnyParentClass+1 = 2 THEN Class2ChargeFee
	WHEN @tnyParentClass+1 = 3 THEN Class3ChargeFee
	WHEN @tnyParentClass+1 = 4 THEN Class4ChargeFee
	END
	From [dbo].[SystemConfig] with (nolock)

	IF @intTotalCharge < @intChargeFeeLimit
	BEGIN
		SET @chvErrMsg = '充值未达条件,无法申请'
		RETURN -5
	END

	SET @chvErrMsg = ''
	Return 1
END


