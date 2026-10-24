-- =============================================
-- Author:		CCC
-- Create date: 2019/7/16
-- Description:	代付 取得打款清单
-- =============================================
CREATE PROCEDURE [dbo].[PrWeb_Money_WithdrawGet]
	@intGWID			int,
	@intConvertType		int,	--1:支付宝;2:银联
	@intStatus			int = 2	--审核通过
AS
BEGIN
	
	SET NOCOUNT ON

	--取得状态=2(审核通过) 的兑换清单
	SELECT TOP 5
	       C.ID,		--项目组订单号
	       C.Amount,	--支付金额
	       C.BankName,	--银行名称
	       C.RealName,	--帐户名
	       C.Account,	--卡号
	       U.LoginName, --手机号
	       ISNULL(B.BankID, '0') as BankID, --银行编码 
		   ISNULL(B.Code, '0') as BankCode, --编码 
		   C.CreateIP	--用戶IP
	  FROM [LogDB].[Money].[ConvertLog] C with (nolock) 
	  left join CenterDB.Users.UserInfo U with (nolock) on C.UserID=U.UserID
	  left join CenterDB.Money.UnionpayBankCode B with (nolock) on U.UnionpayBank=B.Code
	 WHERE C.ConvertType = @intConvertType
	   AND GWID = @intGWID
	   AND Status = @intStatus

END