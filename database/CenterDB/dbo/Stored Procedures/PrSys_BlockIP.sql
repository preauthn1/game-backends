-- =============================================
-- Author:  <xxx>
-- Create date: <xxx>
-- Description:	<封锁IP>
--	2018/06/27	jason		#535 後台IP封鎖支援IP範圍設定
-- =============================================
CREATE PROCEDURE [dbo].[PrSys_BlockIP]
@chvIPAddress VARCHAR(15),
@intSec INT,
@chvMemo VARCHAR(128),
@dtexpiredate datetime = null,
@chvIPAddress2 varchar(40) = NULL
AS
SET NOCOUNT ON

if (@chvIPAddress2 is null)
begin
	set @chvIPAddress2 = @chvIPAddress
end

declare @temp_ipint1 bigint
declare @temp_ipint2 bigint

select @temp_ipint1 = [dbo].[f_IP2Int](@chvIPAddress),@temp_ipint2 = [dbo].[f_IP2Int](@chvIPAddress2)

declare @ipint1 bigint
declare @ipint2 bigint

if (@temp_ipint1 > @temp_ipint2)
begin
	set @ipint1 = @temp_ipint2
	set @ipint2 = @temp_ipint1
end
else
begin
	set @ipint1 = @temp_ipint1
	set @ipint2 = @temp_ipint2
end

declare @expiredate datetime
if (@intSec = 0)
begin
	set @expiredate = ISNULL(@dtexpiredate,GETDATE()+365)
end
else
begin
	set @expiredate = DATEADD(ss, @intSec, GETDATE())
end

if (@ipint1 > 4278190080)
begin
	set @ipint1 = 4278190080
end

if (@ipint2 > 4278190080)
begin
	set @ipint2 = 4278190080
end

IF NOT EXISTS(SELECT top 1 1 FROM Game.ForbidIPAddress WHERE [Min] <= @ipint1 and [Max] >= @ipint2)
begin
	INSERT INTO Game.ForbidIPAddress
        ( IPAddress,		Memo,		CreateTime, ExpireTime,		[IPAddress2],	[Min],		[Max] )
	SELECT @chvIPAddress,	@chvMemo,	GETDATE(),	@expiredate,	@chvIPAddress2,	@ipint1,	@ipint2
end


--IF NOT EXISTS(SELECT top 1 1 FROM Game.ForbidIPAddress WHERE IPAddress = @chvIPAddress and IPAddress2 = @chvIPAddress2)
--INSERT INTO Game.ForbidIPAddress
--        ( IPAddress, Memo, CreateTime, ExpireTime )
--SELECT @chvIPAddress, @chvMemo, GETDATE(), DATEADD(ss, @intSec, GETDATE())

RETURN 1

GO
GRANT EXECUTE
    ON OBJECT::[dbo].[PrSys_BlockIP] TO [GameUser]
    AS [dbo];


GO
GRANT EXECUTE
    ON OBJECT::[dbo].[PrSys_BlockIP] TO [xbuser]
    AS [dbo];

