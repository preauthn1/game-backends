CREATE TABLE [Game].[GameUserList] (
    [UserID]   INT      NOT NULL,
    [ServerID] SMALLINT NOT NULL,
    [HDType]   TINYINT  NOT NULL
);


GO
CREATE NONCLUSTERED INDEX [IDX_GameUserList_UserID]
    ON [Game].[GameUserList]([UserID] ASC)
    INCLUDE([ServerID]);

