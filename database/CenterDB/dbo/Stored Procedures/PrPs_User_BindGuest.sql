-- =============================================
-- Author:  <xxx>
-- Create date: <xxx>
-- Description:	<绑定游客账号>
-- Changelog:
--		2018/4/9 alan 绑定赠送金幣改為欄位設定
--	2018/05/28	Jason		#349 Add綁定紀錄
--	2018/06/27	jason		#535 後台IP封鎖支援IP範圍設定
--	2018/8/3	alan		增加封锁检查手机号码
--	2019/05/11	Jason		增加裂變檢查
-- =============================================
CREATE PROCEDURE [dbo].[PrPs_User_BindGuest]
@intUserID		INT,					--用户ID
@chvCode		VARCHAR(32),			--验证码
@chvTel			VARCHAR(32),			--绑定手机号码
@chvPassword	VARCHAR(32),			--账号密码
@tintCodeType	tinyint = 1,			--验证码类型 1注册,2找回密码 3找回银行密码 4第一次綁定支付帳號 5修改支付帳號 6全民代理 7净利润
@chvErrMsg		VARCHAR(128) OUTPUT		--返回消息
AS
BEGIN

	SET NOCOUNT ON
	SET XACT_Abort ON

	DECLARE @chvExLoginName		VARCHAR(32)
	DECLARE @intReturn			INT
	DECLARE @chvHDCode			VARCHAR(32)
	DECLARE @tnyHDType			TINYINT
	DECLARE @IPAddress			VARCHAR(40)
	DECLARE @chvExCode			VARCHAR(32)
	DECLARE @AreaID				INT
	DECLARE @PlatformID			INT
	DECLARE	@lngCurrentAmount	BIGINT
	DECLARE @intGiveMoney			BIGINT=0 --绑定赠送金幣/或轉移金幣
	DECLARE @intGiveBankMoney		BIGINT=0 --轉移保险箱金幣
	DECLARE @intRebind		tinyint = 0 --是否重新綁定
	DECLARE @sintSourceType SMALLINT
	DECLARE @chvMemo		VARCHAR(100)=''
	DECLARE @chvMemoBank	VARCHAR(100)=''
	DECLARE @chvMsgContent	VARCHAR(1024)	
	DECLARE @chvTitle		VARCHAR(50)
	DECLARE @intReturnValue int
	
	--检查验证码
	EXEC @intReturn = dbo.PrPs_CheckSMSCode 
			@chvTel = @chvTel,
			@chvCode = @chvCode,
			@tintCodeType = @tintCodeType,
			@chvErrMsg= @chvErrMsg OUTPUT		
	if @intReturn<>1
		return -1


	--检查账号类型
	SELECT	@chvExLoginName = LoginName,
			@chvHDCode = HDCode,
			@tnyHDType = HDType,
			@IPAddress = LastLogonIPAddress,
			@AreaID = AreaID
	FROM Users.UserInfo 
	WHERE
		UserID = @intUserID AND
		UserType = 3

	SELECT	@PlatformID = PlatformID FROM Area.Area WITH (NOLOCK) WHERE AreaID = @AreaID

	IF @chvExLoginName IS NULL
	BEGIN
		SET @chvErrMsg = '操作失败:您的账号不是游客'
		RETURN -1
	END


	IF EXISTS(SELECT TOP 1 1 FROM Game.ForbidIPAddress WHERE [min] <= [dbo].[f_IP2Int](@IPAddress) and [max] >= [dbo].[f_IP2Int](@IPAddress))
	BEGIN
		SET @chvErrMsg = '操作失败:此IP 被封，请和管理员联系'
		RETURN -1	
	END

	IF @chvExLoginName NOT LIKE 'Fast_%'
	BEGIN
		SET @chvErrMsg = '操作失败:您已经绑定过账号'
		RETURN -1
	END

	IF EXISTS(SELECT 1 FROM Users.UserInfo With (nolock) WHERE LoginName = @chvTel)
	BEGIN
		--進行重新綁定
		SET @intRebind = 1

		--SET @chvErrMsg = '操作失败:账号名已经存在'
		--RETURN -1	
	END

	IF EXISTS(SELECT 1 FROM Game.ForbidAccount WITH(NOLOCK) WHERE AccountID=@chvTel AND IdType=1)
	BEGIN
		SET @chvErrMsg = '操作失败:该手机号码被封，请和管理员联系'
		RETURN -1
	END

	--限制一个机器码只能注册3个账号
	IF (SELECT count(1) FROM Users.UserInfo WHERE UserType = 1 AND HDCode = @chvHDCode AND HDType = @tnyHDType) >=3 AND @intRebind=0
	BEGIN
		SET @chvErrMsg = '操作失败:一个设备只允许绑定3个账号'
		RETURN -1
	END

	--密码加密
	SET @chvPassword = [REDACTED]


	----------------------------------------------------------------------------------------------------
	BEGIN TRAN

	IF @intRebind = 1
	BEGIN

		--解綁定
		DECLARE @intOldMoney bigint, @intOldBankMoney bigint
		DECLARE @intOldUserID int
		DECLARE @intOldAreaID int
		DECLARE @intOldPlatformID int

		--找出舊資料
		SELECT @intOldUserID=UserID,@intOldAreaID=AreaID from Users.UserInfo with (nolock) Where LoginName=@chvTel
		SELECT @intOldPlatformID=PlatformID from Area.Area with (nolock) where AreaID=@intOldAreaID
		SELECT @intOldMoney=ISNULL(Amount,0) From Money.Cash with (nolock) where UserID=@intOldUserID
		SELECT @intOldBankMoney=ISNULL(Amount,0) From Money.Bank with (nolock) where UserID=@intOldUserID

		--同平台不能跳包
		IF @intOldPlatformID = @PlatformID
		BEGIN
			Rollback TRAN
			SET @chvErrMsg = '操作失败:账号名已经存在'
			RETURN -1
		END

		/*
		--下列平台不能跳包
		IF @intOldPlatformID in (68,70,71,72,74,75,76,85,92)
		BEGIN
			Rollback TRAN
			SET @chvErrMsg = '操作失败:账号名已经存在'
			RETURN -1
		END
		*/


		--變回遊客
		UPDATE Users.UserInfo SET LoginName='Fast_' + LoginName + Convert(varchar,UserID), UserType=3 Where UserID=@intOldUserID
		--UPDATE Money.Cash SET Amount=0 where UserID=@intOldUserID
		--UPDATE Money.Bank SET Amount=0 where UserID=@intOldUserID

		--更新全民代理系統UserID
		UPDATE Club.dbo.AgentUsers SET USERID=@intUserID WHERE LoginName=@chvTel

		Insert into LogDB.Money.MoneyLog(Userid,SourceTypeID,CurrentAmount,UpdateAmount,TaxAmount,IPAddress,Memo,AreaID,PlatformID,CreateTime)
		Select @intUserID,10003,0,0,0,@IPAddress,'从账号' +Convert(varchar,@intOldUserID) + '转移', @AreaID,@PlatformID, getdate()
		
		SET @sintSourceType=10003 --升級帳號
		SET @chvMemo = '从账号' +Convert(varchar,@intOldUserID) + '转移'
		SET @chvTitle = '转移账号'
		SET @chvMsgContent = '亲爱的玩家：
			恭喜你转移账号成功
                                                                                                                                  
		（如有任何疑问请联系客服）'
	END
	ELSE
	BEGIN
		--绑定赠送金币
		SET @sintSourceType=10002 --升級帳號
		SELECT @intGiveMoney = GiftMoney FROM Area.Area WITH (NOLOCK) WHERE AreaID = @AreaID
		SET @intGiveBankMoney = 0
		
		IF @intGiveMoney > 0
		BEGIN

			SET @chvMemo = '升级账号送金币'
			SET @chvTitle = '升级账号赠送金币'
			SET @chvMsgContent = '亲爱的玩家：
				恭喜你升级账号成功，系统已赠送您:'+dbo.Fn_ShowMoney(@intGiveMoney)+'金币
                                                                                                                                  
			（如有任何疑问请联系客服）'

		END
		ELSE
		BEGIN

			SET @chvMemo = '升级账号'
			SET @chvTitle = '升级账号'
			SET @chvMsgContent = '亲爱的玩家：
				恭喜你升级账号成功
                                                                                                                                  
			（如有任何疑问请联系客服）'

		END
		
	END

	--更新用户信息
	UPDATE Users.UserInfo SET Password = [REDACTED], LoginName = @chvTel, UserType = 1 WHERE UserID = @intUserID

	--升级送金币或转移金币
	IF @intGiveMoney > 0
	BEGIN

		EXEC [dbo].PrPsWeb_UpdateUserCash
			@intUserID			= @intUserID,
			@sintSourceType		= @sintSourceType,
			@lngUpdateAmount	= @intGiveMoney,
			@chvIPAddress		= @IPAddress,
			@chvMemo			= @chvMemo,
			@lngCurrentAmount	= @lngCurrentAmount	OUTPUT, --当前金币数
			@lngTaxAmount		= 0,
			@intAreaID			= @AreaID,
			@intPlatformID		= @PlatformID
	

	END

	--转移保险箱
	IF @intGiveBankMoney > 0
	BEGIN
		EXEC dbo.PrPsWeb_UpdateUserBank
			@intUserID,
			12000,		--存款操作
			@intGiveBankMoney,
			@IPAddress,
			@chvMemoBank,
			NULL,
			@chvErrMsg OUTPUT
	END

	--发送系统邮件
	EXEC PrPs_SendSysMail
				@intUserID, 
				@chvTitle,
				@chvMsgContent,
				0,
				NULL



	--增加升級紀錄
	EXEC [LOGDBLINKEDSERVER].[LogDB].[dbo].[PrPs_AddUserBindLog]
		@intUserID		= @intUserID,
		@tnyHDType		= @tnyHDType,
		@intPlatformID	= @PlatformID,
		@intAreaID		= @AreaID

	----------------------------------------------------------------------------------------------------
	COMMIT TRAN


	EXEC Club02.[dbo].[PrWeb_Split_BindingCheck]
		@intUserID = @intUserID,				--用户 ID
		@chvMsg	= ''


	SET @chvErrMsg = '操作成功:绑定账号成功'
	RETURN 1
END
GO
GRANT EXECUTE
    ON OBJECT::[dbo].[PrPs_User_BindGuest] TO [GameUser]
    AS [dbo];


GO
GRANT EXECUTE
    ON OBJECT::[dbo].[PrPs_User_BindGuest] TO [xbuser]
    AS [dbo];

