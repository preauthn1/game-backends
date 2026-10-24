-- =============================================
-- Author:  <xxx>
-- Create date: <xxx>
-- Description:	<捕鱼抽水记录,捕鱼对接专用>
-- =============================================
CREATE PROCEDURE [dbo].[PrGs_FishCommission]
@ServerID		SMALLINT,				
@ShotNum		INT,                	
@Win			bigint,			 
@Commission		int		
AS

begin try
	declare @ErrorNumber int;
	declare @ErrorMessage varchar(8000);
	declare @ErrorSeverity int;
	declare @ErrorState int;
	declare @ErrorLine int;
	declare @ErrorProcedure varchar(128);

SET NOCOUNT ON

--IF @ServerID / 100 <> 23
--BEGIN
--	select -1
--	RETURN -1
--END

declare @NowTime datetime = GETDATE()

insert into Fish.dbo.FishCommission([ServerID]
      ,[ShotNum]
      ,[Win]
      ,[Commission]
      ,[CurrDate]) select @ServerID, @ShotNum, @Win, @Commission, @NowTime
      
if @Commission <> 0
begin
	insert into [Game].[FishSystaxlog]([ServerID]
      ,[TaxScale]
      ,[TaxAmount]
      ,[GameTime]) select @ServerID, '0', CAST(@Commission AS VARCHAR), @NowTime
end     


select 1
RETURN 1

end try
begin catch
	select @ErrorNumber = ERROR_NUMBER(),
	@ErrorMessage = ERROR_MESSAGE(),
	@ErrorSeverity = ERROR_SEVERITY(),
	@ErrorState = ERROR_STATE(),
	@ErrorLine = ERROR_LINE(),
	@ErrorProcedure = ERROR_PROCEDURE();

	--insert into [CenterDB].[dbo].[DBErrorLog]([ErrorNumber],[ErrorMessage],[ErrorSeverity],[ErrorState],[ErrorLine],[ErrorProcedure],[createtime])
	--select @ErrorNumber,@ErrorMessage,@ErrorSeverity,@ErrorState,@ErrorLine,@ErrorProcedure,GETDATE();

	RAISERROR(@ErrorMessage, @ErrorSeverity, @ErrorState) WITH NOWAIT;
end catch
GO
GRANT EXECUTE
    ON OBJECT::[dbo].[PrGs_FishCommission] TO [GameUser]
    AS [dbo];


GO
GRANT EXECUTE
    ON OBJECT::[dbo].[PrGs_FishCommission] TO [xbuser]
    AS [dbo];

