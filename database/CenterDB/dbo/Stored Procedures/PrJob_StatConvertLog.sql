-- =============================================
-- Author:  Jason
-- Create date: 2018/03/22
-- Description:	#406 金幣統計改為LogDB計算及同步更新CenterDB
-- =============================================
CREATE PROCEDURE [dbo].[PrJob_StatConvertLog]
(
	@dtStatTime DATETIME = NULL
)
AS
BEGIN
	
	SET NOCOUNT ON

	SET @dtStatTime	= ISNULL(@dtStatTime,GETDATE())

	DECLARE @LogStartTime			DATETIME
	DECLARE @LogEndTime				DATETIME
	DECLARE	@lngTotalConvertAmount	BIGINT
	DECLARE	@lngTotalTaxAmount		BIGINT
	DECLARE	@AlipayAmount			BIGINT
	DECLARE	@AlipayTaxAmount		BIGINT
	DECLARE	@UnionpayAmount			BIGINT
	DECLARE	@UnionpayTaxAmount		BIGINT

	DECLARE @Temp_StatConvert	TABLE ([StatTime] DATETIME,[Amount] BIGINT,[TaxAmount] BIGINT,[RealTaxAmount] BIGINT)

	DECLARE @Temp_ConvertLog TABLE
		(
			[Id]				INT,
			[UserID]			INT,
			[Amount]			BIGINT,
			[TaxAmount]			BIGINT,
			[RealTaxAmount]		BIGINT,
			[Status]			TINYINT,
			[ConvertType]		TINYINT,
			[RealName]			VARCHAR(64),
			[Account]			VARCHAR(64),
			[BankName]			VARCHAR(50),
			[CreateIP]			VARCHAR(15),
			[TransID]			VARCHAR(64),
			[TransData]			VARCHAR(max),
			[CreateTime]		DATETIME,
			[LastUpdateTime]	DATETIME,
			[CheckTime]			DATETIME,
			[PlatformID]		INT,
			[AreaID]			INT,
			[BankAmount]		BIGINT,
			[CashAmount]		BIGINT,
			[Operator]			VARCHAR(16)
		)


	IF (DATEPART(HOUR,@dtStatTime) = 0) AND (DATEPART(MINUTE,@dtStatTime) < 10)
	BEGIN
		SET @LogStartTime		= DATEADD(DAY,-30,DATEDIFF(DAY,0,@dtStatTime))
		SET @LogEndTime			= DATEADD(DAY,0,DATEDIFF(DAY,0,@dtStatTime))
	END
	ELSE
	BEGIN
		SET @LogStartTime		= DATEADD(DAY,-30,DATEDIFF(DAY,0,@dtStatTime))
		SET @LogEndTime			= DATEADD(DAY,1,DATEDIFF(DAY,0,@dtStatTime))
	END


	/*  暫存紀錄  */
	----------------------------------------------------------------------------------
	INSERT INTO @Temp_ConvertLog
	EXEC [LOGDBLINKEDSERVER].[LogDB].[dbo].[PrWeb_Money_GetConvertLog]
		@dtbeginTime			= @LogStartTime,		
		@dtendTime				= @LogEndTime,
		@lngTotalConvertAmount  = @lngTotalConvertAmount	OUTPUT,     --兌換總金額
		@lngTotalTaxAmount      = @lngTotalTaxAmount		OUTPUT,     --手續費總金額
		@AlipayAmount			= @AlipayAmount				OUTPUT,     --支付寶總金額
		@AlipayTaxAmount		= @AlipayTaxAmount			OUTPUT,     --支付寶總金額
		@UnionpayAmount			= @UnionpayAmount			OUTPUT,     --銀行總金額
		@UnionpayTaxAmount		= @UnionpayTaxAmount		OUTPUT		--銀行總金額
	
	
	INSERT INTO @Temp_StatConvert
	SELECT	[StatTime] = DATEADD(DAY,0,DATEDIFF(DAY,0,[CreateTime])),
			[Amount] = SUM(CASE WHEN [Status] = 3 THEN [Amount] ELSE 0 END) - SUM(CASE WHEN [Status] = 11 THEN [Amount] ELSE 0 END),
			[TaxAmount] = SUM(CASE WHEN [Status] = 3 THEN [TaxAmount] ELSE 0 END) - SUM(CASE WHEN [Status] = 11 THEN [TaxAmount] ELSE 0 END),
			[RealTaxAmount] = SUM(CASE WHEN [Status] = 3 THEN [RealTaxAmount] ELSE 0 END) - SUM(CASE WHEN [Status] = 11 THEN [RealTaxAmount] ELSE 0 END)
	FROM @Temp_ConvertLog
	GROUP BY DATEADD(DAY,0,DATEDIFF(DAY,0,[CreateTime]))
	----------------------------------------------------------------------------------

	
	/*  更新統計資料  */
	----------------------------------------------------------------------------------
	MERGE [Stat].[ConvertLog] AS A
	USING (
				SELECT	[StatTime],[Amount],[TaxAmount],[RealTaxAmount]
				FROM @Temp_StatConvert
		) AS B ON (A.[StatTime] = B.[StatTime])
	WHEN MATCHED THEN
		UPDATE SET	A.[Amount] = B.[Amount],
					A.[TaxAmount] = B.[TaxAmount],
					A.[RealTaxAmount] = B.[RealTaxAmount]
	WHEN NOT MATCHED THEN
		INSERT ([StatTime],		[Amount],			[TaxAmount],			[RealTaxAmount])
		VALUES
			(
				B.[StatTime],	ISNULL([Amount],0),	ISNULL([TaxAmount],0),	ISNULL([RealTaxAmount],0)
			);
	----------------------------------------------------------------------------------

END

GO
GRANT EXECUTE
    ON OBJECT::[dbo].[PrJob_StatConvertLog] TO [xbuser]
    AS [dbo];

