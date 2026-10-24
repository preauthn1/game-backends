-- =============================================
-- Author:		alan
-- Create date: 2019/5/7
-- Description:	统一支付 取得打款清单
-- =============================================
CREATE PROCEDURE [dbo].[PrWeb_Money_TungyiWithdrawGet]
AS
BEGIN

	SET NOCOUNT ON

	--取得银行卡 & 状态=2(审核通过) 的兑换清单
	SELECT top 5
	C.ID, --项目组订单号
	C.Amount,   --支付金额
	C.BankName, --银行名称
	C.RealName, --帐户名
	C.Account,  --卡号
	U.LoginName, --手机号
	ISNULL(B.BankID, '0') as BankID --银行编码
 
	FROM [LogDB].[Money].[ConvertLog] C with (nolock) 
	left join CenterDB.Users.UserInfo U with (nolock) on C.UserID=U.UserID
	left join CenterDB.Money.UnionpayBankCode B with (nolock) on U.UnionpayBank=B.Code
	join [CenterDB].[area].[Platform] as P with (nolock) on C.PlatformID = P.PlatformID
	WHERE 
	C.ConvertType=2 		--银行
	and Status=2			--审核通过
	and P.Company = 1

	
END
GO
GRANT EXECUTE
    ON OBJECT::[dbo].[PrWeb_Money_TungyiWithdrawGet] TO [GameUser]
    AS [dbo];

