-- =============================================
-- Author:  alan
-- Create date: 2018/06/11
-- Description:	#419 紀錄註冊使用者的設備資訊
-- =============================================
CREATE PROCEDURE [dbo].[PrWeb_User_AddPreRegHD]
(
	@chvIPAddress varchar(40),
	@chvHDCode	VARCHAR(64),
	@chvHDInfo	VARCHAR(2000)
)
AS
BEGIN	
	SET NOCOUNT ON
	INSERT INTO [Log].[PreRegHD](IPAddress,HDcode,HDInfo) 
	VALUES(@chvIPAddress,@chvHDCode,@chvHDInfo)
END
GO
GRANT EXECUTE
    ON OBJECT::[dbo].[PrWeb_User_AddPreRegHD] TO [GameUser]
    AS [dbo];


GO
GRANT EXECUTE
    ON OBJECT::[dbo].[PrWeb_User_AddPreRegHD] TO [xbuser]
    AS [dbo];

