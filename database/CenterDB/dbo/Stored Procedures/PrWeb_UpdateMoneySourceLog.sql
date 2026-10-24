-- =============================================
-- Author:  2018/04/12
-- Create date: Jason
-- Description:	更新金幣統計
--	2018/06/07	Jason		#406 金幣統計改為LogDB計算及同步更新CenterDB
-- =============================================
CREATE PROCEDURE [dbo].[PrWeb_UpdateMoneySourceLog]
(
	@intUserID				INT,		--用户ID
	@intServerID			INT,		--伺服器ID
	@lngUpdateAmount		BIGINT,		--更新金币数	
	@lngTaxAmount			BIGINT,		--稅收金币数	
	@CreateTime				DATETIME	--執行時間
)
AS
BEGIN
	
	SET NOCOUNT ON

	DECLARE @UserID				INT = @intUserID
	DECLARE @ServerID			INT = @intServerID
	DECLARE @UpdateAmount		BIGINT
	DECLARE @TaxAmount			BIGINT
	DECLARE @RobotUpdateAmount	BIGINT
	DECLARE @RobotTaxAmount		BIGINT
	DECLARE @bitIsRobot			BIT = 0
	DECLARE @StatTime			DATETIME = DATEADD(DAY,0,DATEDIFF(DAY,0,@CreateTime))
	--DECLARE @EndTime			DATETIME = DATEADD(DAY,1,DATEDIFF(DAY,0,@CreateTime))


	/* 確認是否為機器人及更新內容  */
	----------------------------------------------------------------------------------------
	IF (@ServerID >= 10000)
	BEGIN
		SET @UpdateAmount = @lngUpdateAmount
		SET @TaxAmount = @lngTaxAmount
		SET @RobotUpdateAmount = 0
		SET @RobotTaxAmount = 0
	END
	ELSE IF EXISTS(SELECT TOP 1 1 FROM Robot.RobotList WITH (NOLOCK) WHERE UserID = @UserID)
	BEGIN
		SET @UpdateAmount = 0
		SET @TaxAmount = 0
		SET @RobotUpdateAmount = @lngUpdateAmount
		SET @RobotTaxAmount = @lngTaxAmount
	END
	ELSE
	BEGIN
		SET @UpdateAmount = @lngUpdateAmount
		SET @TaxAmount = @lngTaxAmount
		SET @RobotUpdateAmount = 0
		SET @RobotTaxAmount = 0
	END	
	----------------------------------------------------------------------------------------


	/*  更新金幣統計  */
	----------------------------------------------------------------------------------------
	IF (@ServerID < 10000)
	BEGIN
		UPDATE Stat.MoneySourceLog
		SET Amount = Amount + @UpdateAmount,
			TaxAmount = TaxAmount + @TaxAmount,
			RobotAmount = RobotAmount + @RobotUpdateAmount,
			RobotTaxAmount = RobotTaxAmount + @RobotTaxAmount
		WHERE
			SourceID = @ServerID
			AND StatTime = @StatTime
	END
	----------------------------------------------------------------------------------------
	
END

GO
GRANT EXECUTE
    ON OBJECT::[dbo].[PrWeb_UpdateMoneySourceLog] TO [GameUser]
    AS [dbo];


GO
GRANT EXECUTE
    ON OBJECT::[dbo].[PrWeb_UpdateMoneySourceLog] TO [xbuser]
    AS [dbo];

