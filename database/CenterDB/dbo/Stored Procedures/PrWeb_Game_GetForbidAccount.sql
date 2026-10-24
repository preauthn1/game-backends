-- =============================================
-- Author:  alan
-- Create date: 2018/08/3
-- Description: 取得支付宝或手机号封锁清单

-- 2019/7/5	Eddie	調整搜尋條件時間範圍
-- 2019/7/9	Eddie	修改:若有傳入UserID但沒有時間區間的話，則回傳該帳號所有紀錄
-- =============================================
CREATE PROCEDURE [dbo].[PrWeb_Game_GetForbidAccount]
(
	@chvAccountID	varchar(50) = null,
	@tnyIdType		tinyint = null, --1 正式帐号(电话) 2支付宝帐号 3銀聯帳號
	@dtStartTime	datetime = null,
	@dtEndTime		datetime = null
)
AS
BEGIN
	SET NOCOUNT ON

	IF (@chvAccountID is not null and @dtStartTime is null and @dtEndTime is null)
	Begin
		SELECT AccountID, IdType, Memo
		FROM [Game].[ForbidAccount] as a with (nolock)
		WHERE
		ISNULL(@chvAccountID, AccountID) = AccountID
		and ISNULL(@tnyIdType,IdType) = IdType		
		ORDER BY CreateTime desc
	End
	ELSE
	Begin
		SELECT AccountID, IdType, Memo
		FROM [Game].[ForbidAccount] as a with (nolock)
		WHERE
		ISNULL(@chvAccountID, AccountID) = AccountID
		and ISNULL(@tnyIdType,IdType) = IdType
		and CreateTime >= IsNull(@dtStartTime, dateadd(day,-1,getdate()))
		and CreateTime <= IsNUll(@dtEndTime, getdate())
		--and ISNULL(@dtStartTime,getdate()) < CreateTime
		--and ISNULL(@dtEndTime,dateadd(week,-7,getdate())) >= CreateTime
		ORDER BY CreateTime desc
	END
END
GO


