CREATE   FUNCTION   [dbo].[f_Int2IP](   
@IP   bigint   
)RETURNS   varchar(15)   
AS   
BEGIN   
DECLARE   @re   varchar(15)   
SET   @re=''   
SELECT   @re=@re+'.'+CAST(@IP/ID   as   varchar)   
,@IP=@IP%ID   
from(   
SELECT   ID=CAST(16777216   as   bigint)   
UNION   ALL   SELECT   65536   
UNION   ALL   SELECT   256   
UNION   ALL   SELECT   1)a   
RETURN(STUFF(@re,1,1,''))   
END

GO
GRANT EXECUTE
    ON OBJECT::[dbo].[f_Int2IP] TO [GameUser]
    AS [dbo];


GO
GRANT EXECUTE
    ON OBJECT::[dbo].[f_Int2IP] TO [xbuser]
    AS [dbo];

