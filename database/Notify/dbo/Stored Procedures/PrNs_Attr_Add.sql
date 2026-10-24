-- =============================================
-- Author:		Jason
-- Create date: 2018/03/21
-- Description:	新增屬性通知
-- =============================================
CREATE PROCEDURE [dbo].[PrNs_Attr_Add]
(
	@intUserID		INT,
	@tnyTypeID		TINYINT,
	@lngParam		BIGINT = NULL,
	@tnyStatus		TINYINT,
	@dtStatTime		DATETIME
)
AS
BEGIN

	SET NOCOUNT ON

	SET @lngParam	=	ISNULL(@lngParam,0)

	INSERT INTO Notify.Notify.Attr
		(	UserID,		TypeID ,	Param ,		Status ,	CreateTime ,	LastPostTime	)
	VALUES
		(	@intUserID,	@tnyTypeID,	@lngParam,	@tnyStatus,	@dtStatTime,	@dtStatTime	)

END
GO
GRANT EXECUTE
    ON OBJECT::[dbo].[PrNs_Attr_Add] TO [xbuser]
    AS [dbo];

