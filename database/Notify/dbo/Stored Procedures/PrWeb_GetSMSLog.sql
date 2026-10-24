-- =============================================
-- Author:  Jason
-- Create date: 2018/06/14
-- Description:	#451 後台增加短信(SMS)發送狀態查詢
-- =============================================
CREATE PROCEDURE [dbo].[PrWeb_GetSMSLog]
(
	@intAdminID			int,
	@chvTel				VARCHAR(11) = NULL,
	@intUserID			INT = NULL,
	@dtBeginTime		DATETIME = NULL,
	@dtEndTime			DATETIME = NULL,
	@intPageIndex		INT = 1,
	@intPageSize		INT = 20,
	@intPageCount		INT	OUTPUT	--记录总数		
)
AS
BEGIN

	SET NOCOUNT ON

	DECLARE @BeginTime	DATETIME = DATEADD(DAY,0,DATEDIFF(DAY,0,ISNULL(@dtBeginTime,GETDATE()-7)))
	DECLARE @EndTime	DATETIME = GETDATE()

	--渠道篩選
	DECLARE @Temp_AreaID TABLE (AreaID	INT)
	INSERT INTO @Temp_AreaID Select AreaID From CenterDB.Area.AreaAdminUser where AdminID=@intAdminID

		

	/*  回傳資料  */
	-------------------------------------------------------------------------------------
	SELECT	@intPageCount = COUNT(1)
	FROM (
			SELECT	id=1
			FROM [Notify].[SMSVCode] S WITH (NOLOCK)
			left join CenterDB.Users.UserInfo U with (nolock) on S.UserID=U.UserID
			WHERE
				(ISNULL(@chvTel,S.[Tel]) = S.[Tel])
				AND (ISNULL(@intUserID,S.[UserID]) = S.[UserID])
				AND S.[CreateTime] >= @BeginTime
				AND S.[CreateTime] < @EndTime
				AND U.AreaID in (select areaid from @Temp_AreaID)

		) AS A
	;WITH CTE
	AS
	(
		SELECT	ROW_NUMBER() OVER (ORDER BY [CreateTime] DESC) AS rowNum,
				[CodeID],
				[Tel],
				[VCode],
				[CreateTime],
				[LastSendTime],
				[CodeType],
				[Status],
				[IPAddress],
				[UpdateTime],
				[ExpireTime],
				[UserID],
				[SendResult],
				[Vender]
		FROM (
				SELECT	ROW_NUMBER() OVER (ORDER BY S.[CreateTime] DESC) AS rowno,
				S.[CodeID],
				S.[Tel],
				S.[VCode],
				S.[CreateTime],
				S.[LastSendTime],
				S.[CodeType],
				S.[Status],
				S.[IPAddress],
				S.[UpdateTime],
				S.[ExpireTime],
				S.[UserID],
				S.[SendResult],
				S.[Vender]
				FROM [Notify].[SMSVCode] S WITH (NOLOCK)
				left join CenterDB.Users.UserInfo U with (nolock) on S.UserID=U.UserID
				WHERE
					(ISNULL(@chvTel, S.[Tel]) = S.[Tel])
					AND (ISNULL(@intUserID, S.[UserID]) = S.[UserID])
					AND S.[CreateTime] >= @BeginTime
					AND S.[CreateTime] <= @EndTime
					AND U.AreaID in (select areaid from @Temp_AreaID)
			) AS A

	)
	SELECT * FROM CTE WHERE rowNum BETWEEN @intPageSize * (@intPageIndex -1) + 1 AND @intPageSize * @intPageIndex
	-------------------------------------------------------------------------------------



END

GO
GRANT EXECUTE
    ON OBJECT::[dbo].[PrWeb_GetSMSLog] TO [xbuser]
    AS [dbo];

