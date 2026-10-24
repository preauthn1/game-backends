-- =============================================
-- Author: Eddie
-- Create date: 2019/07/17
-- Description: Rxʤe
-- =============================================
Create PROCEDURE [dbo].[PrWeb_Campaign_DeleteData]
@intID int

AS
BEGIN
	
	DELETE [Area].[Campaign] 
	WHERE [Id] = @intID

END

GO


