CREATE TABLE [Area].[Payment] (
    [ID]          INT          IDENTITY (1, 1) NOT NULL,
    [GatewayName] VARCHAR (30) NOT NULL,
    [Status]      BIT          NOT NULL,
    CONSTRAINT [PK_Payment] PRIMARY KEY CLUSTERED ([ID] ASC)
);

