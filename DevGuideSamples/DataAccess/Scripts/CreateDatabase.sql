USE [master]
GO

IF NOT EXISTS (SELECT [name] FROM sys.databases WHERE [name] = N'DataAccessExamples')
	CREATE DATABASE [DataAccessExamples]
GO
