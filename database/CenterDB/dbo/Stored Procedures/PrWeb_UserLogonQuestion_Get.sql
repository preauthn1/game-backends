
-- =============================================
-- Author:  alan
-- Create date: 2018/3/24
-- Description:	获取用户登入階段投诉问题
-- Changelog:
--	20180410	alan		調整篩選條件, QueryType 根據問題類型(TypeSort)篩選
--	2018/06/08	Emily		#416 後台客服問題(用戶登入問題列表)處理處理人修正
--	2018/06/12	Alan		@chvPlatformIDs VARCHAR(32) change to VARCHAR(2000)
-- =============================================
CREATE PROCEDURE [dbo].[PrWeb_UserLogonQuestion_Get]	
@chvPlatformIDs		varchar(2000),		--平台ID集合
@chvQuestionType	varchar(100)=null,		--問題類型
@tnyStatus			tinyint=null,		--状态
@chvHandlePerson	varchar(32)=null,	--处理人
@dtBeginTime		datetime=null,		
@dtEndTime			datetime=null,
@intPageIndex		int=1,
@intPageSize		int=20,
@intPageCount		INT	= 0	 output
AS
BEGIN
	
	SELECT @intPageCount = COUNT(1) 
	FROM Users.QuestionLog as L with(nolock)	
	WHERE  (@tnyStatus IS NULL OR L.Status = @tnyStatus)
		AND (@chvQuestionType IS NULL OR L.QuestionType IN(select value from dbo.fn_Split(@chvQuestionType,',')))
		AND (@chvHandlePerson IS NULL OR L.HandlePerson = @chvHandlePerson)
		AND (@dtBeginTime IS NULL OR L.CreateTime >= @dtBeginTime)
		AND (@dtEndTime IS NULL OR L.CreateTime <= @dtEndTime)
		AND (QuestionType in (SELECT ID FROM Customer.QuestionType WITH (NOLOCK) WHERE TypeSort='Logon'))
	;with tab as 
	(
		select L.*,
		rowid = ROW_NUMBER() over(order by L.CreateTime desc),
		'User.Id' = '0',
		'User.Grade' = '0',
		'User.LoginName' = '0',
		'User.Area.Platform.Name'= P.Name,
		AU.[DisplayName]
		FROM Users.QuestionLog as L with(nolock)
		INNER JOIN Area.[Platform] AS P ON P.PlatformID = L.PlatformID
		LEFT JOIN [AdminSys].[Admin].[Users] AS AU WITH (NOLOCK) ON (L.HandlePerson = AU.LoginName)
		WHERE (@tnyStatus IS NULL OR L.Status = @tnyStatus)
		AND (@chvQuestionType IS NULL OR L.QuestionType IN(select value from dbo.fn_Split(@chvQuestionType,',')))
		AND (@chvHandlePerson IS NULL OR L.HandlePerson = @chvHandlePerson)
		AND (@dtBeginTime IS NULL OR L.CreateTime >= @dtBeginTime)
		AND (@dtEndTime IS NULL OR L.CreateTime <= @dtEndTime)
		AND (QuestionType in (SELECT ID FROM Customer.QuestionType WITH (NOLOCK) WHERE TypeSort='Logon'))
	)
	SELECT * FROM tab WHERE rowid between @intPageSize * (@intPageIndex -1) + 1 and @intPageSize * @intPageIndex
	return 1

END
GO
GRANT EXECUTE
    ON OBJECT::[dbo].[PrWeb_UserLogonQuestion_Get] TO [GameUser]
    AS [dbo];


GO
GRANT EXECUTE
    ON OBJECT::[dbo].[PrWeb_UserLogonQuestion_Get] TO [xbuser]
    AS [dbo];

