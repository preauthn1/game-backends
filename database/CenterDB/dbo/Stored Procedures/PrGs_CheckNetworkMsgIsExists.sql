-- =============================================
-- Author:  <xxx>
-- Create date: <xxx>
-- Description:	<检查网络消息是否已经处理>
-- =============================================
CREATE PROCEDURE [dbo].[PrGs_CheckNetworkMsgIsExists]
	@intServerID		int,
	@intMsgID			int
AS
BEGIN		

INSERT INTO Log.NetworkMsgLog(ServerID,MsgID)
SELECT @intServerID,@intMsgID
WHERE NOT EXISTS(SELECT 1 FROM Log.NetworkMsgLog WHERE ServerID=@intServerID AND MsgID=@intMsgID)

if @@ROWCOUNT > 0
BEGIN
	RETURN 0
END
ELSE 	
BEGIN	
	RETURN 1		
END
END

GO
GRANT EXECUTE
    ON OBJECT::[dbo].[PrGs_CheckNetworkMsgIsExists] TO [GameUser]
    AS [dbo];


GO
GRANT EXECUTE
    ON OBJECT::[dbo].[PrGs_CheckNetworkMsgIsExists] TO [xbuser]
    AS [dbo];

