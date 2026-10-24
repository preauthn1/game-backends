-- =============================================
-- Author:  alan
-- Create date: 2018/7/31
-- Description:	提供服務器進入遊戲房間,获取用户基本信息
-- =============================================
CREATE PROCEDURE [dbo].[PrGs_User_GetUserInfo]
	@intUserID			INT,	                    --用户ID		
	@chvErrMsg			varchar(128) output			--返回消息
AS
SET NOCOUNT ON

SET @chvErrMsg = ''

DECLARE @lngUserLevel bigint,@chvKey varchar(32)
DECLARE @intAreaID int

SELECT @intAreaID = AreaID, @lngUserLevel=GradeID from Users.UserInfo with(nolock) WHERE UserID = @intUserID
IF @@ROWCOUNT = 0
BEGIN
	SET @chvErrMsg = '用户不存在'
	return
END

--获取游戏信息
DECLARE 
@lngCashAmount		BIGINT ,        	--现金
@lngBankAmount		BIGINT ,        	--银行
@bIsLock			BIT,				--是否金币锁
@intShowConvertLimit	BIGINT,				--顯示兌換的充值金額 (單位分)
@intShowAllGameLimit	BIGINT				--顯示其他遊戲的充值金額 (單位分)


set @bIsLock = 0
set @lngCashAmount = 0 
set @lngBankAmount = 0 
set @intShowConvertLimit = 0
set @intShowAllGameLimit = 0

select @lngCashAmount = Amount FROM Money.Cash with(nolock) WHERE UserID = @intUserID
select @lngBankAmount = Amount FROM Money.Bank with(nolock) WHERE UserID = @intUserID

SELECT @bIsLock = 1 FROM Game.GameUserList with(nolock) where UserID = @intUserID

SELECT 
u.LoginName as LoginName,	--账号名
u.NickName,									--昵称
u.UserType,									--用户类型
u.Sex,										--性别
u.FaceID AS  HeadID,						--头像ID
@lngCashAmount AS CashAmount,				--现金数量
@lngBankAmount as BankAmount,				--银行现金数量
(u.SumCharge + u.SumAgentCharge) as SumCharge, --充值金額
u.SumConvert,
@intShowConvertLimit as ShowConvertLimit, --顯示兌換的充值金額 (單位分)
@intShowAllGameLimit as ShowAllGameLimit, --顯示所有遊戲充值金額 (單位分)
@bIsLock AS IsGaming,							--是否游戏中
CASE WHEN UserType = 3 THEN 0 ELSE 1 END  AS IsBindGuest,					--是否绑定游客,
CASE WHEN ISNULL(AlipayAccount, '') = '' THEN 0 ELSE 1 END AS IsBindZhifubao,			--是否绑定支付宝
ISNULL(AlipayAccount,'') as AlipayAccount,	--支付宝账号
dbo.Fn_HideRealName(RealName) as RealName,			--支付宝账号姓名
CASE WHEN ISNULL(UnionpayAccount, '') = '' THEN 0 ELSE 1 END AS IsBindUnionpayBank,		--是否绑定銀行卡
ISNULL(UnionpayBank,'') as UnionpayBank,				--銀行卡代號
dbo.Fn_HideAccount(UnionpayAccount) as UnionpayAccount,	--銀行卡卡號
dbo.Fn_HideRealName(UnionpayRealName) as UnionpayRealName,	--銀行卡账号姓名
CASE WHEN UnionpayBank IS NULL THEN '' ELSE (select Bankname from Money.UnionpayBankCode where UnionpayBank = Code ) END as UnionpayBankName, -- 銀行名稱
ISNULL(@lngUserLevel,0) as UserLevel,					--用户等级
ExperienceCash											--體驗金
FROM Users.UserInfo u with (nolock)
WHERE u.UserID = @intUserID

GO
GRANT EXECUTE
    ON OBJECT::[dbo].[PrGs_User_GetUserInfo] TO [GameUser]
    AS [dbo];


GO
GRANT EXECUTE
    ON OBJECT::[dbo].[PrGs_User_GetUserInfo] TO [xbuser]
    AS [dbo];

