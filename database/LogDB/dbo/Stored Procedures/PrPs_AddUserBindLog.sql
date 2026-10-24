-- =============================================
-- Author:		Jason
-- Create date: 2018/05/28
-- Description:	寫入用戶綁定紀錄 #349
-- =============================================
CREATE PROCEDURE [dbo].[PrPs_AddUserBindLog]
(
	@intUserID		INT,
	@tnyHDType		TINYINT,
	@intPlatformID	INT,
	@intAreaID		INT
)
AS
BEGIN
	SET NOCOUNT ON

	DECLARE @UserID		INT = @intUserID
	DECLARE @HDType		TINYINT = @tnyHDType
	DECLARE @PlatformID	INT = @intPlatformID
	DECLARE @AreaID		INT = @intAreaID
	DECLARE @StatTime	DATETIME = GETDATE()


	/*  回傳紀錄  */
	------------------------------------------------------
	INSERT INTO [Users].[UserBindLog] ([UserID],[HDType],[PlatformID],[AreaID],[CreateTime])
	VALUES (@UserID,@HDType,@PlatformID,@AreaID,@StatTime)
	------------------------------------------------------

	RETURN
END
GO
GRANT EXECUTE
    ON OBJECT::[dbo].[PrPs_AddUserBindLog] TO [GameUser]
    AS [dbo];


GO
GRANT EXECUTE
    ON OBJECT::[dbo].[PrPs_AddUserBindLog] TO [xbuser]
    AS [dbo];

