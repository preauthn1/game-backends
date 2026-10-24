CREATE TABLE [Pay].[ConvertGateway] (
    [ID]          INT          NOT NULL,
    [GatewayName] VARCHAR (20) NOT NULL,
    [ConvertType] TINYINT      NOT NULL,
    [Status]      TINYINT      NOT NULL,
    [Company]     INT          NOT NULL,
    CONSTRAINT [PK_ConvertGateway] PRIMARY KEY CLUSTERED ([ID] ASC)
);


GO
EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'营运商编号', @level0type = N'SCHEMA', @level0name = N'Pay', @level1type = N'TABLE', @level1name = N'ConvertGateway', @level2type = N'COLUMN', @level2name = N'Company';


GO
EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'开关', @level0type = N'SCHEMA', @level0name = N'Pay', @level1type = N'TABLE', @level1name = N'ConvertGateway', @level2type = N'COLUMN', @level2name = N'Status';


GO
EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'代付方式', @level0type = N'SCHEMA', @level0name = N'Pay', @level1type = N'TABLE', @level1name = N'ConvertGateway', @level2type = N'COLUMN', @level2name = N'ConvertType';


GO
EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'代付商名称', @level0type = N'SCHEMA', @level0name = N'Pay', @level1type = N'TABLE', @level1name = N'ConvertGateway', @level2type = N'COLUMN', @level2name = N'GatewayName';


GO
EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'代付商编号', @level0type = N'SCHEMA', @level0name = N'Pay', @level1type = N'TABLE', @level1name = N'ConvertGateway', @level2type = N'COLUMN', @level2name = N'ID';

