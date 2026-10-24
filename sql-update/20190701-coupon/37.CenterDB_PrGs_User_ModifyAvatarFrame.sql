USE [CenterDB]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

-- =============================================
-- Author:		Mars
-- Create date: 2019-07-02
-- Description: 儲存用戶頭像框設定
-- =============================================
CREATE PROCEDURE [dbo].[PrGs_User_ModifyAvatarFrame]
	@intUserID		INT,		--用户ID
	@intAvatarFrame	INT = 0,	--VIP頭像框ID
	@chvErrMsg		VARCHAR(128) OUTPUT		--返回消息
AS
BEGIN 
	SET NOCOUNT ON

	--取得使用者紀錄
	UPDATE Users.UserInfo SET AvatarFrame = @intAvatarFrame WHERE UserID = @intUserID;

	IF @@ROWCOUNT > 0
	BEGIN
		--更新成功
		RETURN 1
	END

	ELSE
	BEGIN
		SET @chvErrMsg = 'VIP头像框更新失败';
		RETURN -1
	END
END