-- =============================================
-- Author:  alan
-- Create date: 20181226
-- Description:	XML to json
-- =============================================
CREATE FUNCTION [dbo].[Fn_xml2Json]
(
	@chvXML xml
)
RETURNS varchar(max)
AS
BEGIN

--XML to Json
DECLARE @Json varchar(1000)
SELECT @Json=Stuff(  
  (SELECT * from  
    (SELECT ',{'+  
      Stuff((SELECT ',"'+coalesce(b.c.value('local-name(.)', 'NVARCHAR(MAX)'),'')+'":"'+
                    b.c.value('text()[1]','NVARCHAR(MAX)') +'"'
               
             from x.a.nodes('*') b(c)  
             for xml path(''),TYPE).value('(./text())[1]','NVARCHAR(MAX)')
        ,1,1,'')+'}' 
   from @chvXML.nodes('//*') x(a)    --from @chvXML.nodes('/root/*') x(a)  // if define "FOR XML path,root"
   ) JSON(theLine)  
  for xml path(''),TYPE).value('.','NVARCHAR(MAX)' )
,1,1,'')  


Return '[' + @Json + ']'

END

GO
GRANT EXECUTE
    ON OBJECT::[dbo].[Fn_xml2Json] TO [GameUser]
    AS [dbo];


GO
GRANT EXECUTE
    ON OBJECT::[dbo].[Fn_xml2Json] TO [xbuser]
    AS [dbo];

