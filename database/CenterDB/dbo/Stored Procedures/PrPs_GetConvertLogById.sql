-- =============================================
-- Author:		Jason
-- Create date: 2018/04/11
-- Description:	針對記錄編號回傳兌換紀錄給Center
-- =============================================
CREATE PROCEDURE [dbo].[PrPs_GetConvertLogById]
(
	@intID		INT,
	@intRtnCode	INT = 0		OUTPUT
)
AS
BEGIN
	SET NOCOUNT ON

	
	DECLARE @ID		INT = @intID
	
	/*  回傳紀錄  */
	------------------------------------------------------
	EXEC [LOGDBLINKEDSERVER].[LogDB].[dbo].[PrPs_GetConvertLogById]
		@intID = @ID,
		@intRtnCode = @intRtnCode	OUTPUT
	------------------------------------------------------

	SET @intRtnCode	= ISNULL(@intRtnCode,0)

	RETURN
END
GO
GRANT EXECUTE
    ON OBJECT::[dbo].[PrPs_GetConvertLogById] TO [GameUser]
    AS [dbo];


GO
GRANT EXECUTE
    ON OBJECT::[dbo].[PrPs_GetConvertLogById] TO [xbuser]
    AS [dbo];

