CREATE TABLE [Apple].[ReviewHistory] (
    [LogID]      INT          IDENTITY (1, 1) NOT NULL,
    [AreaID]     INT          NULL,
    [UserID]     INT          NULL,
    [HDCode]     VARCHAR (50) NULL,
    [IPAddress]  VARCHAR (25) NULL,
    [CreateTime] DATETIME     CONSTRAINT [DF_Apple.ReviewHistory_CreateTime] DEFAULT (getdate()) NULL,
    CONSTRAINT [PK_Apple.ReviewHistory] PRIMARY KEY CLUSTERED ([LogID] ASC)
);

