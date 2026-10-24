-- =============================================
-- Author:  <xxx>
-- Create date: <xxx>
-- Description:	<联系客服>
-- Changelog:
--		20180326 alan 支援登入階段联系客服
--		20180329 Jason	增加輸入參數@intPlatformID & @intAreaID及取消取得@intPlatformID語法
--		20180428 alan	增加事先指派客服人員
--		2018/06/20	Jason		#489 用戶提交問題時, SQL後端過濾輸入的內容, 排除HTML
-- =============================================
CREATE PROCEDURE [dbo].[PrPs_User_ContactService]
@intPlatformID INT,
@intAreaID	INT,
@intUserID INT,
@chvMsg NVARCHAR(512),
@intQuestionType int,
@chvIPAddress VARCHAR(15),
@chvErrMsg varchar(128) OUTPUT
AS
SET NOCOUNT ON

DECLARE @chvLastHandler varchar(20) = '' --最後被哪位客服服務

IF @intUserID IS NULL or @intUserID = 0
BEGIN
	SET @intUserID = 0
END

IF (@intPlatformID = 0 OR @intAreaID = 0)
BEGIN
	SET @chvErrMsg = '操作失败:錯誤的操作'
	RETURN -1
END

IF @intUserID <> 0 AND EXISTS(SELECT 1 FROM Game.ForbidChatUser WHERE UserID = @intUserID)
BEGIN
	SET @chvErrMsg = '操作失败:您没有提交问题权限'
	RETURN -1
END

IF @intUserID <> 0 AND (SELECT COUNT(1) FROM Users.QuestionLog WHERE UserID = @intUserID AND DATEDIFF(DAY, CreateTime, GETDATE()) <= 1) >= 10
BEGIN
	SET @chvErrMsg = '操作失败:提交问题太频繁,请稍后再试'
	RETURN -1
END


SET @chvMsg = (SELECT [dbo].[FN_ClearHTMLTag](@chvMsg))

IF len(
	replace( 
		replace(
			 replace(@chvmsg,char(10), '') --换行
		,char(13),'') --归位
	, '　','')  --全行空白
) = 0
BEGIN
	SET @chvErrMsg = '操作失败:提交问题空白,请重新输入'
	RETURN -1
END


IF @intUserID = 0 --未知使用者, 屬於登入階段提問問題 
BEGIN
		--派選一位客服
		Declare @T1 Table (UserID int, LoginName varchar(20))
		Insert @T1 
		EXEC  [AdminSys].[dbo].[PrWeb_AvailableCS_ByPlatform]
		@intPlatformID = @intPlatformID

		select @chvLastHandler = LoginName from @T1
END
ELSE
BEGIN

		--派選一位客服
		Declare @T2 Table (UserID int, LoginName varchar(20))
		Insert @T2	
		EXEC  [AdminSys].[dbo].[PrWeb_AvailableCS_ByPlatform]
		@intPlatformID = @intPlatformID

		select @chvLastHandler = LoginName from @T2

END

INSERT INTO Users.QuestionLog
    ( UserID ,
		PlatformID,
        Question ,
		QuestionType,
        Status ,
        IPAddress,
        CreateTime,
		HandlePerson,
        HandleTime
    )
SELECT 
	@intUserID,
	@intPlatformID,
	@chvMsg,
	@intQuestionType,
	0,
	@chvIPAddress,
	GETDATE(),
	@chvLastHandler,
	GETDATE()

--更用户问题数
IF @intUserID <> 0
BEGIN
	UPDATE Users.UserInfo
	SET QuestionCount = QuestionCount+ 1
	WHERE UserID = @intUserID
END

IF @intUserID <> 0
	SET @chvErrMsg = '提交成功，请稍后在邮件中查看客服回复'
ELSE
	SET @chvErrMsg = '提交成功，客服将尽快协助处理'

RETURN 1

GO
GRANT EXECUTE
    ON OBJECT::[dbo].[PrPs_User_ContactService] TO [GameUser]
    AS [dbo];


GO
GRANT EXECUTE
    ON OBJECT::[dbo].[PrPs_User_ContactService] TO [xbuser]
    AS [dbo];

