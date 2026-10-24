CREATE PROCEDURE [dbo].[PrTF_UserLogon]
@intAreaID	int,
@intUserID	int,
@chvHDcode varchar(64),
@chvIPAddress varchar(30)
AS
SET NOCOUNT ON

DECLARE @intID int=0
DECLARE @intReviewAreaID int= 0


Insert into Apple.ReviewHistory(AreaID,UserID,HDCode,IPAddress)
values(@intAreaID,@intUserID,@chvHDcode,@chvIPAddress)



Select @intReviewAreaID = AreaID from Apple.ReviewArea Where AreaID=@intAreaID
Select top 1 @intID = id from [Apple].[ReviewUser] with (nolock) where HDcode = @chvHDcode


IF @intID = 0 --帳號不存在
Begin
	
	IF @intReviewAreaID > 0 --送審階段
	Begin
		
		--新增apple審核員
		Insert into [Apple].[ReviewUser](UserID,HDCode,LastLogonIPAddress,LastLogonTime)
		values (@intUserID,@chvHDcode,@chvIPAddress,getdate())
		Set @intID = @@IDENTITY
		
		--紀錄IP
		IF not exists (Select 1 from Apple.IP with (nolock) where IP = @chvIPAddress and areaid = @intAreaID)
			Insert into Apple.IP(AreaID,IP) values (@intAreaID, @chvIPAddress)
	End

End
Else
Begin

	Update [Apple].[ReviewUser] Set HDCode = @chvHDcode, LastLogonIPAddress = @chvIPAddress, LastLogonTime=getdate()
	Where Id = @intID
End

--送審階段紀錄IP
IF @intReviewAreaID > 0
Begin
	IF not exists (Select 1 from Apple.IP with (nolock) where IP = @chvIPAddress and areaid = @intAreaID)
		Insert into Apple.IP(AreaID,IP) values (@intAreaID, @chvIPAddress)
End


--取得apple審核員資料
select UserID,UserFlag,HDCode,LastLogonTime,LastLogonIPAddress from Apple.ReviewUser with (nolock) Where Id = @intID
--取得審核的areaID 
select @intReviewAreaID as ReviewAreaID
--取得曾經紀錄的IP
Select IP from Apple.IP with (nolock) where IP = @chvIPAddress and areaid = @intAreaID
GO
GRANT EXECUTE
    ON OBJECT::[dbo].[PrTF_UserLogon] TO [GameUser]
    AS [dbo];


GO
GRANT EXECUTE
    ON OBJECT::[dbo].[PrTF_UserLogon] TO [xbuser]
    AS [dbo];

