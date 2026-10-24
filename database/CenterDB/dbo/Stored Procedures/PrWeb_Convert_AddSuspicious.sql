

-- =============================================
-- Author: alan
-- Create date: 2018/8/9
-- Description: 新增兑换可疑名单
-- =============================================
CREATE PROCEDURE [dbo].[PrWeb_Convert_AddSuspicious]
@chvName	varchar(50) = null,
@chvAccountID varchar(50),
@chvRtnMsg		varchar(200) = '' OUTPUT
AS
BEGIN
	if not exists(select 1 from  [Game].[ConvertSuspicious] with (nolock) where AccountID = @chvAccountID)
		Insert into [Game].[ConvertSuspicious]([Name], AccountID) values(ISNULL(@chvName,''), @chvAccountID)

    SET @chvRtnMsg	= '新增成功'
	RETURN 1
END



GO
GRANT EXECUTE
    ON OBJECT::[dbo].[PrWeb_Convert_AddSuspicious] TO [GameUser]
    AS [dbo];


GO
GRANT EXECUTE
    ON OBJECT::[dbo].[PrWeb_Convert_AddSuspicious] TO [xbuser]
    AS [dbo];

