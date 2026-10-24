CREATE TABLE [Game].[Certification] (
    [UserID]  INT          NOT NULL,
    [CerCode] VARCHAR (32) NOT NULL,
    CONSTRAINT [PK_Certification] PRIMARY KEY CLUSTERED ([UserID] ASC)
);

