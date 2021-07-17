IF NOT EXISTS(SELECT * FROM sys.databases WHERE name = 'BankDB')
BEGIN
    CREATE DATABASE BankDB;  
END
GO

USE BankDB;
GO
	    
IF OBJECT_ID(N'[Transactions]') IS NULL
BEGIN
	CREATE TABLE [Transactions] (
		[Id] uniqueidentifier NOT NULL,
		[AccountOrigin] VARCHAR(20) NOT NULL,
		[AccountDestination] VARCHAR(20) NOT NULL,
		[Value] decimal(18,2) NOT NULL,
		[Status] int NOT NULL,
		[Message] nvarchar(max) NULL,
		CONSTRAINT [PK_Transactions] PRIMARY KEY ([Id])
	);
END
GO


