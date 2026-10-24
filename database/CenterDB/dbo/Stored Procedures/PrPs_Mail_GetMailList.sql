-- =============================================
-- Author:  <xxx>
-- Create date: <xxx>
-- Description:	<获取用户邮件列表>
-- =============================================
CREATE PROCEDURE [dbo].[PrPs_Mail_GetMailList]
    @intUserID INT							--用户ID    
AS
    SET NOCOUNT ON;

    SELECT  substring(m.Title,1,15) + case when len(m.Title)>15 then '...' else '' end  AS title,
            m.MsgID AS id ,												--消息ID
            CASE WHEN mu.Status = 0 THEN 0 ELSE 1 END  AS readtype,		--状态 0未读 1已读
            CONVERT(VARCHAR(16), m.CreateTime, 120) AS SendTime			--发送时间
    FROM    Message.MessageList m WITH(NOLOCK),
            Message.ReceiveUser mu WITH(NOLOCK)
    WHERE   m.MsgID = mu.MsgID
            AND mu.UserID = @intUserID
            AND mu.Status <> 2
            AND m.CreateTime > dbo.Fn_EmailStartTime()
    ORDER BY readtype ASC ,
            SendTime DESC;

    RETURN 1;

GO
GRANT EXECUTE
    ON OBJECT::[dbo].[PrPs_Mail_GetMailList] TO [GameUser]
    AS [dbo];


GO
GRANT EXECUTE
    ON OBJECT::[dbo].[PrPs_Mail_GetMailList] TO [xbuser]
    AS [dbo];

