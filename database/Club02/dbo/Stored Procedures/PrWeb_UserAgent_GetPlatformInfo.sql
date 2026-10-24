-- =============================================
-- Author:  jason
-- Create date: 2019/04/18
-- Description: 后台 admin提取PlatformID
-- =============================================
CREATE PROCEDURE [dbo].[PrWeb_UserAgent_GetPlatformInfo]
AS
BEGIN

	set nocount on

	select	distinct a.PlatformID,PlatformName = b.Name
	from CenterDB.[Area].[Area] as a with (nolock)
		inner join CenterDB.[Area].[Platform] as b with (nolock) on a.PlatformID = b.PlatformID

	return 1
END







