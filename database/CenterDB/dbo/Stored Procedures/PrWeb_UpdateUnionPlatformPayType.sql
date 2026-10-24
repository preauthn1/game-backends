-- =============================================
-- Author:  <xxx>
-- Create date: <xxx>
-- Description:	<更新平台支付开关>
-- =============================================
CREATE PROCEDURE [dbo].[PrWeb_UpdateUnionPlatformPayType]
	@intAdminId		int,
	@intPlatformID	int,
	@chvPayTypeString varchar(max) = ''
AS
BEGIN

	--營運商篩選
	DECLARE @Temp_PlatformID TABLE (PlatformID	INT, Company INT)
	INSERT INTO @Temp_PlatformID 
	Select DISTINCT 
	       A.PlatformID,
	       P.Company
	From CenterDB.Area.PlatformAdminUser as A
	JOIN CenterDB.Area.[Platform] as P
	  ON A.PlatformID = P.PlatformID
	where AdminID=@intAdminId

	declare @FullstringDelimiter varchar(1) = '|'
	declare @LevelstringDelimiter varchar(1) = ','
	declare @temp_levelstringsplit table (Num int,levelstring varchar(100))

	insert into @temp_levelstringsplit
	select	[id],replace([Value],',','.')
	from [dbo].[fn_Split](@chvPayTypeString,@FullstringDelimiter)
	where [Value] != '' AND  [Value] IS NOT NULL

	--依營運商可使用支付方式先全部關閉
	UPDATE [CenterDB].[Pay].UnionPlatformPayType 
	   SET Status = 0 
	  FROM [CenterDB].[Pay].UnionPlatformPayType as U
	  LEFT JOIN [CenterDB].[Pay].PayGateway as G
	    ON U.GWID = G.ID
	  JOIN @Temp_PlatformID as P
	    ON G.Company = P.Company	  
	 WHERE U.PlatformID = @intPlatformID

	--更新支付開關
	MERGE [CenterDB].[Pay].UnionPlatformPayType AS T
	USING (SELECT PlatformID = @intPlatformID,
				  GWID = cast(PARSENAME(levelstring,3) as int),
				  TypeID = cast(PARSENAME(levelstring,2) as int),
				  [Status] = cast(PARSENAME(levelstring,1) as tinyint)
			FROM @temp_levelstringsplit) AS S
	  ON T.PlatformID = S.PlatformID AND T.GWID = S.GWID AND T.TypeID = S.TypeID
	WHEN MATCHED THEN
		 UPDATE SET [Status] = S.[Status]
	WHEN NOT MATCHED THEN
		INSERT (PlatformID, GWID, TypeID, [Status], Popular)
		VALUES (S.PlatformID, S.GWID, S.TypeID,  S.[Status], 0);

	return 1

END
