-- =============================================
-- Author:  alan
-- Create date: 2018/3/12
-- Description:	<绑定銀行卡>
--	2018/03/21	Jason		新增銀行帳號修改紀錄
--	2018/03/22	Jason		修正銀聯卡帳號不能修改問題&新增@tintCodeType判斷驗證碼類型 & 更新部分調整RealName為UnionpayRealName
--	2018/3/27	alan		調整验证码失效順序. 如果只是姓名輸入錯誤驗證碼不會失效
--	2019/05/11	Jason		增加裂變檢查
-- =============================================
CREATE PROCEDURE [dbo].[PrPs_BindUnionPayAccount]
	@intUserID			int,				--用户ID
	@chvCer				varchar(32),		--凭证
	@chvCode			VARCHAR(6),			--验证码
	@chvRealName		varchar(64),		--銀行账号对应的姓名
	@chvUnionpayBank	varchar(5),			--所屬銀行代號
	@chvUnionpayAccount	varchar(64),		--銀行账号
	@tintCodeType		tinyint,			--验证码类型 1注册(這邊不用),2找回密码(這邊不用) 3找回银行密码(這邊不用) 4第一次綁定支付帳號 5修改支付帳號 6全民代理 7净利润
	@chvErrMsg			varchar(128) OUTPUT	--返回消息
AS
BEGIN

	SET NOCOUNT ON
	
	DECLARE @IPAddress	VARCHAR(15)
	
	--若不符條件則返回錯誤
	IF (@tintCodeType NOT IN (4,5,7))
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
	
	IF not len(ISNULL(@chvUnionPayAccount,'')) between 5 and 32
	BEGIN
		SET @chvErrMsg = '銀行卡账号长度必须是5-32位'
		RETURN -1
	END
	
	IF (SELECT COUNT(1) FROM Users.UserInfo WITH(NOLOCK) WHERE UnionPayAccount = @chvUnionPayAccount)>=3
	BEGIN
		SET @chvErrMsg = '该銀行卡账号已绑定3个游戏账号,请更换其它銀行卡账号进行绑定'
		RETURN -1
	END
	
	
	IF EXISTS(SELECT 1 FROM Users.UserInfo WITH(NOLOCK) WHERE UserID = @intUserID AND LEN(ISNULL(UnionPayAccount,''))>0)
		AND @tintCodeType = 4 -- Type = 4 不應該有值
	BEGIN
		SET @chvErrMsg = '您的账号已经绑定銀行卡不能重复绑定'
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
	
	
	IF NOT EXISTS(SELECT 1 FROM [Money].[UnionpayBankCode] WHERE Code=@chvUnionpayBank)
	BEGIN
		SET @chvErrMsg = '銀行代碼不正确'
		RETURN -1
	END

	--清理帳號是否有非法字元
	SET  @chvUnionpayAccount = [dbo].Fn_ClearAccount(@chvUnionpayAccount) 


	/*  --绑定用户銀行卡账号  */
	---------------------------------------------------------------------------------------------------------
	--UPDATE Users.UserInfo
	--SET 
	--RealName = @chvRealName,
	--UnionpayBank = @chvUnionpayBank,
	--UnionPayAccount = @chvUnionPayAccount
	--WHERE UserID = @intUserID

	DECLARE @Temp_UserInfo	TABLE
		(	UserID INT,
			New_RealName VARCHAR(64),
			New_UnionpayBank VARCHAR(5),
			New_UnionpayAccount VARCHAR(64),
			Old_RealName VARCHAR(64),
			Old_UnionpayBank VARCHAR(5),
			Old_UnionpayAccount VARCHAR(64)
		)
	
	DECLARE @LogChangeType			TINYINT = 10	-- 支付寶/銀聯榜定
	DECLARE @LogMemo				VARCHAR(128)
	DECLARE @NowTime				DATETIME = GETDATE()
	DECLARE @LogOldData				VARCHAR(64)
	DECLARE @LogNewData				VARCHAR(64)

	UPDATE Users.UserInfo
	SET [UnionpayRealName] = @chvRealName,
		UnionpayBank = @chvUnionpayBank,
		UnionPayAccount = @chvUnionPayAccount
	OUTPUT	INSERTED.UserID,INSERTED.UnionPayRealName,INSERTED.UnionpayBank,INSERTED.UnionpayAccount,
							DELETED.UnionPayRealName,DELETED.UnionpayBank,DELETED.UnionpayAccount
			INTO @Temp_UserInfo
	WHERE UserID = @intUserID

	SELECT	@LogOldData = '姓名 - ' +  Old_RealName + ' 銀行代碼 - ' + Old_UnionpayBank + ' 銀行卡账号' + Old_UnionpayAccount,
			@LogNewData = '姓名 - ' +  New_RealName + ' 銀行代碼 - ' + New_UnionpayBank + ' 銀行卡账号' + New_UnionpayAccount,
			@LogMemo = '銀行卡账号已修改'
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
    ON OBJECT::[dbo].[PrPs_BindUnionPayAccount] TO [GameUser]
    AS [dbo];


GO
GRANT EXECUTE
    ON OBJECT::[dbo].[PrPs_BindUnionPayAccount] TO [xbuser]
    AS [dbo];

