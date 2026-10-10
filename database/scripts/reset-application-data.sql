/*
    InvestManage development-data reset

    SQL Server does not allow TRUNCATE TABLE when foreign-key relationships
    reference a table, even when those constraints are disabled. This script
    uses ordered DELETE statements inside one transaction to provide the same
    empty-data result without dropping or weakening any constraints.

    EF migration history is intentionally preserved so the existing schema is
    not treated as an unmigrated database afterward.
*/

USE [InvestManage];
GO

SET NOCOUNT ON;
SET XACT_ABORT ON;

DECLARE @ConfirmReset bit = 0;
DECLARE @DeleteCurrencyReferenceData bit = 0;

-- Change @ConfirmReset to 1 only when you intend to remove application data.
IF @ConfirmReset <> 1
BEGIN
    THROW 50001, 'Reset cancelled. Set @ConfirmReset to 1 after verifying the target database.', 1;
END;

IF DB_NAME() <> N'InvestManage'
BEGIN
    THROW 50002, 'Reset cancelled because the current database is not InvestManage.', 1;
END;

BEGIN TRY
    BEGIN TRANSACTION;

    -- Level 1: financial history depends on assignments, investments, and currencies.
    DELETE FROM dbo.Transactions;
    DELETE FROM dbo.PriceHistory;

    -- Level 2: assignments depend on accounts and investments.
    DELETE FROM dbo.AccountInvestments;

    -- Level 3: accounts depend on users and currencies; investments depend on currencies.
    DELETE FROM dbo.InvestmentAccounts;
    DELETE FROM dbo.InvestmentItems;

    -- Level 4: users have no remaining application dependants.
    DELETE FROM dbo.Users;

    -- CAD and USD are required by account, investment, and transaction entry.
    -- Set this option to 1 only when a completely empty domain database is required.
    IF @DeleteCurrencyReferenceData = 1
    BEGIN
        DELETE FROM dbo.Currencies;
    END;

    COMMIT TRANSACTION;
END TRY
BEGIN CATCH
    IF XACT_STATE() <> 0
    BEGIN
        ROLLBACK TRANSACTION;
    END;

    THROW;
END CATCH;

SELECT
    (SELECT COUNT_BIG(*) FROM dbo.Users) AS UsersRemaining,
    (SELECT COUNT_BIG(*) FROM dbo.InvestmentAccounts) AS AccountsRemaining,
    (SELECT COUNT_BIG(*) FROM dbo.InvestmentItems) AS InvestmentsRemaining,
    (SELECT COUNT_BIG(*) FROM dbo.AccountInvestments) AS AssignmentsRemaining,
    (SELECT COUNT_BIG(*) FROM dbo.Transactions) AS TransactionsRemaining,
    (SELECT COUNT_BIG(*) FROM dbo.PriceHistory) AS PricesRemaining,
    (SELECT COUNT_BIG(*) FROM dbo.Currencies) AS CurrenciesRemaining;
GO
