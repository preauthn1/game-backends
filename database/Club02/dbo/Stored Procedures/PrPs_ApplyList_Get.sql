





-- =============================================
-- Author:  alan
-- Create date: 2018/08/8
-- Description: 提供一级代理取得下线玩家申请代理的清单
-- =============================================
CREATE PROCEDURE [dbo].[PrPs_ApplyList_Get]
	@intUserID			INT
AS
BEGIN
 
    --取得代理者的uniqID
	DECLARE @UID uniqueidentifier
	Select @UID = ID from [dbo].[AgentUsers] With (nolock) Where UserID = @intUserID

	--取得代理申请清单(status 0待審核 1審核通過 2审核未通过)
	Select UserID,LoginName From dbo.AgentApply with (nolock) WHERE ParentID = @UID AND [Status] = 0


END


