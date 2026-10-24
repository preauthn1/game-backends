






-- =============================================
-- Author:  harry
-- Create date: 2018/08/31
-- Description:	新增登入纪录
--
-- =============================================
CREATE PROCEDURE [dbo].[PrWeb_LogonLog_Create]
	@UID		uniqueidentifier,  --登入者
	@chvIPAddress		varchar(64),
	@intYYYYMMDD		int
AS
BEGIN	

	INSERT INTO  dbo.LogonLog( ID, IPAddress, YYYYMMDD) 
	Values (@UID, @chvIPAddress, @intYYYYMMDD)

	RETURN 1

END



