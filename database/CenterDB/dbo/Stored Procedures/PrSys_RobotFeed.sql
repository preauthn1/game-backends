

-- =============================================
-- Author:  alan
-- Create date: 2018/4/15
-- Description:	機器人加錢
--	2018/05/07	alan	根據區段以及最低金額判斷加錢
--	2018/05/22	alan	#341 機器人金額調整
--  2018/05/28  alan	1.調整作法與金額 2.超過最大金額強制縮減
-- =============================================
CREATE PROCEDURE [dbo].[PrSys_RobotFeed]
AS

SET NOCOUNT ON

DECLARE @userid int
DECLARE @Group char(1)
DECLARE @minAmount bigint = 0
DECLARE @maxAmount bigint = 0
DECLARE @lngAmount bigint = 0


-- 進行機器人加錢(針對不在遊戲內的ServerID = 0)
DECLARE robot_moneyfeed_cursor CURSOR FOR 
     Select UserID, [Group] from  [Robot].[RobotList] with (nolock) where ServerID = 0

OPEN robot_moneyfeed_cursor
FETCH NEXT FROM robot_moneyfeed_cursor INTO  @userid,@Group

WHILE @@FETCH_STATUS = 0

BEGIN

	
		IF @Group = 'A'
		BEGIN
			SET @minAmount = 500000
			SET @maxAmount = 1000000
		END
		ELSE
		IF @Group = 'B'
		BEGIN
			SET @minAmount = 1000000
			SET @maxAmount = 2000000
		END
		ELSE
		IF @Group = 'C'
		BEGIN
			SET @minAmount = 1000
			SET @maxAmount = 6000
		END
		ELSE
		IF @Group = 'D'
		BEGIN
			SET @minAmount = 30000
			SET @maxAmount = 100000
		END
		ELSE
		IF @Group = 'E'
		BEGIN
			SET @minAmount = 8000
			SET @maxAmount = 20000
		END
		
		SET @lngAmount =  ROUND(((@maxAmount - @minAmount -1) * RAND() + @minAmount), 0) 
		
		Update Money.Cash Set Amount = @lngAmount Where 
		UserID = @userid AND (Amount <= @minAmount or Amount >= @maxAmount)
		
		--IF @@ROWCOUNT > 0
		--	PRINT 'give money:' + @Group + ' ' + convert(varchar,@userid) + ' ' +  convert(varchar,@lngAmount)



	FETCH NEXT FROM robot_moneyfeed_cursor INTO @userid,@Group

END

CLOSE robot_moneyfeed_cursor
DEALLOCATE robot_moneyfeed_cursor


GO
GRANT EXECUTE
    ON OBJECT::[dbo].[PrSys_RobotFeed] TO [GameUser]
    AS [dbo];


GO
GRANT EXECUTE
    ON OBJECT::[dbo].[PrSys_RobotFeed] TO [xbuser]
    AS [dbo];

