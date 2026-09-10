CREATE DATABASE LegacyBanking;
GO

USE LegacyBanking;
GO

CREATE TABLE Customers
(
    Id INT IDENTITY(1,1) PRIMARY KEY,
    CustomerNumber NVARCHAR(50) NOT NULL,
    FullName NVARCHAR(200) NOT NULL,
    Email NVARCHAR(200) NULL,
    Mobile NVARCHAR(50) NULL,
    NationalId NVARCHAR(100) NULL,
    DateOfBirth DATETIME NULL,
    KycStatus NVARCHAR(30) NULL,
    CreatedOn DATETIME NOT NULL
);

CREATE TABLE Accounts
(
    Id INT IDENTITY(1,1) PRIMARY KEY,
    AccountNumber NVARCHAR(50) NOT NULL,
    CustomerId INT NOT NULL,
    AccountType NVARCHAR(30) NOT NULL,
    Balance DECIMAL(18,2) NOT NULL,
    Currency NVARCHAR(3) NOT NULL,
    Status NVARCHAR(20) NOT NULL,
    OpenedOn DATETIME NOT NULL,
    CONSTRAINT FK_Accounts_Customers
        FOREIGN KEY(CustomerId) REFERENCES Customers(Id)
);

CREATE TABLE Transactions
(
    Id BIGINT IDENTITY(1,1) PRIMARY KEY,
    Reference NVARCHAR(100) NOT NULL,
    AccountId INT NOT NULL,
    Type NVARCHAR(20) NOT NULL,
    Amount DECIMAL(18,2) NOT NULL,
    Description NVARCHAR(500) NULL,
    TransactionDate DATETIME NOT NULL,
    Status NVARCHAR(30) NOT NULL,
    CONSTRAINT FK_Transactions_Accounts
        FOREIGN KEY(AccountId) REFERENCES Accounts(Id)
);

CREATE TABLE Loans
(
    Id INT IDENTITY(1,1) PRIMARY KEY,
    LoanNumber NVARCHAR(50) NOT NULL,
    CustomerId INT NOT NULL,
    Principal DECIMAL(18,2) NOT NULL,
    InterestRate DECIMAL(9,4) NOT NULL,
    TermMonths INT NOT NULL,
    OutstandingBalance DECIMAL(18,2) NOT NULL,
    Status NVARCHAR(30) NOT NULL,
    StartDate DATETIME NOT NULL
);

CREATE TABLE AuditLogs
(
    Id BIGINT IDENTITY(1,1) PRIMARY KEY,
    UserName NVARCHAR(200) NULL,
    Action NVARCHAR(100) NULL,
    EntityName NVARCHAR(100) NULL,
    EntityId NVARCHAR(100) NULL,
    Payload NVARCHAR(MAX) NULL,
    CreatedOn DATETIME NOT NULL
);
