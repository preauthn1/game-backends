-- =============================================
-- Author:  jason
-- Create date: 2019/05/28
-- Description: 充值返利活動  取得個人流水及限制
-- =============================================
CREATE PROCEDURE [dbo].[PrWeb_Event_ChargeBonusAmount_Get]	
(
	@intUserID	INT,
	@chvMsg		VARCHAR(100) OUTPUT
)
AS
BEGIN

	SET NOCOUNT ON

	DECLARE @NowTime DATETIME = GETDATE()
	DECLARE @EventID INT = 1
	DECLARE @EventStatus INT = 0
	DECLARE @PlatformID INT
	DECLARE @AreaID INT
	DECLARE @NowFlowAmount BIGINT
	DECLARE @FlowAmountLimit BIGINT
	DECLARE @Rate	BIGINT
	DECLARE @FlowAmountMultiple BIGINT


	SELECT	@NowFlowAmount = SumFlowAmount,
			@AreaID = AreaID
	FROM CenterDB.Users.UserInfo WITH (NOLOCK)
	WHERE
		UserID = @intUserID

	SELECT	@PlatformID = PlatformID
	FROM CenterDB.Area.Area WITH (NOLOCK)
	WHERE
		AreaID = @AreaID

	SELECT	@Rate = [LngValue1],
			@FlowAmountMultiple = [LngValue2]
	FROM [dbo].[EventContent] WITH (NOLOCK)
	WHERE
		[PlatformID] = @PlatformID
		AND AreaID = @AreaID
		AND EventID = @EventID	

	SELECT	@FlowAmountLimit = [ChargeAmount] * @FlowAmountMultiple
	FROM Club02.dbo.Event_ChargeBonusRecord WITH (NOLOCK)
	WHERE
		[UserID] = @intUserID

	SELECT	@EventStatus = CASE WHEN [EventStartDate] <= @NowTime AND [EventEndDate] > @NowTime AND [Status] = 1 THEN 1 ELSE 0 END
	FROM [dbo].[EventPlatformInfo] WITH (NOLOCK)
	WHERE
		[PlatformID] = @PlatformID
		AND AreaID = @AreaID
		AND EventID = @EventID


	SET @NowFlowAmount = ISNULL(@NowFlowAmount,0)
	SET @FlowAmountLimit = ISNULL(@FlowAmountLimit,0)
	SET @Rate = ISNULL(@Rate,0)
	SET @FlowAmountMultiple = ISNULL(@FlowAmountMultiple,0)
	SET @EventStatus = ISNULL(@EventStatus,0)

	SELECT	NowFlowAmount = @NowFlowAmount,
			FlowAmountLimit = @FlowAmountLimit,
			Rate = @Rate,
			FlowAmountMultiple = @FlowAmountMultiple,
			EventStatus = @EventStatus

	SET @chvMsg = '操作完成'
	RETURN 1
END
