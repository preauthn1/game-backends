-- =============================================
-- Author:  jason
-- Create date: 2019/05/13
-- Description:	JackpotBonusGame紀錄
--
-- =============================================
CREATE PROCEDURE [dbo].[PrGs_DFDC_JackpotBonusGameRecord]
	@intUserID			INT,		-- 用户ID
	@chvRecordStr		VARCHAR(MAX),	--玩家JackpotBonusGame紀錄字串
	@lngWinMoney		BIGINT,
	@chvMsg				VARCHAR(100)	OUTPUT
AS
BEGIN

	SET NOCOUNT ON
	
	
	DECLARE @FullstringDelimiter VARCHAR(1) = '|'
	DECLARE @LevelstringDelimiter VARCHAR(1) = ','
	DECLARE @Temp_stringsplit TABLE (Num INT,RecordString VARCHAR(100))
	DECLARE @GameType INT = 2 -- 1:freespin 2:dfdc
	
	INSERT INTO @Temp_stringsplit
	SELECT	[id],[Value]
	FROM [dbo].[fn_Split](@chvRecordStr,@FullstringDelimiter)

	/*  确认笔数  */
	IF ((SELECT COUNT(Num) FROM @Temp_stringsplit) <> 9)
	BEGIN
		SET @chvMsg	= '操作失敗 : 设定数据错误'
		RETURN -1
	END
	
	DECLARE @minno INT
	DECLARE @maxno INT
	DECLARE @checkmax INT = 0
	DECLARE @errorlevel INT = 0

	SELECT	@minno = MIN(Num),
			@maxno = MAX(Num)
	FROM @Temp_stringsplit


	WHILE(@minno <= @maxno)
	BEGIN
		DECLARE @string VARCHAR(MAX) = NULL

		SELECT	@string = RecordString FROM @Temp_stringsplit WHERE Num = @minno

		SET @string = REPLACE(@string,',','.')
		
		DECLARE @TurnID INT = null
		DECLARE @Place INT = null
		DECLARE @Icon INT = null
		
		SET @TurnID = CAST(PARSENAME(@string,3) as INT)
		SET @Place = CAST(PARSENAME(@string,2) as INT)
		SET @Icon = CAST(PARSENAME(@string,1) as INT)


		IF NOT EXISTS (SELECT TOP 1 1 FROM [Game].[DFDC_BonusGameRecord] WHERE UserID = @intUserID AND TurnID = @TurnID)
		BEGIN
			INSERT INTO [Game].[DFDC_BonusGameRecord]
				([UserID],[TurnID],[Place],[Icon])
			VALUES
				(@intUserID,@TurnID,@Place,@Icon)
		END
		ELSE
		BEGIN
			UPDATE [Game].[DFDC_BonusGameRecord]
			SET [Place] = @Place,
				[Icon] = @Icon
			WHERE
				UserID = @intUserID
				AND TurnID = @TurnID
		END
	
	
		SET @minno = @minno + 1
	END
	
	
	IF ((SELECT	COUNT(1) FROM [Game].[DFDC_BonusGameRecord] WITH (NOLOCK) WHERE UserID = @intUserID) < 9)
	BEGIN
		SET @chvMsg	= '操作失敗 : 设定数据错误'
		RETURN -1
	END


	IF NOT EXISTS (SELECT TOP 1 1 FROM [Game].[GameWinMoney] WITH (NOLOCK) WHERE UserID = @intUserID AND [GameType] = @GameType)
	BEGIN
		INSERT INTO [Game].[GameWinMoney]
			([UserID],[GameType],[WinMoney])
		VALUES
			(@intUserID,@GameType,@lngWinMoney)
	END
	ELSE
	BEGIN
		UPDATE [Game].[GameWinMoney]
		SET [WinMoney] = @lngWinMoney
		WHERE
			UserID = @intUserID
			AND [GameType] = @GameType
	END

	IF(@@ROWCOUNT <> 1)
	BEGIN
		SET @chvMsg	= '操作失敗 : 设定数据错误'
		RETURN -1
	END


	
	SET @chvMsg = '操作成功'
	
	RETURN 1	
	
	
END




GO
GRANT EXECUTE
    ON OBJECT::[dbo].[PrGs_DFDC_JackpotBonusGameRecord] TO [GameUser]
    AS [dbo];

