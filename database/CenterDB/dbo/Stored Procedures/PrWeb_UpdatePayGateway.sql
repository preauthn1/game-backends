
-- =============================================
-- Author:  <xxx>
-- Create date: <xxx>
-- Description:	<更新PayGateway>
-- =============================================
CREATE PROCEDURE [dbo].[PrWeb_UpdatePayGateway]
@intGWID INT,							--第三方厂商
@tnyPriority tinyint					    --权重

AS
BEGIN
	--更新[PayGateway]
	UPDATE [CenterDB].[Pay].[PayGateway]
	SET [Priority] = @tnyPriority
	WHERE [ID] = @intGWID

	RETURN 1
END


GO
GRANT EXECUTE
    ON OBJECT::[dbo].[PrWeb_UpdatePayGateway] TO [GameUser]
    AS [dbo];


GO
GRANT EXECUTE
    ON OBJECT::[dbo].[PrWeb_UpdatePayGateway] TO [xbuser]
    AS [dbo];

