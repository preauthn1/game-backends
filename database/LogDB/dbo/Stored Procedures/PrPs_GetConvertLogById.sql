-- =============================================
-- Author:		Jason
-- Create date: 2018/04/11
-- Description:	針對記錄編號回傳兌換紀錄給Center
-- 2018/04/17	Jason		新增回傳資料表
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
	IF EXISTS (SELECT TOP 1 1 FROM [Money].[ConvertLog] WITH (NOLOCK) WHERE [Id] = @ID)
	BEGIN
		SET @intRtnCode	=	1
	END
	ELSE
	BEGIN
		SET @intRtnCode	=	0
	END

	SELECT	TOP 1
			c.[Id],
			c.[PlatformID],
			p.[Name] AS PlatformName,
			c.[AreaID],
			c.[UserID],
			c.[Amount],
			c.[TaxAmount],
			c.[RealTaxAmount],
			c.[Status],
			c.[ConvertType],
			c.[RealName],
			c.[Account],
			c.[BankName],
			c.[CreateIP],
			c.[TransID],
			c.[TransData],
			c.[CreateTime],
			c.[LastUpdateTime],
			c.[CheckTime]	  
	FROM [Money].[ConvertLog] AS c WITH (NOLOCK)
	LEFT JOIN [CenterDB].[Area].[Platform] as p on c.PlatformID = p.PlatformID
	WHERE
		[Id] = @ID
	------------------------------------------------------

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

