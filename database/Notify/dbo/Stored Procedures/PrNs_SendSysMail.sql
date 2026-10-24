-- =============================================
-- Author:		Jason
-- Create date: 2018/03/20
-- Description:	寫入郵件通知提供Center呼叫
-- =============================================
CREATE PROCEDURE [dbo].[PrNs_SendSysMail]
(
	@intUserID			INT,
	@tnyTypeID			TINYINT,
    @bintParam			BIGINT,
    @tnyStatus			TINYINT,
    @dtCreateTime		DATETIME,
    @dtLastPostTime		DATETIME
)
AS
BEGIN
	SET NOCOUNT ON

	DECLARE @UserID			INT = @intUserID
	DECLARE @TypeID			TINYINT = @tnyTypeID
	DECLARE @Param			BIGINT = @bintParam
	DECLARE @Status			TINYINT = @tnyStatus
	DECLARE @CreateTime		DATETIME = @dtCreateTime
	DECLARE @LastPostTime	DATETIME = @dtLastPostTime

	
	/*  回傳紀錄  */
	------------------------------------------------------
	INSERT INTO Notify.Notify.Attr
        (	UserID,	TypeID,		Param,	Status,	CreateTime,	LastPostTime     )
	SELECT	@UserID,@TypeID,	@Param,	@Status,@CreateTime,@LastPostTime
	------------------------------------------------------

	RETURN
END
GO
GRANT EXECUTE
    ON OBJECT::[dbo].[PrNs_SendSysMail] TO [xbuser]
    AS [dbo];

