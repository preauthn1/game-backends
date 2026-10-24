

-- =============================================
-- Author: emily
-- Create date: 2018/07/17
-- Description: 刪除廣告內容
-- =============================================
Create PROCEDURE [dbo].[PrWeb_AD_DeleteData]
@intID int

AS
BEGIN
	
	DELETE [Area].[Advertisement] 
	WHERE [Id] = @intID

END


GO
GRANT EXECUTE
    ON OBJECT::[dbo].[PrWeb_AD_DeleteData] TO [GameUser]
    AS [dbo];


GO
GRANT EXECUTE
    ON OBJECT::[dbo].[PrWeb_AD_DeleteData] TO [xbuser]
    AS [dbo];

