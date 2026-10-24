EXECUTE sp_addrolemember @rolename = N'db_owner', @membername = N'lisa';


GO
EXECUTE sp_addrolemember @rolename = N'db_datareader', @membername = N'GameUser';


GO
EXECUTE sp_addrolemember @rolename = N'db_datareader', @membername = N'xbuser';


GO
EXECUTE sp_addrolemember @rolename = N'db_datawriter', @membername = N'xbuser';

