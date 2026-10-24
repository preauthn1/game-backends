

-- =============================================
-- Author: alan
-- Create date: 2018/8/9
-- Description: 刪除兑换可疑名单
-- =============================================
CREATE PROCEDURE [dbo].[PrWeb_Convert_EditSuspicious]
@ID	int,
@chvAction varchar(10),
@chvName varchar(50) = null,
@chvAccountID varchar(50) = null,
@chvErrMsg	VARCHAR(256) OUTPUT		--返回消息
AS
BEGIN

IF lower(@chvAction) = 'upd'
  BEGIN
	Update [Game].[ConvertSuspicious] SET [Name]=@chvName, AccountID=@chvAccountID WHERE ID = @ID

	SET @chvErrMsg	= '修改成功'
	RETURN 1
  END
IF lower(@chvAction) = 'del'
  BEGIN
	Delete [Game].[ConvertSuspicious] WHERE ID = @ID

	SET @chvErrMsg	= '刪除成功'
	RETURN 1
  END
END


GO
GRANT EXECUTE
    ON OBJECT::[dbo].[PrWeb_Convert_EditSuspicious] TO [GameUser]
    AS [dbo];


GO
GRANT EXECUTE
    ON OBJECT::[dbo].[PrWeb_Convert_EditSuspicious] TO [xbuser]
    AS [dbo];

