CREATE TABLE [Notify].[ServiceTalk] (
    [UserID]      INT NOT NULL,
    [UnreadCount] INT CONSTRAINT [DF_ServiceTalk_UnreadCount] DEFAULT ((0)) NOT NULL,
    CONSTRAINT [PK_ServiceTalk] PRIMARY KEY CLUSTERED ([UserID] ASC)
);

