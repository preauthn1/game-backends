-- =============================================
-- Author:  <xxx>
-- Create date: <xxx>
-- Description:	<获取客服用户清单>
-- =============================================
CREATE  PROCEDURE [dbo].[PrWeb_QuestionUserNameList_Get]
@intAdminID			INT,
@intIncludeUser		int=null			--新增用户ID
AS
BEGIN
	DECLARE @intPlatformID	INT

	--渠道篩選
	DECLARE @Temp_PlatformID TABLE (PlatformID	INT)
	INSERT INTO @Temp_PlatformID Select PlatformID From CenterDB.Area.PlatformAdminUser where AdminID=@intAdminID

	DECLARE @chvHandlePerson varchar(32)
	select  @chvHandlePerson=LoginName from AdminSys.Admin.Users where UserID=@intAdminID

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

				INSERT INTO [CenterDB].[Users].[QuestionLog] (PlatformID, UserID, Question, Status, IPAddress, HandlePerson, CreateTime)
				VALUES (@intPlatformID, @intIncludeUser, '', 3, '', @chvHandlePerson, getdate());

			END
			ELSE
			BEGIN
				
				;with cte as (
					SELECT top 1 * FROM [CenterDB].[Users].[QuestionLog] where UserID= @intIncludeUser order by id desc
				)
				update cte SET HandlePerson = @chvHandlePerson where Status != 1

			END
		END
	END


	select top 5000 L.UserID,U.faceID,L.HandlePerson, CASE WHEN ReplyContent is null then 1 else 0 end IsNew, L.CreateTime
	FROM Users.QuestionLog as L with(nolock)
	LEFT JOIN Users.UserInfo as U with(nolock) ON L.UserID = U.UserID
	WHERE 
	L.PlatformID in (select PlatformID from @Temp_PlatformID)
	and L.CreateTime >= dateadd(month,-1,getdate())
	and L.UserID >0


	order by L.CreateTime desc
						 
	
	return 1
END

GO
GRANT EXECUTE
    ON OBJECT::[dbo].[PrWeb_QuestionUserNameList_Get] TO [GameUser]
    AS [dbo];

