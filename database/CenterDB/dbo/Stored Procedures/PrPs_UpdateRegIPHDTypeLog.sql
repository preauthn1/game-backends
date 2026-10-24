-- =============================================
-- Author:		Jason
-- Create date: 2018/03/16
-- Description:	IP & 硬件註冊紀錄
-- ChangeLog:
--		2018/6/27	alan IPReg增加logdate区分
--	2018/07/16	jason		#597 資料庫效能優化 新增字串空白過濾
-- =============================================
CREATE PROCEDURE [dbo].[PrPs_UpdateRegIPHDTypeLog]
(
	@chvIPAddress		VARCHAR(40) = NULL,
	@tnyHDType			TINYINT = NULL,
	@chvHDCode			VARCHAR(64) = NULL
)
AS
BEGIN
	SET NOCOUNT ON

	DECLARE @IPAddress	VARCHAR(40) = rtrim(ltrim(@chvIPAddress))
	DECLARE @HDType		TINYINT = rtrim(ltrim(@tnyHDType))
	DECLARE @HDCode		VARCHAR(64) = rtrim(ltrim(@chvHDCode))

	DECLARE @NowTime	DATETIME = GETDATE()
	DECLARE @LogDate	INT = CAST(CONVERT(VARCHAR(8),@NowTime,112) AS INT)


	/*  --IP注册记录  */
	-------------------------------------------------------------------------------------------
	IF (@IPAddress IS NOT NULL)
	BEGIN
		UPDATE [Log].[IPReg]
		SET RegCount = RegCount + 1
		WHERE
			[LogDate] = @LogDate and
			IPAddress = @IPAddress

		IF @@ROWCOUNT = 0
		BEGIN
			INSERT INTO [Log].[IPReg]
				([LogDate], IPAddress,	RegCount)
			SELECT	@LogDate, @IPAddress,	1
		END
	END
	-------------------------------------------------------------------------------------------


	/*  --硬件注册记录  */
	-------------------------------------------------------------------------------------------
	IF (@HDType IS NOT NULL AND @HDCode IS NOT NULL)
	BEGIN
		UPDATE [Log].[HDReg]
		SET RegCount = RegCount + 1
		WHERE 
			HDType = @HDType
			AND HDCode = @HDCode
			AND [LogDate] = @LogDate

		IF @@ROWCOUNT = 0
		BEGIN
			INSERT INTO [Log].[HDReg]
				(	HDType,		HDCode,		RegCount, LogDate)
			SELECT	@HDType,	@HDCode,	1, @LogDate
		END
	END
	-------------------------------------------------------------------------------------------
END
GO
GRANT EXECUTE
    ON OBJECT::[dbo].[PrPs_UpdateRegIPHDTypeLog] TO [GameUser]
    AS [dbo];


GO
GRANT EXECUTE
    ON OBJECT::[dbo].[PrPs_UpdateRegIPHDTypeLog] TO [xbuser]
    AS [dbo];

