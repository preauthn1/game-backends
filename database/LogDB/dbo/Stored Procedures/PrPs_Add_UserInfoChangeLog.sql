-- =============================================
-- Author:		Jason
-- Create date: 2018/03/20
-- Description:	手機驗證找回銀行密碼紀錄
-- =============================================
CREATE PROCEDURE [dbo].[PrPs_Add_UserInfoChangeLog]
(
	@intUserID		INT,
	@tnyChangeType	TINYINT,
	@chrOldData		VARCHAR(1024),
	@chrNewData		VARCHAR(1024),
	@chrIPAddress	VARCHAR(15),
	@chrMemo		VARCHAR(128),
	@dtWriteTime	DATETIME
)
AS
BEGIN
	SET NOCOUNT ON
	
	SET @chrOldData	=	ISNULL(@chrOldData,' ')
	SET @chrNewData	=	ISNULL(@chrNewData,' ')
		
	/*  回傳紀錄  */
	------------------------------------------------------
	INSERT INTO [Users].[UserInfoChangeLog]
		([UserID],	[ChangeType],	[OldData],	[NewData],	[IPAddress],	[Memo],	[WriteTime]
		)
	VALUES
		(@intUserID,@tnyChangeType,	@chrOldData,@chrNewData,@chrIPAddress,	@chrMemo,@dtWriteTime)
	------------------------------------------------------

	RETURN
END
GO
GRANT EXECUTE
    ON OBJECT::[dbo].[PrPs_Add_UserInfoChangeLog] TO [GameUser]
    AS [dbo];


GO
GRANT EXECUTE
    ON OBJECT::[dbo].[PrPs_Add_UserInfoChangeLog] TO [xbuser]
    AS [dbo];

