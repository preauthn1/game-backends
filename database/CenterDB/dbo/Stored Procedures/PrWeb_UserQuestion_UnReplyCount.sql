-- =============================================
-- Author:  <xxx>
-- Create date: <xxx>
-- Description:	<查询未处理问题数>
-- =============================================
CREATE PROCEDURE [dbo].[PrWeb_UserQuestion_UnReplyCount]

	@chvPlatformIDs	varchar(2000)=null,	--平台ID
	@chvHandlePerson	VARCHAR(32) = NULL,	--操作人員
	@intReplyCount	int output			--未处理数
AS
BEGIN
	

	SELECT @intReplyCount = COUNT(1)
	FROM Users.QuestionLog as L with(nolock)
	INNER JOIN Users.UserInfo as U WITH(NOLOCK) ON L.UserID=U.UserID
	INNER JOIN Area.Area as A ON U.AreaID = A.AreaID	
	WHERE (A.PlatformID in (SELECT CAST(value as int) FROM dbo.fn_Split(@chvPlatformIDs,',')))
	AND ISNULL(@chvHandlePerson,L.[HandlePerson]) = L.[HandlePerson]
	AND	L.Status = 0 					
	AND L.CreateTime>dateadd(dd,-1,GETDATE())

END
GO
GRANT EXECUTE
    ON OBJECT::[dbo].[PrWeb_UserQuestion_UnReplyCount] TO [GameUser]
    AS [dbo];


GO
GRANT EXECUTE
    ON OBJECT::[dbo].[PrWeb_UserQuestion_UnReplyCount] TO [xbuser]
    AS [dbo];

