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

-- Các Stored Procedure dưới đây được giữ lại để đối chiếu bài Week 1.
-- Backend Week 2 đã chuyển CRUD sang EF Core Repository.
-- API GetAll dùng IQueryable để filtering/sorting/paging chạy trực tiếp trên SQL Server.

CREATE OR ALTER PROCEDURE dbo.sp_Product_GetAll
    @Keyword NVARCHAR(200) = NULL,
    @IsActive BIT = NULL,
    @PageIndex INT = 1,
    @PageSize INT = 5,
    @TotalRecords INT OUTPUT
AS
BEGIN
    SET NOCOUNT ON;

    IF @PageIndex < 1 SET @PageIndex = 1;
    IF @PageSize < 1 SET @PageSize = 5;
    IF @PageSize > 100 SET @PageSize = 100;

    SET @Keyword = NULLIF(LTRIM(RTRIM(@Keyword)), N'');

    SELECT @TotalRecords = COUNT(1)
    FROM dbo.ProductList
    WHERE IsDeleted = 0
      AND (
            @Keyword IS NULL
            OR ProductCode LIKE N'%' + @Keyword + N'%'
            OR ProductName LIKE N'%' + @Keyword + N'%'
          )
      AND (@IsActive IS NULL OR IsActive = @IsActive);

    SELECT
        Id,
        ProductCode,
        ProductName,
        Price,
        Quantity,
        IsActive,
        CreatedDate,
        ModifiedDate
    FROM dbo.ProductList
    WHERE IsDeleted = 0
      AND (
            @Keyword IS NULL
            OR ProductCode LIKE N'%' + @Keyword + N'%'
            OR ProductName LIKE N'%' + @Keyword + N'%'
          )
      AND (@IsActive IS NULL OR IsActive = @IsActive)
    ORDER BY Id DESC
    OFFSET (@PageIndex - 1) * @PageSize ROWS
    FETCH NEXT @PageSize ROWS ONLY;
END
GO

CREATE OR ALTER PROCEDURE dbo.sp_Product_GetById
    @Id INT
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        Id,
        ProductCode,
        ProductName,
        Price,
        Quantity,
        IsActive,
        CreatedDate,
        ModifiedDate
    FROM dbo.ProductList
    WHERE Id = @Id
      AND IsDeleted = 0;
END
GO

CREATE OR ALTER PROCEDURE dbo.sp_Product_Create
    @ProductCode NVARCHAR(50),
    @ProductName NVARCHAR(200),
    @Price DECIMAL(18,2),
    @Quantity INT,
    @IsActive BIT,
    @CreatedBy INT = NULL,
    @NewId INT OUTPUT
AS
BEGIN
    SET NOCOUNT ON;

    IF EXISTS
    (
        SELECT 1
        FROM dbo.ProductList
        WHERE ProductCode = @ProductCode
          AND IsDeleted = 0
    )
        THROW 50001, N'Mã sản phẩm đã tồn tại.', 1;

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
    (
        @ProductCode,
        @ProductName,
        @Price,
        @Quantity,
        @IsActive,
        @CreatedBy
    );

    SET @NewId = SCOPE_IDENTITY();
END
GO

CREATE OR ALTER PROCEDURE dbo.sp_Product_Update
    @Id INT,
    @ProductCode NVARCHAR(50),
    @ProductName NVARCHAR(200),
    @Price DECIMAL(18,2),
    @Quantity INT,
    @IsActive BIT,
    @Success BIT OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    SET @Success = 0;

    IF EXISTS
    (
        SELECT 1
        FROM dbo.ProductList
        WHERE ProductCode = @ProductCode
          AND Id <> @Id
          AND IsDeleted = 0
    )
        THROW 50001, N'Mã sản phẩm đã tồn tại.', 1;

    UPDATE dbo.ProductList
    SET ProductCode = @ProductCode,
        ProductName = @ProductName,
        Price = @Price,
        Quantity = @Quantity,
        IsActive = @IsActive,
        ModifiedDate = GETUTCDATE()
    WHERE Id = @Id
      AND IsDeleted = 0;

    IF @@ROWCOUNT > 0
        SET @Success = 1;
END
GO

CREATE OR ALTER PROCEDURE dbo.sp_Product_Delete
    @Id INT,
    @Success BIT OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    SET @Success = 0;

    UPDATE dbo.ProductList
    SET IsDeleted = 1,
        ModifiedDate = GETUTCDATE()
    WHERE Id = @Id
      AND IsDeleted = 0;

    IF @@ROWCOUNT > 0
        SET @Success = 1;
END
GO
