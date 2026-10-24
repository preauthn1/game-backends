-- =============================================
-- Author:		Jason
-- Create date: 2018/03/22
-- Description:	寫入使用者兌換紀錄
--	2018/04/24	jason	新增bankamount,cashamount,opreator 寫入
-- =============================================
CREATE PROCEDURE [dbo].[PrPs_AddConvertLog]
(
	@intUserID			INT,
	@lngAmount			BIGINT,
	@lngTaxAmount		BIGINT,
	@lngRealTaxAmount	BIGINT= 0,
	@tnyStatus			TINYINT,
	@chvRealName		VARCHAR(64),
	@tnyConvertType		TINYINT,
	@chvAccount			VARCHAR(64),
	@chvBankName		VARCHAR(64),
	@chvIP				VARCHAR(15),
	@dtStatTime			DATETIME,
	@intPlatformID		INT = NULL,
	@intAreaID			INT = NULL,
	@lngUserCashAmount	BIGINT = NULL,
	@lngUserBankAmount	BIGINT = NULL
)
AS
BEGIN
	SET NOCOUNT ON

	
	DECLARE @UserID				INT = @intUserID
	DECLARE @Amount				BIGINT = @lngAmount
	DECLARE @TaxAmount			BIGINT = @lngTaxAmount
	DECLARE @RealTaxAmount		BIGINT = @lngRealTaxAmount
	DECLARE @Status				TINYINT = @tnyStatus
	DECLARE @RealName			VARCHAR(64) = @chvRealName
	DECLARE @ConvertType		TINYINT = @tnyConvertType
	DECLARE @PayAccount			VARCHAR(64) = ISNULL(@chvAccount,' ')
	DECLARE @BankName			VARCHAR(64) = ISNULL(@chvBankName,' ')
	DECLARE @IP					VARCHAR(15) = @chvIP
	DECLARE @StatTime			DATETIME = @dtStatTime
	DECLARE @PlatformID			INT = @intPlatformID
	DECLARE @AreaID				INT = @intAreaID
	DECLARE @UserBankAmount		BIGINT = @lngUserBankAmount
	DECLARE @UserCashAmount		BIGINT = @lngUserCashAmount

	
	/*  寫入兌換申請紀錄  */
	------------------------------------------------------
	INSERT INTO [Money].[ConvertLog]
		(	
			UserID,		Amount,		TaxAmount,	RealTaxAmount,	Status,		RealName,		ConvertType,
			Account,		BankName,	CreateIP,	CreateTime,	LastUpdateTime,	CheckTime,	[PlatformID],	[AreaID],	[BankAmount],		[CashAmount]
		)
	VALUES
		(	@UserID,	@Amount,	@TaxAmount,	@RealTaxAmount,	@Status,	@RealName,	@ConvertType,
			@PayAccount,	@BankName,	@IP,		@StatTime,	@StatTime,		@StatTime,	@PlatformID,	@AreaID,	@UserBankAmount,	@UserCashAmount
		)
	------------------------------------------------------

	RETURN
END
GO
GRANT EXECUTE
    ON OBJECT::[dbo].[PrPs_AddConvertLog] TO [GameUser]
    AS [dbo];


GO
GRANT EXECUTE
    ON OBJECT::[dbo].[PrPs_AddConvertLog] TO [xbuser]
    AS [dbo];

