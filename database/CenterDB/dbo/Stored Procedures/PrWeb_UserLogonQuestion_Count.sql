-- =============================================
-- Author:  alan
-- Create date: 2018/3/23
-- Description:	查詢登入階段提出問題的筆數
-- Changelog:
--		20180410 alan 調整篩選條件, QueryType 根據問題類型(TypeSort)篩選
-- =============================================
CREATE PROCEDURE [dbo].[PrWeb_UserLogonQuestion_Count]
	@chvPlatformIDs	varchar(4000),	--平台ID 
	@tintStatus		tinyint,		--問題狀態 0未處理 1已回覆
	@intCount	int output			--數量
AS
BEGIN

	SELECT @intCount = COUNT(1)
	FROM Users.QuestionLog as L with(nolock)
	WHERE	L.Status = @tintStatus
	AND L.CreateTime > dateadd(dd,-1,GETDATE())
	AND L.QuestionType in (SELECT ID FROM Customer.QuestionType WITH (NOLOCK) WHERE TypeSort='Logon')
	AND L.PlatformID in (select value from dbo.fn_Split(@chvPlatformIDs,','))
END
GO
GRANT EXECUTE
    ON OBJECT::[dbo].[PrWeb_UserLogonQuestion_Count] TO [GameUser]
    AS [dbo];


GO
GRANT EXECUTE
    ON OBJECT::[dbo].[PrWeb_UserLogonQuestion_Count] TO [xbuser]
    AS [dbo];

