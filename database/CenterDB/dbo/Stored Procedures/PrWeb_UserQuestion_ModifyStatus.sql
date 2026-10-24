-- =============================================
-- Author:  <xxx>
-- Create date: <xxx>
-- Description:	<修改问题状态>
-- 2018/06/15  Emily  增加參數處理人
-- =============================================
CREATE PROCEDURE [dbo].[PrWeb_UserQuestion_ModifyStatus]
	@intQuestionID		int,		--问题ID
	@tintStatus			tinyint,	--问题状态
	@chvHandlePerson    varchar(64) --處理人
AS
BEGIN
	
	UPDATE Users.QuestionLog
	SET Status = @tintStatus, 
	    HandlePerson = @chvHandlePerson,
		HandleTime = GETDATE()
	WHERE Id = @intQuestionID AND Status =0

END

GO
GRANT EXECUTE
    ON OBJECT::[dbo].[PrWeb_UserQuestion_ModifyStatus] TO [GameUser]
    AS [dbo];


GO
GRANT EXECUTE
    ON OBJECT::[dbo].[PrWeb_UserQuestion_ModifyStatus] TO [xbuser]
    AS [dbo];

