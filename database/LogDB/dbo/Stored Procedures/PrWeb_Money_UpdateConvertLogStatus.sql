-- =============================================
-- Author:		Jason
-- Create date: 2018/03/22
-- Description:	<修改兑换状态>
--	2018/04/24	Jason		新增[Operator] = @chvUserName
--	2018/04/25	Jason		新增[LastUpdateTime] = GETDATE()
--	2018/05/16	Jason		#256 新增兌換功能拒絕兌換歸還用戶金幣
--	2018/05/19	alan		調整 審核完成(2) 後也可以退還金幣
--	2018/05/22	alan		調整 订单异常(4) 异常帐号(5) 账户错误(7) 也可以退還金幣
--  2019/07/15	CCC			增加备注栏位
-- =============================================
CREATE PROCEDURE [dbo].[PrWeb_Money_UpdateConvertLogStatus]
(
	@intId				INT,	--ID
	@tnyStatus			INT,	--状态
	@chvUserName		VARCHAR(16),
	@chvMemo			VARCHAR(200),
	@intGWID			int = 0,
	@intplatformid		int output,
	@intareaid			int output,
	@intUserID			INT				OUTPUT,
	@lngConvertAmount	BIGINT			OUTPUT,
	@lngReturnAmount	BIGINT			OUTPUT,
	@intRtnCode			INT				OUTPUT,
	@chvRtnMsg			VARCHAR(64)		OUTPUT
)
AS
BEGIN
	SET NOCOUNT ON

	
	DECLARE @ID		INT = @intId

	SET @intRtnCode			=	0
	SET @chvRtnMsg			=	'無操作行為'

	IF NOT EXISTS (SELECT TOP 1 1 FROM [Money].[ConvertLog] WITH (NOLOCK) WHERE Id = @ID)
	BEGIN
		SET @intRtnCode = -1
		SET @chvRtnMsg = '訂單不存在'
		RETURN
	END


	/*  更新兌換申請紀錄  */
	------------------------------------------------------
	IF (@tnyStatus = 11) --選擇退還金幣
	BEGIN
		UPDATE [Money].[ConvertLog]
		SET @intplatformid = platformid,
			@intareaid = areaid,
			@lngConvertAmount = ABS(Amount),
			@lngReturnAmount = ABS(Amount) + TaxAmount,
			@intUserID = UserID,
			Status = @tnyStatus,
			TransData = '',
			[Operator] = @chvUserName,			
			LastUpdateTime = GETDATE(),
			Memo = @chvMemo
		WHERE
			Id = @intId
			AND Status in (0,2,4,5,7,100)

	END
	ELSE IF @tnyStatus = 2	--审核通过
	BEGIN
		UPDATE [Money].[ConvertLog] 
		SET @intplatformid = platformid,
			@intareaid = areaid,
			@lngConvertAmount = Amount,
			@lngReturnAmount = Amount + TaxAmount,
			@intUserID = UserID,
			Status = @tnyStatus,
			[Operator] = @chvUserName,
			[LastUpdateTime] = GETDATE(),
			Memo = @chvMemo,
			GWID = ISNULL(@intGWID, GWID)
		WHERE
			Id = @ID
		and Status not in (3,11) --已打款.退还金币
	END
	ELSE
	BEGIN
		UPDATE [Money].[ConvertLog] 
		SET @intplatformid = platformid,
			@intareaid = areaid,
			@lngConvertAmount = Amount,
			@lngReturnAmount = Amount + TaxAmount,
			@intUserID = UserID,
			Status = @tnyStatus,
			[Operator] = @chvUserName,
			[LastUpdateTime] = GETDATE(),
			Memo = @chvMemo
		WHERE
			Id = @ID
		and Status not in (3,11) --已打款.退还金币
	END
		
	IF @@ROWCOUNT=0
	BEGIN
		SET @intRtnCode = -2
		SET @chvRtnMsg = '訂單更新失敗'
		RETURN
	END
	------------------------------------------------------

	

	SET @intRtnCode = 1
	SET @chvRtnMsg = '訂單更新完成'
	RETURN
END
GO
GRANT EXECUTE
    ON OBJECT::[dbo].[PrWeb_Money_UpdateConvertLogStatus] TO [GameUser]
    AS [dbo];


GO
GRANT EXECUTE
    ON OBJECT::[dbo].[PrWeb_Money_UpdateConvertLogStatus] TO [xbuser]
    AS [dbo];

