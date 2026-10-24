-- =============================================
-- Author:		alan
-- Create date: 2018/4/27
-- Description:	指派客服人員
-- changelog:
--		20180522	alan	調整根據user身分與是否上線指派人員
--		20180605	alan	調整根據user身分判定是否為客服人員
--		20180613	alan	如果沒有任何一個人員被篩選出來, 直接選定一位客服
-- =============================================
CREATE PROCEDURE [dbo].[PrWeb_AvailableCS_Get]
AS
BEGIN

	SET NOCOUNT ON;
	
	select top 1 UserID, LoginName from  [Admin].[Users]  with (nolock) where UserType = 9 and lastlogontime > dateadd (hour,-2,getdate()) order by NEWID()
	if @@ROWCOUNT = 0 --如果沒有找到任何人, 直接選定一個客服
		select top 1 UserID, LoginName from  [Admin].[Users]  with (nolock) where UserType = 9

END
