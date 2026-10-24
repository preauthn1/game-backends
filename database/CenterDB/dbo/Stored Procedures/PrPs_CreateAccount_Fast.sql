-- =============================================
-- Author:  <xxx>
-- Create date: <xxx>
-- Description:	<快速登录_生成账号>
--  2018/04/27	Jason		UserID取號改為呼叫SP取得隨機數
-- =============================================
CREATE PROCEDURE [dbo].[PrPs_CreateAccount_Fast]
@tnyHDType		tinyint,                    --硬件类型
@chvHDCode		varchar(64),                --硬件码
@chvIPAddress	VARCHAR(15),				--IP地址
@intAreaID		int,						--渠道ID
@intUserID		INT OUTPUT					--用户ID
AS
SET NOCOUNT ON
DECLARE @chvLoginName VARCHAR(32),
@chvNickName VARCHAR(32),
@chvPassword VARCHAR(32),
@intFaceID INT,
@tnySex TINYINT

--生成用户名
EXEC dbo.PrPsWeb_CreateLoginName
'Fast_',
20,
@chvLoginName OUTPUT

--生成昵称
EXEC dbo.PrPsWeb_CreateNickName
2,
@chvNickName OUTPUT

--生成密码
SET @chvPassword = [REDACTED], '-', ''))

--生成头像ID
SET @intFaceID = CAST(RAND() * 16 + 1 AS INT)

--生成性别
SET @tnySex = CASE WHEN @intFaceID<9 THEN 0 ELSE 1 END


/* 直接取號  */
-----------------------------------------------------------------------------------
EXEC [dbo].[PrPs_User_ApplyNewUID_S]
	@intUserID	= @intUserID	OUTPUT

SET @intUserID	= ISNULL(@intUserID,0)
-----------------------------------------------------------------------------------


/*  创建账号  */
-----------------------------------------------------------------------------------
IF (@intUserID > 0)
BEGIN
	INSERT INTO Users.UserInfo
			( 
				[UserID],	FaceID,		LoginName,		NickName,		Password,		BankPassword,
				Sex,		HDType,		HDCode ,		UserType,		CreateTime,		CreateIPAddress ,
				LastLogonTime,	LastLogonIPAddress,		AreaID
			)
	SELECT
				@intUserID,	@intFaceID,	@chvLoginName,	@chvNickName,	@chvPassword,	dbo.Fn_EncryptPwd('888888'),
				@tnySex,	@tnyHDType,	@chvHDCode,		3,				GETDATE(),		@chvIPAddress,
				GETDATE(),		@chvIPAddress,			@intAreaID
END
ELSE
BEGIN
	RETURN -1
END

-----------------------------------------------------------------------------------

--注册公共逻辑
EXEC PrPsWeb_CreateAccount_Common
@tnyHDType,
@chvHDCode,
@chvIPAddress,
@intUserID,
1 --账号注册

		
RETURN 1

GO
GRANT EXECUTE
    ON OBJECT::[dbo].[PrPs_CreateAccount_Fast] TO [GameUser]
    AS [dbo];


GO
GRANT EXECUTE
    ON OBJECT::[dbo].[PrPs_CreateAccount_Fast] TO [xbuser]
    AS [dbo];

