
-- =============================================
-- Author:  <xxx>
-- Create date: <xxx>
-- Description:	<删除权限>
-- =============================================
CREATE PROCEDURE [dbo].[PrWeb_Right_Delete]
	@intRightID		int		--RightID	
AS
BEGIN

	SET NOCOUNT ON


	DECLARE @Temp_RightResult	TABLE (RightID INT)
	DECLARE @Temp_RightDeleted	TABLE
		(	[RightID] INT,[RightName] VARCHAR(32),[ParentID] INT,[RightToken] VARCHAR(128),
			[PageUrl] VARCHAR(512),[RightType] TINYINT,[Sort] INT,[ShowInMenu] BIT,[IconCls] VARCHAR(32)
		)
	DECLARE @Temp_RoleRightDeleted	TABLE (Id INT,RoleID INT,[RightID] INT)
	DECLARE @Temp_UserRightDeleted	TABLE (Id INT,UserID INT,[RightID] INT)


	/*  確認權限層級及暫存  */
	---------------------------------------------------------------------------------------------
	IF EXISTS (SELECT TOP 1 1 FROM [Admin].[Right] WHERE RightID = @intRightID AND [ParentID] = 0)
	BEGIN
		INSERT INTO @Temp_RightResult
		SELECT	RightID
		FROM [Admin].[Right] WITH (NOLOCK)
		WHERE
			[ParentID] = @intRightID
		UNION
		SELECT	@intRightID
	END
	ELSE
	BEGIN
		INSERT INTO @Temp_RightResult VALUES (@intRightID)
	END
	---------------------------------------------------------------------------------------------


	/*  執行刪除權限  */
	---------------------------------------------------------------------------------------------
	--删除权限
	DELETE FROM [Admin].[Right]
	OUTPUT	DELETED.RightID,
			DELETED.RightName,
			DELETED.ParentID,
			DELETED.RightToken,
			DELETED.PageUrl,
			DELETED.RightType,
			DELETED.Sort,
			DELETED.ShowInMenu,
			DELETED.IconCls
		INTO @Temp_RightDeleted
	WHERE RightID IN (SELECT RightID FROM @Temp_RightResult)

	--删除角色权限		
	DELETE FROM [Admin].[RoleRight]
	OUTPUT	DELETED.Id,
			DELETED.RoleID,
			DELETED.RightID
		INTO @Temp_RoleRightDeleted
	WHERE RightID IN (SELECT RightID FROM @Temp_RightResult)

	--删除使用者权限	
	DELETE FROM [Admin].[UserRight]
	OUTPUT	DELETED.Id,
			DELETED.UserID,
			DELETED.RightID
		INTO @Temp_UserRightDeleted
	WHERE RightID IN (SELECT RightID FROM @Temp_RightResult)
	---------------------------------------------------------------------------------------------


	/*  寫入紀錄  */
	---------------------------------------------------------------------------------------------
	--寫入刪除紀錄
	INSERT INTO [Admin].[Right_Log]
		([RightID],[RightName],[ParentID],[RightToken],[PageUrl],[RightType],[Sort],[ShowInMenu],[IconCls])
	SELECT	[RightID],[RightName],[ParentID],[RightToken],[PageUrl],[RightType],[Sort],[ShowInMenu],[IconCls]
	FROM @Temp_RightDeleted

	--寫入角色刪除紀錄
	INSERT INTO [Admin].[RoleRight_Log]
		([Id],[RoleID],[RightID])
	SELECT	[Id],[RoleID],[RightID]
	FROM @Temp_RoleRightDeleted

	--寫入使用者刪除紀錄
	INSERT INTO [Admin].[UserRight_Log]
		([Id],[UserID],[RightID])
	SELECT	[Id],[UserID],[RightID]
	FROM @Temp_UserRightDeleted
	---------------------------------------------------------------------------------------------
END
