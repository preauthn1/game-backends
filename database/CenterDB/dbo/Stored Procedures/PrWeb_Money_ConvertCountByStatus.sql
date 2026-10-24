-- =============================================
-- Author:  <xxx>
-- Create date: <xxx>
-- Description:	<查询未处理兑换申请数>
--	2018/03/22	Jason		調整寫法及改為呼叫LINKEDSERVER SP
--	2018/06/13	Jason		@chvPlatformIDs VARCHAR(32) change to VARCHAR(2000)
-- =============================================
CREATE PROCEDURE [dbo].[PrWeb_Money_ConvertCountByStatus]
(
	@chvPlatformIDs	VARCHAR(4000) = NULL,	--平台ID
	@tnyStatus		TINYINT,			--申请状态
	@intCount		INT		 OUTPUT		--数量
)
AS
BEGIN
	
	SET NOCOUNT ON
	select @intCount = COUNT(1) from  logdb.Money.ConvertLog with (nolock) where status =@tnyStatus
	and PlatformID in (select value from dbo.fn_Split(@chvPlatformIDs,','))
	return 
		
END

GO
GRANT EXECUTE
    ON OBJECT::[dbo].[PrWeb_Money_ConvertCountByStatus] TO [GameUser]
    AS [dbo];


GO
GRANT EXECUTE
    ON OBJECT::[dbo].[PrWeb_Money_ConvertCountByStatus] TO [xbuser]
    AS [dbo];

