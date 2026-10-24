CREATE TABLE [Game].[Fish_BeginnerOffer] (
    [ID]         INT      IDENTITY (1, 1) NOT NULL,
    [UserID]     INT      NOT NULL,
    [Bullet]     INT      NOT NULL,
    [Score]      INT      NOT NULL,
    [Status]     TINYINT  NOT NULL,
    [CreateTime] DATETIME CONSTRAINT [DF_Fish_BeginnerOffer_CreateTime] DEFAULT (getdate()) NOT NULL,
    CONSTRAINT [PK_Fish_BeginnerOffer] PRIMARY KEY CLUSTERED ([ID] ASC)
);


GO
CREATE NONCLUSTERED INDEX [IX_BeginnerOffer_UserID]
    ON [Game].[Fish_BeginnerOffer]([UserID] ASC)
    INCLUDE([ID], [Bullet], [Score], [Status], [CreateTime]);

