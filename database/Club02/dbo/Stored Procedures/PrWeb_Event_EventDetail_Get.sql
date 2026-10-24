-- =============================================
-- Author:  jason
-- Create date: 2019/05/22
-- Description: 活動 取得活動內容
-- =============================================
CREATE PROCEDURE [dbo].[PrWeb_Event_EventDetail_Get]	
(
	@intAdminID int,
	@intEventID	INT = NULL,
	@intPlatformID INT = NULL,
	@intAreaID INT = NULL
)
AS
BEGIN

	SET NOCOUNT ON

	/*  用户平台  */
	declare @temp_platformid table (Platformid int)
	insert into @temp_platformid
	select platformid
	from [dbo].[AgentPlatformRelations] with (nolock)
	where
		[AdminID] = @intAdminID	
	
	SELECT	B.[PlatformID],
			B.[AreaID],
			A.[EventID],
			A.EventName,
			A.[CreateTime],
			B.EventStartDate,
			B.EventEndDate,
			B.Status,
			C.LngValue1,
			C.LngValue2,
			C.LngValue3,
			C.LngValue4,
			C.LngValue5,
			C.LngValue6,
			C.LngValue7,
			C.LngValue8,
			C.LngValue9,
			C.LngValue10
	FROM [dbo].[EventInfo] AS A WITH (NOLOCK)
		INNER JOIN [dbo].[EventPlatformInfo] AS B WITH (NOLOCK) ON A.EventID = B.EventID
		INNER JOIN [dbo].[EventContent] AS C WITH (NOLOCK) ON B.EventID = C.EventID AND B.PlatformID = C.PlatformID AND B.AreaID = C.AreaID
		JOIN @temp_platformid AS P ON B.PlatformID = P.Platformid
	WHERE
		(A.[EventID] = @intEventID OR @intEventID IS NULL)
		AND (B.[PlatformID] = @intPlatformID OR @intPlatformID IS NULL)
		AND (B.[AreaID] = @intAreaID OR @intAreaID IS NULL)

	RETURN 1	
END
