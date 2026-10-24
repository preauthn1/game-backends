-- =============================================
-- Author:  Jason
-- Create date: 2018/06/06
-- Description:	#413 註冊申請預先寫入紀錄
-- =============================================
CREATE PROCEDURE [dbo].[PrWeb_User_AddPreRegInfo]
(
	@intAreaID	INT,
	@chvHDCode	VARCHAR(64),
	@chvUserIP	VARCHAR(40)
)
AS
BEGIN
	
	SET NOCOUNT ON

	DECLARE @NowTime	DATETIME = GETDATE()
	DECLARE @UpdateTime	DATETIME = DATEADD(MINUTE,1,GETDATE())
	DECLARE @LogDate	INT = CAST(CONVERT(VARCHAR(8),@NowTime,112) AS INT)
	

	/*  寫入註冊申請紀錄  */
	-------------------------------------------------------------------------------------------
	IF EXISTS (SELECT TOP 1 1 FROM [Log].[PreIPReg] WITH (NOLOCK) WHERE [LogDate] = @LogDate AND [IPAddress] = @chvUserIP)
	BEGIN
		UPDATE [Log].[PreIPReg]
		SET [Counts] = [Counts] + 1
		WHERE
			[LogDate] = @LogDate
			AND [IPAddress] = @chvUserIP
	END
	ELSE
	BEGIN
		INSERT INTO [Log].[PreIPReg]
			([LogDate],[IPAddress],[CreateTime],[Counts])
		VALUES
			(@LogDate,@chvUserIP,@NowTime,1)
	END

	IF @@ERROR <> 0
	BEGIN
		RETURN -1
	END


	INSERT INTO [Log].[PreRegData]
		([AreaID],[HDCode],[IPAddress],[CreateTime])
	VALUES
		(@intAreaID,@chvHDCode,@chvUserIP,@NowTime)

	IF @@ERROR <> 0
	BEGIN
		RETURN -1
	END


	RETURN 1
	-------------------------------------------------------------------------------------------

END
GO
GRANT EXECUTE
    ON OBJECT::[dbo].[PrWeb_User_AddPreRegInfo] TO [GameUser]
    AS [dbo];


GO
GRANT EXECUTE
    ON OBJECT::[dbo].[PrWeb_User_AddPreRegInfo] TO [xbuser]
    AS [dbo];

