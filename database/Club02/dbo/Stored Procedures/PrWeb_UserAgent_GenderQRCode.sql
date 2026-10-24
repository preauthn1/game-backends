-- =============================================
-- Author:  jason
-- Create date: 2019/04/11
-- Description:	回传QR CODE
-- =============================================
CREATE PROCEDURE [dbo].[PrWeb_UserAgent_GenderQRCode]
(
	@chvCode	VARCHAR(8)	OUTPUT
)
AS
BEGIN
	SET NOCOUNT ON

	declare @lenth int = 8

	SELECT @chvCode = coalesce(@chvCode, '') + n
	FROM (	SELECT top (@lenth) CHAR(number) n FROM master..spt_values
			WHERE type = 'P' AND 
				(number between ascii(0) and ascii(9)
				or number between ascii('A') and ascii('Z')
				or number between ascii('a') and ascii('z'))
				ORDER BY newid()
		) a

	SET @chvCode	=	UPPER(@chvCode)
END



