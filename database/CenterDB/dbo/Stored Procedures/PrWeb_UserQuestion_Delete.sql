-- =============================================
-- Author:  <xxx>
-- Create date: <xxx>
-- Description:	<修改问题状态>
-- =============================================
CREATE PROCEDURE [dbo].[PrWeb_UserQuestion_Delete]
	@intQuestionID		int		--问题ID	
AS
BEGIN	
	--删除问题,只处理最近一周
	DELETE FROM Users.QuestionLog WHERE Id = @intQuestionID AND CreateTime>DATEADD(WW,-1,GETDATE())			
	
END

GO
GRANT EXECUTE
    ON OBJECT::[dbo].[PrWeb_UserQuestion_Delete] TO [GameUser]
    AS [dbo];


GO
GRANT EXECUTE
    ON OBJECT::[dbo].[PrWeb_UserQuestion_Delete] TO [xbuser]
    AS [dbo];

