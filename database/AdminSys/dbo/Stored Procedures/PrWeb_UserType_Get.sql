

-- =============================================
-- Author:  <xxx>
-- Create date: <xxx>
-- Description:	<取得使用者类别>
-- =============================================
create PROCEDURE [dbo].[PrWeb_UserType_Get]
AS
BEGIN

	SET NOCOUNT ON
	Select * from [Admin].[UserType] with (nolock)
END

