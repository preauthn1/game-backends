
-- =============================================
-- Author:		<harry>
-- Create date: <18/03/19>
-- Description:	<取得簡訊資訊>
--	2018/06/14	Jason		#451 後台增加短信(SMS)發送狀態查詢 新增取得vender資訊
-- =============================================
CREATE PROCEDURE [dbo].[PrNs_GetSMS_ByCodeID]
@intCodeID INT
AS
SET NOCOUNT ON


--取得短信資料
SELECT  CodeID,Vcode,Tel, Vender FROM [Notify].[Notify].[SMSVCode] WHERE CodeID = @intCodeID AND Status = 0

--標示此短信已經發送
UPDATE [Notify].[Notify].[SMSVCode] SET Status = 1 WHERE CodeID = @intCodeID


/*
SELECT CodeID,Vcode,Tel,Vender
FROM [Notify].[Notify].[SMSVCode]
WHERE CodeID = @intCodeID AND Status = 0


IF @@ROWCOUNT>0
BEGIN	
	--更新簡訊狀態
	UPDATE [Notify].[Notify].[SMSVCode]
	SET Status = 1 WHERE CodeID = @intCodeID

	SELECT CodeID,Vcode,Tel,Vender FROM [Notify].[Notify].[SMSVCode] WHERE CodeID = @intCodeID AND Status = 1
END
*/



GO
GRANT EXECUTE
    ON OBJECT::[dbo].[PrNs_GetSMS_ByCodeID] TO [xbuser]
    AS [dbo];

