
-- =============================================
-- Author: alan
-- Create date: 20180824
-- Description: 客服撤回回覆的訊息
-- =============================================
CREATE PROCEDURE [dbo].[PrWeb_UserQuestion_ReplyCancel]
@intQuestionID		INT,
@intRow int
AS
SET NOCOUNT ON
DECLARE @intStatus tinyint = 1
DECLARE @Updmsg NVARCHAR(max)
DECLARE @chvContent nvarchar(max)

SELECT @chvContent = ReplyContent from users.QuestionLog where id = @intQuestionID

SELECT @Updmsg = COALESCE(@Updmsg + char(13) + char(10), '') + value FROM dbo.fn_Split2(@chvContent,char(13)+char(10)) where id <> @intRow

IF LEN(@Updmsg) = 0
BEGIN
	SET @Updmsg = null
	SET @intStatus = 0
END


--问题回复更新
UPDATE Users.QuestionLog
	SET HandleTime = GETDATE()
	,Status = @intStatus
	,ReplyContent = @Updmsg
WHERE Id = @intQuestionID

RETURN 1


GO
GRANT EXECUTE
    ON OBJECT::[dbo].[PrWeb_UserQuestion_ReplyCancel] TO [GameUser]
    AS [dbo];


GO
GRANT EXECUTE
    ON OBJECT::[dbo].[PrWeb_UserQuestion_ReplyCancel] TO [xbuser]
    AS [dbo];

