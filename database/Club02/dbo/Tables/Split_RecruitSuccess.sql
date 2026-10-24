CREATE TABLE [dbo].[Split_RecruitSuccess] (
    [UserID]        INT NOT NULL,
    [RecruitCountB] INT CONSTRAINT [DF_Split_RecruitSuccess_RecruitCountB] DEFAULT ((0)) NULL,
    [RecruitCountC] INT CONSTRAINT [DF_Split_RecruitSuccess_RecruitCountC] DEFAULT ((0)) NULL,
    [RecruitCountD] INT CONSTRAINT [DF_Split_RecruitSuccess_RecruitCountD] DEFAULT ((0)) NULL,
    [SuccessCountB] INT CONSTRAINT [DF_Split_RecruitSuccess_SuccessCountB] DEFAULT ((0)) NULL,
    [SuccessCountC] INT CONSTRAINT [DF_Split_RecruitSuccess_SuccessCountC] DEFAULT ((0)) NULL,
    [SuccessCountD] INT CONSTRAINT [DF_Split_RecruitSuccess_SuccessCountD] DEFAULT ((0)) NULL,
    CONSTRAINT [PK_Split_RecruitSuccess] PRIMARY KEY CLUSTERED ([UserID] ASC)
);

