-- =============================================
-- Author:		<xxx>
-- Create date: <15/09/29>
-- Description:	<清理通知数据>
--	2018/06/13	Jason		#451 後台增加短信(SMS)發送狀態查詢 排程每天清理超過30天記錄
-- =============================================
CREATE PROCEDURE [dbo].[PrJob_ClearNotify]
AS
SET NOCOUNT ON
TRUNCATE TABLE Notify.Attr
--TRUNCATE TABLE Notify.EmailQueue //disable by alan, because EmailQueue table not exist

	
	SELECT	[Tel],[VCode],[Status],[CreateTime],[LastSendTime],IPAddress,CodeType,UpdateTime
	INTO #T
	FROM [Notify].[SMSVCode] WITH (NOLOCK)
	WHERE
		CreateTime >= DATEADD(DAY,-30,DATEDIFF(DAY,0,GETDATE()))

	TRUNCATE TABLE Notify.SMSVCode
	INSERT INTO [Notify].[SMSVCode]
	SELECT	[Tel],[VCode],[Status],[CreateTime],[LastSendTime],IPAddress,CodeType,UpdateTime
	FROM #T

RETURN 1

GO
GRANT EXECUTE
    ON OBJECT::[dbo].[PrJob_ClearNotify] TO [xbuser]
    AS [dbo];

