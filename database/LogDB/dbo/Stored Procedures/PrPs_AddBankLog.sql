-- =============================================
-- Author:		Jason
-- Create date: 2018/03/21
-- Description:	銀行進出紀錄
--	2018/04/10		Jason		新增寫入AreaID & PlatformID
-- =============================================
CREATE PROCEDURE [dbo].[PrPs_AddBankLog]
(
	@intUserID			INT,
	@intSourceTypeID	INT,
	@lngCashAmount		BIGINT,
	@lngCurrentAmount	BIGINT,
	@lngUpdateAmount	BIGINT,
	@chrIPAddress		VARCHAR(15),
	@chrMemo			VARCHAR(128),
	@dtCreateTime		DATETIME,
	@intAreaID			INT = NULL,
	@intPlatformID		INT = NULL
)
AS
BEGIN
	SET NOCOUNT ON
	
	SET @lngCashAmount		=	ISNULL(@lngCashAmount,0)
	SET @lngCurrentAmount	=	ISNULL(@lngCurrentAmount,0)
	SET @lngUpdateAmount	=	ISNULL(@lngUpdateAmount,0)
		
	/*  回傳紀錄  */
	------------------------------------------------------
	INSERT INTO [Money].[BankLog]
		([UserID],	[SourceTypeID],		[CashAmount],	[CurrentAmount],	[UpdateAmount],		[IPAddress],	[Memo],	[CreateTime],	[AreaID],	[PlatformID]	)
	VALUES
		(@intUserID,@intSourceTypeID,	@lngCashAmount,	@lngCurrentAmount,	@lngUpdateAmount,	@chrIPAddress,	@chrMemo,@dtCreateTime,	@intAreaID,	@intPlatformID	)
	------------------------------------------------------

	RETURN
END
GO
GRANT EXECUTE
    ON OBJECT::[dbo].[PrPs_AddBankLog] TO [GameUser]
    AS [dbo];


GO
GRANT EXECUTE
    ON OBJECT::[dbo].[PrPs_AddBankLog] TO [xbuser]
    AS [dbo];

