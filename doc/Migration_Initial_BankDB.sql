CREATE DATABASE "BankDB";
go
USE "BankDB";


IF OBJECT_ID(N'[__EFMigrationsHistory]') IS NULL
BEGIN
    CREATE TABLE [__EFMigrationsHistory] (
        [MigrationId] nvarchar(150) NOT NULL,
        [ProductVersion] nvarchar(32) NOT NULL,
        CONSTRAINT [PK___EFMigrationsHistory] PRIMARY KEY ([MigrationId])
    );
END;

GO

CREATE TABLE [Transactions] (
    [Id] uniqueidentifier NOT NULL,
    [AccountOrigin] VARCHAR(20) NOT NULL,
    [AccountDestination] VARCHAR(20) NOT NULL,
    [Value] decimal(18,2) NOT NULL,
    [Status] int NOT NULL,
    [Message] nvarchar(max) NULL,
    CONSTRAINT [PK_Transactions] PRIMARY KEY ([Id])
);

GO

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20210620181025_InitialMigration', N'3.1.14');

GO

