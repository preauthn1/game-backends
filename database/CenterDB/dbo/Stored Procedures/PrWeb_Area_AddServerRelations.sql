-- =============================================
-- Author:  <xxx>
-- Create date: <xxx>
-- Description:	新增某渠道是否可以看到某區廳
-- =============================================
create PROCEDURE [dbo].[PrWeb_Area_AddServerRelations]
@intAreaID INT,
@intGameID INT,
@intServerID INT,
@intFlagID INT,
@tnyStatus TINYINT,
@chvErrMsg varchar(32) output,
@intRtnCode int output

AS
BEGIN

	SET NOCOUNT ON

	declare @intId int
	select @intId=Id from ServerInfo.ServerRelations with (nolock) where AreaID=@intAreaID and GameID=@intGameID and FlagID=@intFlagID and ServerID=@intServerID
	if @@ROWCOUNT = 1
	begin	
		update ServerInfo.ServerRelations set Status=@tnyStatus where Id = @intId
	end
	else
	begin
		insert into ServerInfo.ServerRelations(AreaID, GameID, FlagID, ServerID, Status)
		values (@intAreaID,@intGameID,@intFlagID,@intServerID,@tnyStatus)
	end

	set @chvErrMsg='添加成功'
	set @intRtnCode = 1
	RETURN

END
GO
GRANT EXECUTE
    ON OBJECT::[dbo].[PrWeb_Area_AddServerRelations] TO [GameUser]
    AS [dbo];

