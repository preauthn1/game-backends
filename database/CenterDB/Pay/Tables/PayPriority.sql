CREATE TABLE [Pay].[PayPriority] (
    [ID]        INT     IDENTITY (1, 1) NOT NULL,
    [GatewayID] INT     NOT NULL,
    [PayTypeID] INT     NOT NULL,
    [Priority]  TINYINT NOT NULL,
    [Status]    BIT     CONSTRAINT [DF_PayPriority_Status] DEFAULT ((1)) NOT NULL,
    CONSTRAINT [PK_Pay_PaymentPriorityID] PRIMARY KEY CLUSTERED ([ID] ASC)
);

