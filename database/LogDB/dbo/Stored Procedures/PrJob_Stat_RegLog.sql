-- =============================================
-- Author:		Jason
-- Create date: 2018/03/30
-- Description:	統計使用者註冊 (機制改寫)
--	2018/04/02	Jason		優化統計語法
-- =============================================
CREATE PROCEDURE [dbo].[PrJob_Stat_RegLog]
(
	@dtStatTime DATETIME = NULL
)
AS
BEGIN
	SET NOCOUNT ON

	SET @dtStatTime = ISNULL(@dtStatTime,GETDATE())

	DECLARE @LogStartTime			DATETIME
	DECLARE @LogEndTime				DATETIME

	DECLARE @Temp_UserRegLog	TABLE ([CreateTime] DATETIME,[PlatformID] INT,[AreaID] INT,[TotalRegCount] INT,[IOSRegCount] INT,[AndroidRegCount] INT,[PCRegCount] INT)


	IF (DATEPART(HOUR,@dtStatTime) = 0) AND (DATEPART(MINUTE,@dtStatTime) < 10)
	BEGIN
		SET @LogStartTime		= DATEADD(DAY,-1,DATEDIFF(DAY,0,@dtStatTime))
		SET @LogEndTime			= DATEADD(DAY,0,DATEDIFF(DAY,0,@dtStatTime))
	END
	ELSE
	BEGIN
		SET @LogStartTime		= DATEADD(DAY,0,DATEDIFF(DAY,0,@dtStatTime))
		SET @LogEndTime			= DATEADD(DAY,1,DATEDIFF(DAY,0,@dtStatTime))
	END


	/*  取註冊紀錄  */
	------------------------------------------------------
	INSERT INTO @Temp_UserRegLog
	SELECT	DISTINCT *
	FROM (
			SELECT	CONVERT(VARCHAR(10),[CreateTime],23) AS [CreateTime],
					ISNULL([PlatformID],0) AS [PlatformID],
					ISNULL([AreaID],0) AS [AreaID],
					COUNT(1) AS [TotalRegCount],
					SUM(CASE WHEN [HDType] = 3 THEN 1 ELSE 0 END) AS [IOSRegCount],  --註冊人數(等同 新用戶登入IOS人數)
					SUM(CASE WHEN [HDType] = 2 THEN 1 ELSE 0 END) AS [AndroidRegCount], --註冊人數(等同 新用戶登入Android人數)
					SUM(CASE WHEN [HDType] = 1 THEN 1 ELSE 0 END) AS [PCRegCount] --註冊人數(等同 新用戶登入PC人數)
			FROM [Users].[UserRegLog] WITH (NOLOCK)
			WHERE
				[CreateTime] >= @LogStartTime
				AND [CreateTime] < @LogEndTime
			GROUP BY ROLLUP(CONVERT(VARCHAR(10),[CreateTime],23),[PlatformID],[AreaID])
		) AS A
	WHERE
		[CreateTime] IS NOT NULL
	------------------------------------------------------

	
	/*  取登入APP紀錄  */
	------------------------------------------------------
	SELECT CONVERT(VARCHAR(10),L.CreateTime,23) AS [CreateTime],

	ISNULL(L.PlatformID,0) AS [PlatformID],
	ISNULL(L.AreaID,0) AS [AreaID],
	COUNT(distinct L.UserID) AS [LoginAppUser],

	COUNT(distinct CASE WHEN L.HDType = 3 THEN L.UserID ELSE null END) AS [LoginAppIOSUser],
	COUNT(distinct CASE WHEN L.HDType = 2 THEN L.UserID ELSE null END) AS [LoginAppAndroidUser],
	COUNT(distinct CASE WHEN L.HDType = 1 THEN L.UserID ELSE null END) AS [LoginAppPCUser]

	INTO #Temp_LoginApp
	FROM [LogDB].[Users].[UserLogonLog] L with (nolock)
	where 
		[CreateTime] >= @LogStartTime
		AND [CreateTime] < @LogEndTime
	group by L.platformid, L.AreaID, CONVERT(VARCHAR(10), L.[CreateTime],23)
	------------------------------------------------------


	/*  更新註冊統計數據 */
	------------------------------------------------------
	MERGE [Stat].[StatisticsRegUser] AS A
	USING (
			SELECT	[CreateTime],[PlatformID],[AreaID],[TotalRegCount],[IOSRegCount],[AndroidRegCount],[PCRegCount]
			FROM @Temp_UserRegLog
			) AS B ON (A.[StatTime] = B.[CreateTime] AND A.[PlatformID] = B.[PlatformID] AND A.[AreaID] = B.[AreaID])
	WHEN MATCHED THEN
		UPDATE SET	A.[StatTime] = B.[CreateTime],
					A.[PlatformID] = B.[PlatformID],
					A.[AreaID] = B.[AreaID],
					A.[TotalRegCount] = ISNULL(B.[TotalRegCount],0),
					A.[IOSRegCount] = ISNULL(B.[IOSRegCount],0),
					A.[AndroidRegCount] = ISNULL(B.[AndroidRegCount],0),
					A.[PCRegCount] = ISNULL(B.[PCRegCount],0)
	WHEN NOT MATCHED THEN
		INSERT
			([StatTime],[PlatformID],[AreaID],[TotalRegCount],[IOSRegCount],[AndroidRegCount],[PCRegCount])
		VALUES
			(
				[CreateTime],
				[PlatformID],
				[AreaID],
				ISNULL([TotalRegCount],0),
				ISNULL([IOSRegCount],0),
				ISNULL([AndroidRegCount],0),
				ISNULL([PCRegCount],0)
			);
	------------------------------------------------------




	/*  更新登入統計數據 */
	------------------------------------------------------
	MERGE [Stat].[StatisticsRegUser] AS A
	USING (
			SELECT	[CreateTime],[PlatformID],[AreaID],LoginAppUser,LoginAppIOSUser,LoginAppAndroidUser,LoginAppPCUSer
			FROM #Temp_LoginApp
			) AS B ON (A.[StatTime] = B.[CreateTime] AND A.[PlatformID] = B.[PlatformID] AND A.[AreaID] = B.[AreaID])
	WHEN MATCHED THEN
		UPDATE SET	A.[StatTime] = B.[CreateTime],
					A.[PlatformID] = B.[PlatformID],
					A.[AreaID] = B.[AreaID],
					A.LoginAppUser = ISNULL(B.LoginAppUser,0),
					A.LoginAppIOSUser = ISNULL(B.LoginAppIOSUser,0),
					A.LoginAppAndroidUser = ISNULL(B.LoginAppAndroidUser,0),
					A.LoginAppPCUSer = ISNULL(B.LoginAppPCUSer,0)
	WHEN NOT MATCHED THEN
		INSERT
			([StatTime],[PlatformID],[AreaID],[LoginAppUser],[LoginAppIOSUser],[LoginAppAndroidUser],[LoginAppPCUSer])
		VALUES
			(
				[CreateTime],
				[PlatformID],
				[AreaID],
				ISNULL(LoginAppUser,0),
				ISNULL(LoginAppIOSUser,0),
				ISNULL(LoginAppAndroidUser,0),
				ISNULL(LoginAppPCUSer,0)
			);
	------------------------------------------------------

	RETURN
END
GO
GRANT EXECUTE
    ON OBJECT::[dbo].[PrJob_Stat_RegLog] TO [xbuser]
    AS [dbo];

