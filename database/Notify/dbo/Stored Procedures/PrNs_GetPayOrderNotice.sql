-- =============================================
-- Author:		alan
-- Create date: 20181219
-- Description:	提供遊戲內充值到賬計算
-- changelog:
-- =============================================
CREATE PROCEDURE [dbo].[PrNs_GetPayOrderNotice]
AS
SET NOCOUNT ON

declare @intMaxID int=0
Select @intMaxID=ISNULL(Max(ID),0) from Notify.PayFinishOrder 

DECLARE	@Temp_UserID TABLE (UserID INT, Amount BIGINT, TimeSeq BIGINT)
DELETE FROM Notify.PayFinishOrder OUTPUT DELETED.UserID, DELETED.Amount, DELETED.TimeSeq INTO @Temp_UserID WHERE ID <= @intMaxID


select TimeSeq, T.UserID, ISNULL(G.ServerID,0) as ServerID, T.Amount as Amount
from @Temp_UserID T 
left join CenterDB.Game.GameUserList G with (nolock) on T.UserID=G.UserID

--group by T.UserID,G.ServerID

RETURN 1

GO
GRANT EXECUTE
    ON OBJECT::[dbo].[PrNs_GetPayOrderNotice] TO [xbuser]
    AS [dbo];

