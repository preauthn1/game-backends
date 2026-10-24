-- =============================================
-- Author:  <xxx>
-- Create date: <xxx>
-- Description:	<更新用户证书和广场信息>
-- =============================================
CREATE PROCEDURE [dbo].[PrPs_UpdateUserCerAndHall]
@intUserID INT,                    --用户ID
@tnyHDType TINYINT,                --客户端类型
@chvCertification VARCHAR(32) OUTPUT,     --证书
@sintServerID smallint              --广场服务器ID
AS
SET NOCOUNT ON
DECLARE @chvExCer VARCHAR(32),@intRet int
SET @intRet = 0

SELECT @chvExCer = CerCode
FROM Game.Certification WITH(NOLOCK)
WHERE UserID = @intUserID 

--更新证书信息
IF @chvExCer IS NULL
INSERT INTO Game.Certification
        ( UserID ,
          CerCode
        )
       SELECT
       @intUserID,
       @chvCertification
else
set @chvCertification = @chvExCer

--更新广场在线信息
UPDATE Game.HallUserList
SET ServerID = @sintServerID,
	HDType = @tnyHDType,
	CreateTime = GETDATE()
WHERE UserID = @intUserID

IF @@ROWCOUNT = 0
INSERT INTO Game.HallUserList
(UserID, ServerID,HDType)
SELECT
@intUserID, @sintServerID,@tnyHDType
      
RETURN 0

GO
GRANT EXECUTE
    ON OBJECT::[dbo].[PrPs_UpdateUserCerAndHall] TO [GameUser]
    AS [dbo];


GO
GRANT EXECUTE
    ON OBJECT::[dbo].[PrPs_UpdateUserCerAndHall] TO [xbuser]
    AS [dbo];

