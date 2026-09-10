USE LegacyBanking;
GO

INSERT INTO Customers
(CustomerNumber,FullName,Email,Mobile,NationalId,DateOfBirth,KycStatus,CreatedOn)
VALUES
('CUS-1001','Alice Morgan','alice@example.com','9000000001','NID-1111','1988-05-10','APPROVED',GETDATE()),
('CUS-1002','Bob Taylor','bob@example.com','9000000002','NID-2222','1985-09-21','APPROVED',GETDATE());

INSERT INTO Accounts
(AccountNumber,CustomerId,AccountType,Balance,Currency,Status,OpenedOn)
VALUES
('ACC-10001',1,'SAVINGS',25000,'INR','ACTIVE',GETDATE()),
('ACC-10002',2,'SAVINGS',15000,'INR','ACTIVE',GETDATE()),
('ACC-10003',1,'CURRENT',90000,'INR','ACTIVE',GETDATE());

INSERT INTO Transactions
(Reference,AccountId,Type,Amount,Description,TransactionDate,Status)
VALUES
('TXN-SEED-1',1,'CREDIT',25000,'Opening balance',GETDATE(),'POSTED'),
('TXN-SEED-2',2,'CREDIT',15000,'Opening balance',GETDATE(),'POSTED');
