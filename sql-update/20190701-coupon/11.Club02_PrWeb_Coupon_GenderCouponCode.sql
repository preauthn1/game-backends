USE [Club02]
GO
/****** Object:  StoredProcedure [dbo].[PrWeb_UserAgent_GenderQRCode]    Script Date: 2019/6/19 下午 01:02:57 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:  jason
-- Create date: 2019/06/19
-- Description:	預產禮碼 (僅工程人員使用)
-- =============================================
CREATE PROCEDURE [dbo].[PrWeb_Coupon_GenderCouponCode]
(
	@intCodeCounts	INT
)
AS
BEGIN
	SET NOCOUNT ON

	DECLARE @lenth INT = 12
	DECLARE @chvCode VARCHAR(12)
	DECLARE @Status INT = 0
	DECLARE @MinNo INT = 1
	DECLARE @MaxNo INT = @intCodeCounts


	WHILE(@MinNo <= @MaxNo)
	BEGIN
		SET @chvCode = ''
		SELECT @chvCode = COALESCE(@chvCode, '') + n
		FROM (	SELECT TOP (@lenth) CHAR(number) n FROM master..spt_values
				WHERE TYPE = 'P' AND 
					(number BETWEEN ASCII(0) AND ASCII(9)
					OR number BETWEEN ASCII('A') AND ASCII('Z')
					OR number BETWEEN ASCII('a') AND ASCII('z'))
					ORDER BY NEWID()
			) AS a

		IF NOT EXISTS (SELECT TOP 1 1 FROM [dbo].[CouponCode] WITH (NOLOCK) WHERE [CouponCode] COLLATE Chinese_PRC_CI_AS = @chvCode)
		BEGIN
			SET @MinNo = @MinNo + 1

			INSERT INTO [dbo].[CouponCode] ([CouponCode],[Status]) VALUES (@chvCode,@Status)
		END
	END

END