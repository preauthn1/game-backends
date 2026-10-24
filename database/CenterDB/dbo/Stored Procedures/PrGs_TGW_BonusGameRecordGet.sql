-- =============================================
-- Author:  Eddie
-- Create date: 2019/07/10
-- Description:	}GSlot-oBonusGameRecord
-- =============================================
CREATE PROCEDURE [dbo].[PrGs_TGW_BonusGameRecordGet]
(
	@intUserID		INT
)
AS
BEGIN
SET NOCOUNT ON
	SELECT [ID],[UserID],[TurnID],[Location],[Money],[BonusID]
	FROM [CenterDB].[Game].[TGW_BonusGameRecord] WITH(NOLOCK)
	WHERE [USERID] = @intUserID
END
GO


