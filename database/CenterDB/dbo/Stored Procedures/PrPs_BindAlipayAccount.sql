-- =============================================
-- Author:  <xxx>
-- Create date: <xxx>
-- Description:	<绑定支付宝>
--	2018/03/21	Jason		新增支付寶帳號修改紀錄
--	2018/03/22	Jason		修正支付寶帳號不能修改問題&新增@tintCodeType判斷驗證碼類型
--	2018/3/27	alan		調整验证码失效順序. 如果只是姓名輸入錯誤驗證碼不會失效
--	2018/8/3	alan		增加支付宝封锁检查
--  2019/05/11  Jason		增加裂变检查
-- =============================================
CREATE PROCEDURE [dbo].[PrPs_BindAlipayAccount]
	@intUserID			int,				--用户ID
	@chvCer				varchar(32),		--凭证
	@chvCode			varchar(6),			--验证码
	@chvRealName		varchar(64),		--支付宝账号对应的姓名
	@chvAlipayAccount	varchar(64),		--支付宝账号
	@tintCodeType		tinyint,			--验证码类型 1注册(這邊不用),2找回密码(這邊不用) 3找回银行密码(這邊不用) 4第一次綁定支付帳號 5修改支付帳號
	@chvErrMsg			varchar(128) OUTPUT	--返回消息
AS
BEGIN

	SET NOCOUNT ON
	DECLARE @IPAddress VARCHAR(15)
	
	--若不符條件則返回錯誤
	IF (@tintCodeType NOT IN (4,5))
	BEGIN
		SET @chvErrMsg = '验证码类型錯誤'
		RETURN -1
	END

	--取得用戶手機號
	DECLARE @chvTel	VARCHAR(16)=null
	SELECT @chvTel = LoginName, @IPAddress=LastLogonIPAddress FROM Users.UserInfo with(nolock) where UserID = @intUserID and UserType=1
	IF @chvTel IS NULL 
	BEGIN
		SET @chvErrMsg = '非法操作'
		return -1
	END		

	
	IF not len(ISNULL(@chvRealName,'')) between 2 and 16
	BEGIN
		SET @chvErrMsg = '姓名字数只能介于2-16之间'
		RETURN -1
	END
	
	IF not len(ISNULL(@chvAlipayAccount,'')) between 5 and 32
	BEGIN
		SET @chvErrMsg = '支付宝账号长度必须是5-32位'
		RETURN -1
	END

	IF (SELECT COUNT(1) FROM Users.UserInfo WITH(NOLOCK) WHERE AlipayAccount = @chvAlipayAccount)>=3
	BEGIN
		SET @chvErrMsg = '该支付宝账号已绑定3个游戏账号,请更换其它支付宝账号进行绑定'
		RETURN -1
	END


	IF EXISTS(SELECT 1 FROM Game.ForbidAccount WITH(NOLOCK) WHERE AccountID=@chvAlipayAccount AND IdType=2)
	BEGIN
		SET @chvErrMsg = '该支付宝账号被封，请和管理员联系'
		RETURN -1
	END

	--检查验证码
	DECLARE @intReturn int
	EXEC @intReturn = dbo.PrPs_CheckSMSCode 
			@chvTel = @chvTel,
			@chvCode = @chvCode,
			@tintCodeType = @tintCodeType, 
			@chvErrMsg= @chvErrMsg OUTPUT		
	if @intReturn<>1
		return -1

	
	IF NOT EXISTS(SELECT 1 FROM Game.Certification WHERE UserID=@intUserID AND CerCode = @chvCer)
	BEGIN
		SET @chvErrMsg = '用户信息不正确'
		RETURN -1
	END

	
	
	IF EXISTS(SELECT 1 FROM Users.UserInfo WITH(NOLOCK) WHERE UserID = @intUserID AND LEN(ISNULL(AlipayAccount,''))>0)
		AND @tintCodeType = 4 -- Type = 4 不應該有值
	BEGIN
		SET @chvErrMsg = '您的账号已经绑定支付宝不能重复绑定'
		RETURN -1
	END


	--清理帳號是否有非法字元
	SET  @chvAlipayAccount = [dbo].Fn_ClearAccount(@chvAlipayAccount) 

	
	/*  --绑定用户支付宝账号  */
	---------------------------------------------------------------------------------------------------------
	--UPDATE Users.UserInfo
	--SET RealName = @chvRealName
	--	,AlipayAccount = @chvAlipayAccount
	--WHERE UserID = @intUserID

	DECLARE @Temp_UserInfo	TABLE (UserID INT,New_RealName VARCHAR(64),New_AlipayAccount VARCHAR(64),Old_RealName VARCHAR(64),Old_AlipayAccount VARCHAR(64))
	
	DECLARE @LogChangeType			TINYINT = 10	-- 支付寶/銀聯榜定
	DECLARE @LogMemo				VARCHAR(128)
	DECLARE @NowTime				DATETIME = GETDATE()
	DECLARE @LogOldData				VARCHAR(64)
	DECLARE @LogNewData				VARCHAR(64)

	UPDATE Users.UserInfo
	SET RealName = @chvRealName
		,AlipayAccount = @chvAlipayAccount
	OUTPUT INSERTED.UserID,INSERTED.RealName,INSERTED.AlipayAccount,DELETED.RealName,DELETED.AlipayAccount INTO @Temp_UserInfo
	WHERE UserID = @intUserID

	SELECT	@LogOldData = '姓名 - ' +  Old_RealName + ' 账号 - ' + Old_AlipayAccount,
			@LogNewData = '姓名 - ' +  New_RealName + ' 账号 - ' + New_AlipayAccount,
			@LogMemo = '支付宝账号已修改'
	FROM @Temp_UserInfo

	EXEC [LOGDBLINKEDSERVER].[LogDB].[dbo].[PrPs_Add_UserInfoChangeLog]
						@intUserID = @intUserID,
						@tnyChangeType = @LogChangeType,
						@chrOldData = @LogOldData,
						@chrNewData = @LogNewData,
						@chrIPAddress = @IPAddress,
						@chrMemo = @LogMemo,
						@dtWriteTime = @NowTime

	---------------------------------------------------------------------------------------------------------


	EXEC Club02.[dbo].[PrWeb_Split_BindingCheck]
		@intUserID = @intUserID,				--用户 ID
		@chvMsg	= ''

	
	SET @chvErrMsg = '绑定成功'
	
	RETURN 1	
	
	
END




GO
GRANT EXECUTE
    ON OBJECT::[dbo].[PrPs_BindAlipayAccount] TO [GameUser]
    AS [dbo];


GO
GRANT EXECUTE
    ON OBJECT::[dbo].[PrPs_BindAlipayAccount] TO [xbuser]
    AS [dbo];

