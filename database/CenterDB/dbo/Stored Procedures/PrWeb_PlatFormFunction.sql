-- =============================================
-- Author:  Jason
-- Create date: 2018/03/22
-- Description:	後台使用功能
-- =============================================
CREATE PROCEDURE [dbo].[PrWeb_PlatFormFunction]
(
	@sintTypeID		SMALLINT = 1	--後台使用功能編號
)
AS
BEGIN
	SET NOCOUNT ON

	/*  回傳功能列表  */
	----------------------------------------------------------------------
	SELECT	[TypeID],
			[FuncID],
			[FuncName]
			--[CreateTime],
			--[Memo]
	FROM [Stat].[WebFunctionType] WITH (NOLOCK)
	WHERE
		[TypeID] = @sintTypeID
	----------------------------------------------------------------------

	RETURN 1
END
GO
GRANT EXECUTE
    ON OBJECT::[dbo].[PrWeb_PlatFormFunction] TO [GameUser]
    AS [dbo];


GO
GRANT EXECUTE
    ON OBJECT::[dbo].[PrWeb_PlatFormFunction] TO [xbuser]
    AS [dbo];

