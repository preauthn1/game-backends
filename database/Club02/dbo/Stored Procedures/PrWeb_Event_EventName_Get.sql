-- =============================================
-- Author:  jason
-- Create date: 2019/05/22
-- Description: 活動 取得活動名稱
-- =============================================
CREATE PROCEDURE [dbo].[PrWeb_Event_EventName_Get]	
(
	@intEventID	INT = NULL
)
AS
BEGIN

	SET NOCOUNT ON


	SELECT	A.[EventID],
			A.EventName,
			A.[CreateTime]
	FROM [dbo].[EventInfo] AS A WITH (NOLOCK)
	WHERE
		(A.[EventID] = @intEventID OR @intEventID IS NULL)

	RETURN 1	
END
