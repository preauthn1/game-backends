


-- =============================================
-- Author:  <xxx>
-- Create date: <xxx>
-- Description:	<更新域名设定>
-- =============================================
CREATE PROCEDURE [dbo].[PrWeb_DomainConfig_Update]
@chvPayment			VARCHAR(2048),			
@chvDLUpdate		VARCHAR(2048),			
@chvClub			VARCHAR(2048),			
@chvCustomer		VARCHAR(2048),
@intCompany			INT	
AS
BEGIN
	--更新订单状态
	UPDATE ServerInfo.DomainConfig
	SET 
	[Payment] = @chvPayment
	,[DLUpdate] = @chvDLUpdate
	,[Club] = @chvClub
	,[Customer] = @chvCustomer
	WHERE Company = @intCompany
	RETURN 1
END




GO
GRANT EXECUTE
    ON OBJECT::[dbo].[PrWeb_DomainConfig_Update] TO [GameUser]
    AS [dbo];


GO
GRANT EXECUTE
    ON OBJECT::[dbo].[PrWeb_DomainConfig_Update] TO [xbuser]
    AS [dbo];

