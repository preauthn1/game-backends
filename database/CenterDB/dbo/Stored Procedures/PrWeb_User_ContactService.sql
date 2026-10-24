
-- =============================================
-- Author:  <xxx>
-- Create date: <xxx>
-- Description:	<联系客服>
-- Changelog:
--		20180326 alan 支援登入階段联系客服
--		20180329 Jason	增加輸入參數@intPlatformID & @intAreaID及取消取得@intPlatformID語法
--		20180428 alan	增加事先指派客服人員
--		2018/06/20	Jason		#489 用戶提交問題時, SQL後端過濾輸入的內容, 排除HTML
--		2018/08/17	alan		#679 增加email欄位
--		2019/06/15	CCC＆Bob	客服分流
-- =============================================
CREATE PROCEDURE [dbo].[PrWeb_User_ContactService]
@intPlatformID INT,
@intAreaID	INT,
@intUserID INT,
@chvMsg NVARCHAR(512),
@chvUserEmail varchar(50) = null,
@intQuestionType int,
@chvIPAddress VARCHAR(15),
@chvImageUrl VARCHAR(250) = null,
@chvErrMsg varchar(128) OUTPUT
AS
SET NOCOUNT ON

DECLARE @chvLastHandler varchar(20) = '' --最後被哪位客服服務
DECLARE @intAvailableAdminID int
DECLARE @intLastHandler int 		--最後被哪位客服服務

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


SET @chvMsg = (SELECT [dbo].[FN_ClearHTMLTag](@chvMsg))

IF @chvMsg = ''
BEGIN	
	RETURN 1
END

IF @intUserID = 0 --未知使用者, 屬於登入階段提問問題 
BEGIN
		--派選一位客服
		Declare @T1 Table (UserID int, LoginName varchar(20))
		Insert @T1 Exec [AdminSys].[dbo].PrWeb_AvailableCS_Get
		select @chvLastHandler = LoginName from @T1
END
ELSE
BEGIN

		--派選一位客服
		Declare @T2 Table (UserID int, LoginName varchar(20))
		Insert @T2 Exec [AdminSys].[dbo].PrWeb_AvailableCS_Get
		select @chvLastHandler = LoginName from @T2
		-------------------------------------------------------------------
		
	  	  --找尋最後對話紀錄 
			SELECT top 1 @intLastHandler= Operator FROM [CenterDB].[Users].[QuestionLog]
			where CreateTime >DATEADD(minute, -120, GETDATE()) and  UserID=@intUserID
			order by Id DESC

		;with Temp_P AS
		(
		--找尋該平台營運商
		SELECT Company
		  FROM CenterDB.Area.[Platform]
		 WHERE PlatformID = @intPlatformID
		)
		, Temp_Company AS
		(
		--找尋當前上班的客服
		Select DISTINCT
			   P.Company, C.UserID
		  From [CenterDB].[Users].[CSOnBoard] C
		  JOIN CenterDB.Area.PlatformAdminUser as A
			ON A.AdminID = C.UserID and C.Status=0
		  JOIN CenterDB.Area.[Platform] as P
			ON A.PlatformID = P.PlatformID
		  JOIN Temp_P G
			ON  P.Company = G.Company


		), Temp_AllAvailableCS as 
		(
		  --抽選客服
		  SELECT *, NEWID() as id
		  FROM Temp_Company
		)
		   
		  SELECT TOP 1  @intAvailableAdminID=UserID FROM Temp_AllAvailableCS WHERE @intLastHandler is null OR UserID = @intLastHandler ORDER BY id

		  select top 1 @chvLastHandler=LoginName from  [AdminSys].[Admin].[Users]  with (nolock) where UserID=@intAvailableAdminID

		
		--------------------------------------------------------------------

END




;


INSERT INTO Users.QuestionLog
    ( UserID ,
		PlatformID,
        Question ,
		QuestionType,
        Status ,
        IPAddress,
        CreateTime,
		HandlePerson,
        HandleTime,
		UploadImage,
		Operator
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
	GETDATE(),
	ISNULL(@chvImageUrl,''),
	@intAvailableAdminID

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
    ON OBJECT::[dbo].[PrWeb_User_ContactService] TO [GameUser]
    AS [dbo];


GO
GRANT EXECUTE
    ON OBJECT::[dbo].[PrWeb_User_ContactService] TO [xbuser]
    AS [dbo];

