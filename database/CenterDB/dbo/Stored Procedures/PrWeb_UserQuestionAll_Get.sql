-- =============================================
-- Author:  <xxx>
-- Create date: <xxx>
-- Description:	<获取用户最新投诉问题>
-- =============================================
CREATE PROCEDURE PrWeb_UserQuestionAll_Get
@intAdminID			INT=null,
@chvPlatformIDs		varchar(4000),		--平台ID集合
@intUserID			int=null,			--用户ID
@tnyStatus			tinyint=null,		--状态
@chvHandlePerson	varchar(32)=null,	--处理人
@dtBeginTime		datetime=null,		
@dtEndTime			datetime=null,
@intIncludeUser		int=null,			--用户ID
@intPageIndex		int=1,
@intPageSize		int=20,
@intPageCount		varchar(32)		output
AS
BEGIN
	DECLARE @intPlatformID	INT

	--渠道篩選
	DECLARE @Temp_PlatformID TABLE (PlatformID	INT)
	INSERT INTO @Temp_PlatformID Select PlatformID From CenterDB.Area.PlatformAdminUser where AdminID=@intAdminID

	DECLARE @intOperator int --处理人	

	--是否为打卡上班
	SELECT @intOperator = UserID FROM [CenterDB].[Users].[CSOnBoard] WHERE UserID=@intAdminID AND Status = 0
	

	--如果客服主动加入某个用户且用户从未询问客服, 则先塞入一笔空资料
	IF @intIncludeUser IS NOT NULL
	BEGIN

		--检查用户资料
		SELECT @intPlatformID = A.PlatformID FROM Users.UserInfo as U
		INNER JOIN Area.Area as A ON A.AreaID = U.AreaID	
		WHERE U.UserID = @intIncludeUser

		if @@ROWCOUNT <> 0 and @intPlatformID in (select PlatformID from @Temp_PlatformID)
		begin
	
			IF not exists (SELECT Id FROM Users.QuestionLog as L with(nolock)where UserID = @intIncludeUser)
			BEGIN

				INSERT INTO [CenterDB].[Users].[QuestionLog] (PlatformID, UserID, Question, Status, IPAddress, HandlePerson, CreateTime, Operator)
				VALUES (@intPlatformID, @intIncludeUser, '', 3, '', @chvHandlePerson, getdate(), @intOperator);

			END
			ELSE
			BEGIN
				
				;with cte as (
					SELECT top 1 * FROM [CenterDB].[Users].[QuestionLog] where UserID= @intIncludeUser order by id desc
				)
				update cte SET HandlePerson = @chvHandlePerson

			END
		END
	END


	Create Table #Temp_User (UserID INT primary key, CreateTime DATETIME)

	INSERT INTO #Temp_User
	SELECT L.UserID , CreateTime = MAX(L.CreateTime)
	  FROM Users.QuestionLog as L with(nolock)
	 INNER JOIN Users.UserInfo as U with(nolock) ON L.UserID = U.UserID
	 INNER JOIN Area.Area as A ON A.AreaID = U.AreaID	
	 WHERE (@intUserID IS NULL OR L.UserID = @intUserID)
	   AND (@tnyStatus IS NULL OR L.Status = @tnyStatus)
	   AND (@chvHandlePerson IS NULL OR L.HandlePerson = @chvHandlePerson)
	   AND (@dtBeginTime IS NULL OR L.CreateTime >= @dtBeginTime)
	   AND (@dtEndTime IS NULL OR L.CreateTime <= @dtEndTime)
	   AND L.PlatformID in (select PlatformID from @Temp_PlatformID)
	   AND (@intOperator IS NULL OR L.Operator IS NULL OR L.Operator = @intOperator)
	 GROUP BY L.UserID

	 SELECT @intPageCount = COUNT(1) FROM #Temp_User
	
	;with tab as 
	(
		select L.*,
		rowid = ROW_NUMBER() over(order by L.CreateTime desc),
		'User.Id' = U.UserID,
		'User.Grade' = U.GradeID,
		'User.LoginName' = U.LoginName,
		'User.Area.Platform.Name'=P.Name,
		'User.FaceID' = U.[FaceID],
		AU.[DisplayName]
		FROM Users.QuestionLog as L with(nolock)
		JOIN #Temp_User as T ON L.UserID = T.UserID AND L.CreateTime = T.CreateTime    
		INNER JOIN Users.UserInfo as U with(nolock) ON L.UserID = U.UserID
		INNER JOIN Area.Area as A ON A.AreaID = U.AreaID		
		INNER JOIN Area.[Platform] AS P ON P.PlatformID = A.PlatformID
		LEFT JOIN [AdminSys].[Admin].[Users] AS AU WITH (NOLOCK) ON (L.HandlePerson = AU.LoginName)
		WHERE 
		(@intUserID IS NULL OR L.UserID = @intUserID)
		AND (@tnyStatus IS NULL OR L.Status = @tnyStatus)
		AND (@chvHandlePerson IS NULL OR L.HandlePerson = @chvHandlePerson)
		AND (@dtBeginTime IS NULL OR L.CreateTime >= @dtBeginTime)
		AND (@dtEndTime IS NULL OR L.CreateTime <= @dtEndTime)
		AND L.PlatformID in (select PlatformID from @Temp_PlatformID)
		AND (@intOperator IS NULL OR L.Operator IS NULL OR L.Operator = @intOperator)				 
	)
	SELECT * FROM tab WHERE rowid between @intPageSize * (@intPageIndex -1) + 1 and @intPageSize * @intPageIndex
	

	return 1
END
