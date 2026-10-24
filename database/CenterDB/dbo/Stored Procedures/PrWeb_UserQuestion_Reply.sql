
-- =============================================
-- Author:  <xxx>
-- Create date: <xxx>
-- Description:	<回复问题>
-- Changelog:
--		2018/3/25 alan	修正content內容未寫入ReplyContent欄位 
--		2018/5/18	alan 把這個用戶提問的之前問題都變更成已經處理
--		2018/6/12	alan 調整為只變更當筆訊息為已經處理, 不異動其他問題
--		2018/6/24	bob  将未处理直接改为已处理 
-- =============================================
CREATE PROCEDURE [dbo].[PrWeb_UserQuestion_Reply]
@intQuestionID		INT,
@chvHandlePerson	varchar(64),
@intUserID INT,
@chvTitle VARCHAR(128),
@chvContent NVARCHAR(2048)
AS
SET NOCOUNT ON
DECLARE @intMsgID INT


--问题回复更新
UPDATE Users.QuestionLog
SET HandleTime = GETDATE()
,Status = 1
,HandlePerson=@chvHandlePerson
,ReplyContent = @chvContent
WHERE Id = @intQuestionID

--将未处理直接改为已处理 
update Users.QuestionLog 
SET HandleTime = GETDATE()
,Status = 1
,HandlePerson=@chvHandlePerson
where UserID=@intUserID and Status != 1


--AND CreateTime> DATEADD(WW,-1,GETDATE())	--只能回复最近一周的问题


EXEC PrPs_SendSysMail
	@intUserID,
	@chvTitle,
    @chvContent,
	0,
    --NULL,
    1


--新增客戶端提醒通知
IF @intUserID <> 0
Begin
	Update [Notify].[Notify].ServiceTalk SET UnreadCount = UnreadCount + 1 WHERE UserID=@intUserID
	IF @@ROWCOUNT=0
		Insert into [Notify].[Notify].ServiceTalk(UserID, UnreadCount) Values(@intUserID,1)

	Insert into [Notify].[Notify].Attr (UserID, TypeID, Param, [Status], CreateTime, LastPostTime)
		Select @intUserID, 4, UnreadCount, 0, getdate(), getdate() from [Notify].[Notify].ServiceTalk where UserID = @intUserID
End

RETURN 1
GO
GRANT EXECUTE
    ON OBJECT::[dbo].[PrWeb_UserQuestion_Reply] TO [GameUser]
    AS [dbo];


GO
GRANT EXECUTE
    ON OBJECT::[dbo].[PrWeb_UserQuestion_Reply] TO [xbuser]
    AS [dbo];

