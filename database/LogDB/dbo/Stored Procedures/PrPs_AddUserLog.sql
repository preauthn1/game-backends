-- =============================================
-- Author:		Jason
-- Create date: 2018/03/27
-- Description:	寫入用戶登入紀錄
-- =============================================
CREATE PROCEDURE [dbo].[PrPs_AddUserLog]
(
	@tnyLogType		TINYINT = 1,	--1:Create 2:Login
	@intUserID		INT,
	@dtStatTime		DATETIME,
	@tnyHDType		TINYINT,		--1:PC 2:Android 3:iOS
	@chvHDCode		VARCHAR(64) = NULL,
	@dtCreateTime	DATETIME = NULL,
	@intServerID	INT = NULL,
	@intAreaID		INT = NULL,
	@chvIPAddress	VARCHAR(40) = NULL,
	@intUserLevel	INT = NULL,
	@intPlatformID	INT = NULL
)
AS
BEGIN
	SET NOCOUNT ON

	DECLARE @LogType		TINYINT = @tnyLogType
	DECLARE @UserID			INT = @intUserID
	DECLARE @StatTime		DATETIME = @dtStatTime
	DECLARE @HDType			TINYINT = @tnyHDType
	DECLARE @CreateTime		DATETIME = ISNULL(@dtCreateTime,GETDATE())
	DECLARE @ServerID		INT = @intServerID
	DECLARE @AreaID			INT = @intAreaID
	DECLARE @IPAddress		VARCHAR(40) = @chvIPAddress
	DECLARE @UserLevel		INT = @intUserLevel
	DECLARE @HDCode			VARCHAR(64) = @chvHDCode


	/*  回傳紀錄  */
	------------------------------------------------------
	IF (@LogType = 1)
	BEGIN
		INSERT INTO [Users].[UserRegLog]
			(	[UserID],	[HDType],	[CreateTime],	[PlatformID],	[AreaID],	[HDCode],	[IPAddress])
		VALUES
			(	@UserID,	@HDType,	@CreateTime,	@intPlatformID,	@AreaID,	@HDCode,	@IPAddress	)
	END

	IF (@LogType = 2)
	BEGIN
		INSERT INTO [Users].[UserLogonLog]
			(	[CreateTime],	[UserID],	[HDType],	[ServerID],		[AreaID],	[IPAddress],	[UserLevel],	[PlatFormID],	[HDCode])
		VALUES
			(	@StatTime,		@UserID,	@HDType,	@ServerID,		@AreaID,	@IPAddress,		@UserLevel,		@intPlatformID,	@HDCode	)
	END
	------------------------------------------------------

	RETURN
END
GO
GRANT EXECUTE
    ON OBJECT::[dbo].[PrPs_AddUserLog] TO [GameUser]
    AS [dbo];


GO
GRANT EXECUTE
    ON OBJECT::[dbo].[PrPs_AddUserLog] TO [xbuser]
    AS [dbo];

