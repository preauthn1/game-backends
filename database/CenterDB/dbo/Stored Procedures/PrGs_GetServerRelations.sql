-- =============================================
-- Author:  Eddie
-- Create date: 2019/07/16
-- Description:	^ǤiϥΪserverid
-- =============================================
CREATE PROCEDURE [dbo].[PrGs_GetServerRelations]
 
AS
SET NOCOUNT ON

/*  ^ǤiϥΪserverid  */
select AreaID,GameID,ServerID,FlagID,Status
from [ServerInfo].[ServerRelations] with (nolock)
where status = 0

Return 1
GO


