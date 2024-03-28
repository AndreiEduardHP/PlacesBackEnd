CREATE TABLE [dbo].[Location] (
    [Id]        INT        IDENTITY (1, 1) NOT NULL,
    [latitude]  FLOAT (53) NOT NULL,
    [longitude] FLOAT (53) NOT NULL,
    CONSTRAINT [PK_Location] PRIMARY KEY CLUSTERED ([Id] ASC)
);

CREATE TABLE [dbo].[UserProfile] (
    [Id]                INT            IDENTITY (1, 1) NOT NULL,
    [FirstName]         NVARCHAR (MAX) NOT NULL,
    [LastName]          NVARCHAR (MAX) NOT NULL,
    [Username]          NVARCHAR (MAX) NOT NULL,
    [PhoneNumber]       NVARCHAR (MAX) NOT NULL,
    [Email]             NVARCHAR (MAX) NOT NULL,
    [City]              NVARCHAR (MAX) NOT NULL,
    [Interest]          NVARCHAR (MAX) NOT NULL,
    [ImageUrl]          NVARCHAR (MAX) NULL,
    [CurrentLocationId] INT            NOT NULL,
    CONSTRAINT [PK_UserProfile] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [FK_UserProfile_Location_CurrentLocationId] FOREIGN KEY ([CurrentLocationId]) REFERENCES [dbo].[Location] ([Id])
);


GO
CREATE NONCLUSTERED INDEX [IX_UserProfile_CurrentLocationId]
    ON [dbo].[UserProfile]([CurrentLocationId] ASC);


CREATE TABLE [dbo].[Connections] (
    [Id]         INT IDENTITY (1, 1) NOT NULL,
    [SenderId]   INT NOT NULL,
    [ReceiverId] INT NULL,
    [Status]     INT NOT NULL,
    CONSTRAINT [PK_Connections] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [FK_Connections_UserProfile_SenderId] FOREIGN KEY ([SenderId]) REFERENCES [dbo].[UserProfile] ([Id])
);


GO
CREATE NONCLUSTERED INDEX [IX_Connections_SenderId]
    ON [dbo].[Connections]([SenderId] ASC);


CREATE TABLE [dbo].[Events] (
    [Id]               INT            IDENTITY (1, 1) NOT NULL,
    [EventName]        NVARCHAR (MAX) NOT NULL,
    [EventDescription] NVARCHAR (MAX) NOT NULL,
    [EventTime]        DATETIME2 (7)  NOT NULL,
    [EventLocationId]  INT            NOT NULL,
    [MaxParticipants]  INT            NOT NULL,
    [Type]             INT            NOT NULL,
    [EventImage]       NVARCHAR (MAX) DEFAULT (N'') NOT NULL,
    CONSTRAINT [PK_Events] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [FK_Events_Location_EventLocationId] FOREIGN KEY ([EventLocationId]) REFERENCES [dbo].[Location] ([Id])
);


GO
CREATE NONCLUSTERED INDEX [IX_Events_EventLocationId]
    ON [dbo].[Events]([EventLocationId] ASC);

CREATE TABLE [dbo].[EventUserProfile] (
    [JoinedEventsId] INT NOT NULL,
    [UserProfilesId] INT NOT NULL,
    CONSTRAINT [PK_EventUserProfile] PRIMARY KEY CLUSTERED ([JoinedEventsId] ASC, [UserProfilesId] ASC),
    CONSTRAINT [FK_EventUserProfile_Events_JoinedEventsId] FOREIGN KEY ([JoinedEventsId]) REFERENCES [dbo].[Events] ([Id]) ON DELETE CASCADE,
    CONSTRAINT [FK_EventUserProfile_UserProfile_UserProfilesId] FOREIGN KEY ([UserProfilesId]) REFERENCES [dbo].[UserProfile] ([Id]) ON DELETE CASCADE
);


GO
CREATE NONCLUSTERED INDEX [IX_EventUserProfile_UserProfilesId]
    ON [dbo].[EventUserProfile]([UserProfilesId] ASC);

