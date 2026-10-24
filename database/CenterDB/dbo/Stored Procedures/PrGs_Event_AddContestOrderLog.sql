

-- =============================================
-- Author:  alan
-- Create date: 20181211
-- Description: 紀錄充值
-- =============================================
CREATE PROCEDURE [dbo].[PrGs_Event_AddContestOrderLog]
(
	@intUserID int,
	@lngAmount bigint
)
AS
BEGIN

	SET NOCOUNT ON
	Declare @dtNow datetime=GETDATE()
	Declare @intYYYYMMDD int=CAST(CONVERT(varchar,@dtNow,112) AS INT)

	Insert into Event.ContestUserOrderLog(YYYYMMDD, UserId, Amount, CreateTime) values(@intYYYYMMDD, @intUserID, @lngAmount, @dtNow)
	
END

GO
GRANT EXECUTE
    ON OBJECT::[dbo].[PrGs_Event_AddContestOrderLog] TO [GameUser]
    AS [dbo];


GO
GRANT EXECUTE
    ON OBJECT::[dbo].[PrGs_Event_AddContestOrderLog] TO [xbuser]
    AS [dbo];

