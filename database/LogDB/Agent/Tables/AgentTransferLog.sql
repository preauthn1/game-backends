CREATE TABLE [Agent].[AgentTransferLog] (
    [Id]              INT          IDENTITY (1, 1) NOT NULL,
    [AgentID]         INT          NOT NULL,
    [SourceID]        INT          NOT NULL,
    [RelationUserID]  INT          NOT NULL,
    [RelationAgentID] INT          NULL,
    [SelfAmount]      BIGINT       NOT NULL,
    [RelationAmount]  BIGINT       NOT NULL,
    [UpdateAmount]    BIGINT       NOT NULL,
    [IPAddress]       VARCHAR (15) NOT NULL,
    [CreateTime]      DATETIME     NOT NULL,
    [PlatformID]      INT          NULL,
    [AreaID]          INT          NULL
);

