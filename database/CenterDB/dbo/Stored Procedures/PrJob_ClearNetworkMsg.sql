-- =============================================
-- Author:  <xxx>
-- Create date: <xxx>
-- Description:	<定时清理网络消息记录>
-- =============================================
CREATE PROCEDURE [dbo].[PrJob_ClearNetworkMsg]
	
AS
BEGIN
	
	IF (select COUNT(1) FROM Log.NetworkMsgLog with(nolock))<10000
	BEGIN
		print '网络记录太少不需要清理'
		return 1
	END
	
	SELECT top 2000 * 
	INTO #Temp
	FROM Log.NetworkMsgLog
	order by Id desc
	
	truncate table Log.NetworkMsgLog
	INSERT INTO Log.NetworkMsgLog
	SELECT ServerID,MsgID from #Temp
	
	drop table #Temp
	
	
END

GO
GRANT EXECUTE
    ON OBJECT::[dbo].[PrJob_ClearNetworkMsg] TO [xbuser]
    AS [dbo];

