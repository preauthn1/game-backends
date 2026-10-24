USE [PerGameLog]
GO
/****** Object:  UserDefinedFunction [dbo].[Fn_GetUserDetailInfo]    Script Date: 2019/6/27 下午 06:02:03 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		<xxx>
-- Create date: <xxx>
-- Description:	<解析用户每局记录信息>
-- 2018/04/26	Jason		新增用戶TradeID識別
--	2019/06/19	Jason		新增寫入禮碼金額欄位/原本@chvUserGameRet格式需新增禮碼金額
-- =============================================
ALTER FUNCTION [dbo].[Fn_GetUserDetailInfo]
(	
	@chvDetailInfo		VARCHAR(max)
)
RETURNS @returnTable TABLE (UserID int,CurrentAmount BIGINT,UpdateAmount BIGINT,TradeID BIGINT,CurrentCouponAmount BIGINT,CouponAmount BIGINT)
BEGIN 		

	DECLARE @pos INT
	
	SET @pos = CHARINDEX(',',@chvDetailInfo)
	WHILE @pos>0 OR LEN(@chvDetailInfo)>0
	BEGIN
		DECLARE @r VARCHAR(max)
		IF @pos = 0
		BEGIN
			SET @r = @chvDetailInfo
			SET @chvDetailInfo = ''	
		END
		ELSE
		BEGIN
			SET @r = SUBSTRING(@chvDetailInfo,0,@pos)
			SET @chvDetailInfo = SUBSTRING(@chvDetailInfo,@pos+1,LEN(@chvDetailInfo))
		END 
		
		DECLARE @intUserID INT,@intCurrentAmount BIGINT,@intUpdateAmount BIGINT,@intCharIndex int,@intTradeID BIGINT,@CurrentCouponAmount BIGINT,@CouponAmount BIGINT
		
		SET @intCharIndex = CHARINDEX(':',@r)
		SET @intUserID = CAST(LEFT(@r,@intCharIndex-1) AS INT)
		
		SET @r = SUBSTRING(@r,@intCharIndex+1,100000)
		SET @intCharIndex = CHARINDEX(':',@r)
		SET @intCurrentAmount = CAST(LEFT(@r,@intCharIndex-1) AS BIGINT)
		
		SET @r = SUBSTRING(@r,@intCharIndex+1,100000)
		SET @intCharIndex = CHARINDEX(':',@r)	
		SET @intUpdateAmount = CAST(LEFT(@r,@intCharIndex-1) AS BIGINT)
		
		SET @r = SUBSTRING(@r,@intCharIndex+1,100000)		
		SET @intCharIndex = CHARINDEX(':',@r)	
		SET @intTradeID = CAST(LEFT(@r,@intCharIndex-1) AS BIGINT)
		
		SET @r = SUBSTRING(@r,@intCharIndex+1,100000)
		SET @intCharIndex = CHARINDEX(':',@r)
		SET @CurrentCouponAmount = CAST(LEFT(@r,@intCharIndex-1) AS BIGINT)
		
		SET @r = SUBSTRING(@r,@intCharIndex+1,100000)		
		SET @CouponAmount = CAST(LEFT(@r,100000) AS BIGINT)
		
		INSERT INTO @returnTable
		SELECT @intUserID,@intCurrentAmount,@intUpdateAmount,@intTradeID,@CurrentCouponAmount,@CouponAmount		
		
		SET @pos = CHARINDEX(',',@chvDetailInfo)	
	END
	RETURN 
END
