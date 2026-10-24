

-- =============================================
-- Author: alan
-- Create date: 2018/8/9
-- Description: 取得兑换可疑名单
-- =============================================
CREATE PROCEDURE [dbo].[PrWeb_Convert_GetSuspicious]
@chvName	varchar(50) = null,
@chvAccountID varchar(50) = null
AS
BEGIN

	Select * from [Game].[ConvertSuspicious] with (nolock) 
	where isnull(@chvAccountID, AccountID) = AccountID
	OR  isnull(@chvName, [Name]) = [Name]
	Order by CreateTime

END


GO
GRANT EXECUTE
    ON OBJECT::[dbo].[PrWeb_Convert_GetSuspicious] TO [GameUser]
    AS [dbo];


GO
GRANT EXECUTE
    ON OBJECT::[dbo].[PrWeb_Convert_GetSuspicious] TO [xbuser]
    AS [dbo];

