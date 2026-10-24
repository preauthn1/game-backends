

-- =============================================
-- Author:  alan
-- Create date: 20181211
-- Description: 新增/更新比賽活動充值贈送設定
-- =============================================
CREATE PROCEDURE [dbo].[PrWeb_Event_AddBonusConfig]
(
	@intID	int=null, --主key
	@intContestID int, --場次ID
	@lngOrderReach bigint, --充值到達金額(單位:分)
	@intFreeTicket int, --免費次數
	@chvRtnMsg	varchar(50) = '' OUTPUT, --錯誤訊息
	@intRtnCode int OUTPUT --錯誤代碼
)
AS
BEGIN

	SET NOCOUNT ON

	DECLARE @intPrevFreeTicket int=0
	DECLARE @intNextFreeTicket int=0
	
	--檢查是否有重複金額
	IF EXISTS( Select 1 from Event.Bonus where ContestID=@intContestID and OrderReach=@lngOrderReach and (@intID is null or ID <> @intID) ) --如果是新增資料ID=null, 如果是更新資料檢查其它ID的金額是否重複
	BEGIN
		SET @chvRtnMsg = '充值金额重复'
		SET @intRtnCode = -1
		RETURN
	END

	--找同場次前一個設定的免費次數
	select TOP 1 @intPrevFreeTicket=ISNULL(FreeTicket,0) from [Event].[Bonus] where ContestID=@intContestID and OrderReach < @lngOrderReach ORDER by OrderReach DESC
	--不能低於或等於同場次前一個設定的免費次數
	IF @intFreeTicket <= @intPrevFreeTicket AND @intNextFreeTicket <> 0
	BEGIN
		SET @chvRtnMsg = '免费次数设定错误'
		SET @intRtnCode = -1
		RETURN
	END

	--找同場次下一個設定的免費次數
	select TOP 1 @intNextFreeTicket=ISNULL(FreeTicket,0) from [Event].[Bonus] where ContestID=@intContestID and OrderReach > @lngOrderReach ORDER by OrderReach ASC
	--不能高於或等於同場次下一個設定的免費次數
	IF @intFreeTicket >= @intNextFreeTicket AND @intNextFreeTicket <> 0
	BEGIN
		SET @chvRtnMsg = '免费次数设定错误'
		SET @intRtnCode = -1
		RETURN
	END

	IF @intID is null
	BEGIN

		Insert Event.Bonus(ContestID,OrderReach,FreeTicket) values(@intContestID,@lngOrderReach,@intFreeTicket) 		
	END
	ELSE
	BEGIN
		
		Update Event.Bonus SET OrderReach=@lngOrderReach, FreeTicket=@intFreeTicket where ID = @intID
	END

	SET @chvRtnMsg = '操作成功'
	SET @intRtnCode = 1
END

GO
GRANT EXECUTE
    ON OBJECT::[dbo].[PrWeb_Event_AddBonusConfig] TO [GameUser]
    AS [dbo];


GO
GRANT EXECUTE
    ON OBJECT::[dbo].[PrWeb_Event_AddBonusConfig] TO [xbuser]
    AS [dbo];

