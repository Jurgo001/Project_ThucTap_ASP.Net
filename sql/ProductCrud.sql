IF DB_ID(N'ProductCrudDb') IS NULL
BEGIN
    CREATE DATABASE ProductCrudDb;
END
GO

USE ProductCrudDb;
GO

IF OBJECT_ID(N'dbo.ProductList', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.ProductList
    (
        Id           INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
        ProductCode  NVARCHAR(50) NOT NULL,
        ProductName  NVARCHAR(200) NOT NULL,
        Price        DECIMAL(18,2) NOT NULL CONSTRAINT DF_ProductList_Price DEFAULT (0),
        Quantity     INT NOT NULL CONSTRAINT DF_ProductList_Quantity DEFAULT (0),
        IsActive     BIT NOT NULL CONSTRAINT DF_ProductList_IsActive DEFAULT (1),
        IsDeleted    BIT NOT NULL CONSTRAINT DF_ProductList_IsDeleted DEFAULT (0),
        CreatedDate  DATETIME2 NOT NULL CONSTRAINT DF_ProductList_CreatedDate DEFAULT (GETUTCDATE()),
        ModifiedDate DATETIME2 NULL,
        CreatedBy    INT NULL
    );
END
GO

IF EXISTS
(
    SELECT 1
    FROM sys.key_constraints
    WHERE name = N'UQ_ProductList_ProductCode'
      AND parent_object_id = OBJECT_ID(N'dbo.ProductList')
)
BEGIN
    ALTER TABLE dbo.ProductList
        DROP CONSTRAINT UQ_ProductList_ProductCode;
END
GO

IF NOT EXISTS
(
    SELECT 1
    FROM sys.indexes
    WHERE name = N'UX_ProductList_ProductCode_Active'
      AND object_id = OBJECT_ID(N'dbo.ProductList')
)
BEGIN
    CREATE UNIQUE INDEX UX_ProductList_ProductCode_Active
        ON dbo.ProductList(ProductCode)
        WHERE IsDeleted = 0;
END
GO

IF OBJECT_ID(N'dbo.AppUsers', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.AppUsers
    (
        Id           INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
        Username     NVARCHAR(100) NOT NULL,
        PasswordHash NVARCHAR(500) NOT NULL,
        Role         NVARCHAR(50) NOT NULL,
        IsActive     BIT NOT NULL CONSTRAINT DF_AppUsers_IsActive DEFAULT (1),
        CreatedDate  DATETIME2 NOT NULL CONSTRAINT DF_AppUsers_CreatedDate DEFAULT (GETUTCDATE()),
        CONSTRAINT UQ_AppUsers_Username UNIQUE (Username)
    );
END
GO

IF OBJECT_ID(N'dbo.AuditLogs', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.AuditLogs
    (
        Id           BIGINT IDENTITY(1,1) NOT NULL PRIMARY KEY,
        UserId       INT NULL,
        Username     NVARCHAR(100) NOT NULL,
        Action       NVARCHAR(20) NOT NULL,
        EntityName   NVARCHAR(100) NOT NULL,
        EntityId     NVARCHAR(100) NULL,
        Description  NVARCHAR(500) NOT NULL,
        CreatedDate  DATETIME2 NOT NULL CONSTRAINT DF_AuditLogs_CreatedDate DEFAULT (GETUTCDATE())
    );

    CREATE INDEX IX_AuditLogs_CreatedDate
        ON dbo.AuditLogs(CreatedDate DESC);

    CREATE INDEX IX_AuditLogs_Username
        ON dbo.AuditLogs(Username);
END
GO

IF NOT EXISTS (SELECT 1 FROM dbo.ProductList)
BEGIN
    INSERT INTO dbo.ProductList
    (
        ProductCode,
        ProductName,
        Price,
        Quantity,
        IsActive,
        CreatedBy
    )
    VALUES
        (N'SP001', N'Bàn phím cơ', 850000, 12, 1, NULL),
        (N'SP002', N'Chuột không dây', 350000, 25, 1, NULL),
        (N'SP003', N'Tai nghe gaming', 1200000, 8, 1, NULL);
END
GO
