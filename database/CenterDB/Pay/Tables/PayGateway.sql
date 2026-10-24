CREATE TABLE [Pay].[PayGateway] (
    [ID]          INT          NOT NULL,
    [GatewayName] VARCHAR (20) NOT NULL,
    [Priority]    TINYINT      NOT NULL,
    [Company]     INT          NOT NULL,
    CONSTRAINT [PK_PayGateway] PRIMARY KEY CLUSTERED ([ID] ASC)
);

