-- =============================================
-- Author:  jason
-- Create date: 20190322
-- Description:	提供伺服器AreaID對應公司
-- =============================================
CREATE PROCEDURE [dbo].[PrGs_GetCompanyByAreaID]
(
	@intAreaID	INT
)
AS
BEGIN
	SET NOCOUNT ON

	SELECT	AreaID,
			[Company]
	FROM [Area].[AreaCompanyRelations] WITH (NOLOCK)
	WHERE
		AreaID = @intAreaID

	RETURN 1
END

GO
GRANT EXECUTE
    ON OBJECT::[dbo].[PrGs_GetCompanyByAreaID] TO [GameUser]
    AS [dbo];

