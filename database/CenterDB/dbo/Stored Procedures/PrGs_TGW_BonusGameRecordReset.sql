-- =============================================
-- Author:  Eddie
-- Create date: 2019/07/10
-- Description:	}GSlot-MYUserIDBonusGameRecord
-- =============================================
CREATE PROCEDURE [dbo].[PrGs_TGW_BonusGameRecordReset]
(
	@intUserID		INT
)
AS
BEGIN
SET NOCOUNT ON
	Delete from [Game].[TGW_BonusGameRecord] where ID in (Select ID from [Game].[TGW_BonusGameRecord] with(nolock) where UserID=@intUserID)
END
GO


