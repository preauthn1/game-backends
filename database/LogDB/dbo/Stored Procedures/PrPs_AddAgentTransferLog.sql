
-- =============================================
-- Author:		Jason
-- Create date: 2018/03/21
-- Description:	寫入代理轉帳功能紀錄
--	2018/04/03	Jason		新增PlatformID AreaID參數及欄位新增
-- =============================================
CREATE PROCEDURE [dbo].[PrPs_AddAgentTransferLog]
(
	@intAgentID			INT = NULL,
	@intRelationUserID	INT = NULL,
	@intRelationAgentID	INT = NULL,
	@lngSelfAmount		BIGINT = NULL,
	@lngRelationAmount	BIGINT = NULL,
	@lngAmount			BIGINT = NULL,
	@chvIPAddress		VARCHAR(15) = NULL,
	@dtStatTime			DATETIME = NULL,
	@intPlatformID		INT = NULL,
	@intAreaID			INT = NULL
)
AS
BEGIN

	SET NOCOUNT ON

	DECLARE	@AgentID			INT = @intAgentID
	DECLARE	@RelationUserID		INT = @intRelationUserID
	DECLARE	@RelationAgentID	INT = @intRelationAgentID
	DECLARE	@SelfAmount			BIGINT = @lngSelfAmount
	DECLARE	@RelationAmount		BIGINT = @lngRelationAmount
	DECLARE	@Amount				BIGINT = @lngAmount
	DECLARE	@IPAddress			VARCHAR(15) = @chvIPAddress
	DECLARE	@StatTime			DATETIME = @dtStatTime
	DECLARE @PlatformID			INT = @intPlatformID
	DECLARE @AreaID				INT = @intAreaID

	/*  回傳資料  */
	-------------------------------------------------------------------------------------------
	INSERT INTO [Agent].[AgentTransferLog]
		(	
			AgentID,
			SourceID,
			RelationUserID,
			RelationAgentID,
			SelfAmount,
			RelationAmount,
			UpdateAmount,
			IPAddress,
			CreateTime,
			PlatformID,
			AreaID
		)
	SELECT	@AgentID,
			CASE WHEN @RelationAgentID IS NOT NULL THEN 10001 ELSE 10002 END,
			@RelationUserID,
			@RelationAgentID,
			@SelfAmount,
			@RelationAmount,
			ABS(@Amount)*-1,
			@IPAddress,
			@StatTime,
			@PlatformID,
			@AreaID
	UNION ALL
	SELECT	@AgentID,
			10000,
			@RelationUserID,
			@RelationAgentID,
			@SelfAmount,
			@RelationAmount,
			ABS(@Amount),
			@IPAddress,
			@StatTime,
			@PlatformID,
			@AreaID
	WHERE
		@intRelationAgentID IS NOT NULL
	-------------------------------------------------------------------------------------------

END
GO
GRANT EXECUTE
    ON OBJECT::[dbo].[PrPs_AddAgentTransferLog] TO [GameUser]
    AS [dbo];


GO
GRANT EXECUTE
    ON OBJECT::[dbo].[PrPs_AddAgentTransferLog] TO [xbuser]
    AS [dbo];

