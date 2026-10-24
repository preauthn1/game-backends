



-- =============================================
-- Author:  alan
-- Create date: 2018/08/8
-- Description:	
--	申请成为代理, 玩家可以申請成為一級(渠道代理审核) 二级代理(一级代理审核) 依此類推
--	已经绑定為四级代理下線的玩家不能升级代理(四級代理底下的玩家無法申請)
-- =============================================
CREATE PROCEDURE [dbo].[PrPs_ApplyAgent]
	@intUserID		INT, 
	@chvTel			VARCHAR(20) = null,
	@chvWeixin		VARCHAR(50) = null,
	@chvQQ			VARCHAR(50) = null,
	@chvEmail		VARCHAR(50) = null,
	@chvCode		VARCHAR(10) = null, --短信驗證碼
	@chvErrMsg		Varchar(50) OUT
AS
BEGIN
 
	DECLARE @chvLoginName varchar(32)
	DECLARE @intUserType TINYINT
	DECLARE @ParentUID uniqueidentifier --父级代理uniqid
	DECLARE @UserUID uniqueidentifier --玩家uniqid
	DECLARE @tnyClass tinyint --玩家身分 100: 系統管理者 0:渠道代理 1 2 3 4級 99:一般玩家  
	DECLARE @tnyApplyClass tinyint
	DECLARE @tnyParentClass tinyint --父级代理等级
	DECLARE @intPlatformID int --平台號
	DECLARE @intAreaID int -- 渠道號
	DECLARE @intTotalCharge bigint --總充值金額
	DECLARE @intChargeFeeLimit	bigint = 0 --升級充值限制


	--检查验证码
	DECLARE @intReturn int
	EXEC @intReturn = [CenterDB].[dbo].[PrPs_CheckSMSCode]
			@chvTel = @chvTel,
			@chvCode = @chvCode,
			@tintCodeType = 6, --1注册 ,2找回密码, 3找回银行密码, 4第一次綁定支付帳號, 5修改支付帳號 6全民代理
			@chvErrMsg= @chvErrMsg OUTPUT		
	IF @intReturn<>1
		RETURN -1



	--檢查使用者狀態
	DECLARE	@return_value int
	EXEC @return_value = [dbo].[PrPs_ApplyAgentCheck] @intUserID = @intUserID, @chvErrMsg = @chvErrMsg OUTPUT
	IF @return_value NOT IN (1,-6)
	BEGIN
		RETURN -1
	END

	


	SELECT @chvLoginName = LoginName, @intUserType=UserType, @intPlatformID = A.PlatformID, @intAreaID = U.AreaID, @intTotalCharge=(SumCharge+SumAgentCharge) From CenterDB.Users.UserInfo U WITH (NOLOCK)
	JOIN CenterDB.Area.Area A ON A.AreaID = U.AreaID	
	WHERE UserID = @intUserID

	SELECT @UserUID=ID, @tnyClass=Class, @ParentUID=ParentID From dbo.AgentUsers With (nolock) Where UserID = @intUserID

	--取得父級代理身分
	SELECT @tnyParentClass = Class from dbo.AgentUsers where ID = @ParentUID

	--如果父級代理是1級, 則玩家申請成為2級, 依此類推
	SET @tnyApplyClass = @tnyParentClass + 1

	INSERT INTO  dbo.AgentApply(ID,UserID,LoginName,PlatformID,AreaID,ParentID,ApplyClass,Status, Weixin, Tel, QQ, Email) 
	Values (@UserUID, @intUserID, @chvLoginName, @intPlatformID, @intAreaID, @ParentUID, @tnyApplyClass, 0, @chvWeixin, @chvTel, @chvQQ, @chvEmail)

	SET @chvErrMsg = '送出成功!'
	RETURN 1
END
