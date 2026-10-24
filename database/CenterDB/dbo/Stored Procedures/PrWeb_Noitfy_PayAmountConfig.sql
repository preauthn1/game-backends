-- =============================================
-- Author:  <xxx>
-- Create date: <xxx>
-- Description:	<通知充值>
-- =============================================
CREATE PROCEDURE [dbo].[PrWeb_Noitfy_PayAmountConfig]
	@intPlatformID		INT				--平台ID
AS
BEGIN
	
	--充值到账通知
		INSERT INTO NOTIFYLINKEDSERVER.Notify.Notify.Attr
				( UserID ,
				  TypeID ,
				  sParam ,
				  Status ,
				  CreateTime ,
				  LastPostTime
				)
		SELECT 0,
			3,
			cast(PlatformID as varchar)+'_'+PayAmountConfig,
			0,
			GETDATE(),
			GETDATE()
		FROM Area.[Platform]
		where PlatformID = @intPlatformID
	
END

GO
GRANT EXECUTE
    ON OBJECT::[dbo].[PrWeb_Noitfy_PayAmountConfig] TO [GameUser]
    AS [dbo];


GO
GRANT EXECUTE
    ON OBJECT::[dbo].[PrWeb_Noitfy_PayAmountConfig] TO [xbuser]
    AS [dbo];

