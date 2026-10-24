
-- =============================================
-- Author:		Jason
-- Create date: 2019/05/24
-- Description:	SlotGameHistoryLog取號
-- =============================================
CREATE PROCEDURE [dbo].[PrWeb_Slot_GetSequence]
(
	@intLogID INT OUTPUT
)
AS
BEGIN
	SET NOCOUNT ON

	
	BEGIN TRAN
		INSERT [dbo].[SlotGameHistorySeqence] WITH (TABLOCKX) DEFAULT VALUES;
    ROLLBACK TRAN

    SELECT @intLogID = SCOPE_IDENTITY()

	RETURN
END

GO
GRANT EXECUTE
    ON OBJECT::[dbo].[PrWeb_Slot_GetSequence] TO [GameUser]
    AS [dbo];

