CREATE TABLE [Money].[Bank] (
    [UserID] INT          NOT NULL,
    [Amount] BIGINT       NOT NULL,
    [Code]   VARCHAR (32) CONSTRAINT [DF_Bank_Code] DEFAULT ('') NOT NULL,
    CONSTRAINT [PK_Bank] PRIMARY KEY CLUSTERED ([UserID] ASC) ON [S_UserID] ([UserID])
) ON [S_UserID] ([UserID]);

