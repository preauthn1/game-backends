-- =============================================
-- Author:		alan
-- Create date: 2019/5/7
-- Description:	统一支付 建立統一代付訂單後更新結果
-- =============================================
CREATE PROCEDURE [dbo].[PrWeb_Money_TungyiWithdrawCreate]
(
    @intStatus int,
    @chvTransId varchar(200),
    @chvTransData varchar(200),
	@intTeamOrderId int
)
AS
BEGIN
	SET NOCOUNT ON
	update [LogDB].[Money].[ConvertLog] set Status=@intStatus, TransId=@chvTransId, TransData=@chvTransData where Id=@intTeamOrderId and Status=2
END


GO
GRANT EXECUTE
    ON OBJECT::[dbo].[PrWeb_Money_TungyiWithdrawCreate] TO [GameUser]
    AS [dbo];

