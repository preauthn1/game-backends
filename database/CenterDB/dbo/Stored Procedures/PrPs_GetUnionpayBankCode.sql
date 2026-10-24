-- =============================================
-- Author:  alan
-- Create date: 2018-3-14
-- Description:	获取銀聯卡銀行代號
-- =============================================
CREATE PROCEDURE [dbo].[PrPs_GetUnionpayBankCode]
AS
BEGIN
	select Code,Bankname from CenterDB.Money.UnionpayBankCode with (nolock)
	where show = 1
END

GO
GRANT EXECUTE
    ON OBJECT::[dbo].[PrPs_GetUnionpayBankCode] TO [GameUser]
    AS [dbo];


GO
GRANT EXECUTE
    ON OBJECT::[dbo].[PrPs_GetUnionpayBankCode] TO [xbuser]
    AS [dbo];

