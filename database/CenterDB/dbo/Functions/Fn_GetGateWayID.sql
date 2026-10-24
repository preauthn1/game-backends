-- =============================================
-- Author:  Jason
-- Create date: 2018/03/23
-- Description:	回傳渠道設定值數量
--	2018/03/27	Jason		新增PayType狀態判斷
-- =============================================
CREATE FUNCTION [dbo].[Fn_GetGateWayID]
(
	@intPayTypeID	INT
)
RETURNS @Temp_ItemCount TABLE ([ID] TINYINT)   
AS
BEGIN
	DECLARE @MinID		INT
	DECLARE @MaxID		INT
	DECLARE @MinPir		TINYINT
	DECLARE @MaxPir		TINYINT
	DECLARE @RtnID		TINYINT 

	DECLARE @Temp_GatewaySetting	TABLE (RowNo INT,ID INT,[Priority] TINYINT)

	INSERT INTO @Temp_GatewaySetting
	SELECT	DISTINCT ID,ROW_NUMBER() OVER (ORDER BY ID ASC) AS ROWNO,
			 [Priority]
	FROM (
	SELECT B.ID,B.[Priority]
	FROM [Pay].[PayType] AS A WITH (NOLOCK)
		INNER JOIN [Pay].[PayGateway] AS B WITH (NOLOCK) ON A.GWID = B.ID
	WHERE
		TypeID = @intPayTypeID
		AND [Status] = 1
	) AS A

	SELECT	@MinID = MIN(RowNo),
			@MaxID = MAX(RowNo)
	FROM @Temp_GatewaySetting

	WHILE (@MinID <= @MaxID)
	BEGIN
		SELECT	@MinPir = 1,@MaxPir	= [Priority] FROM @Temp_GatewaySetting WHERE RowNo = @MinID
		
		WHILE (@MinPir <= @MaxPir)
		BEGIN
			INSERT INTO @Temp_ItemCount ([ID]) VALUES (@MinID)

			SET @MinPir	=	@MinPir + 1
		END
		
		SET @MinID	=	@MinID + 1
	END

	RETURN
END

GO
GRANT SELECT
    ON OBJECT::[dbo].[Fn_GetGateWayID] TO [GameUser]
    AS [dbo];


GO
GRANT SELECT
    ON OBJECT::[dbo].[Fn_GetGateWayID] TO [xbuser]
    AS [dbo];

