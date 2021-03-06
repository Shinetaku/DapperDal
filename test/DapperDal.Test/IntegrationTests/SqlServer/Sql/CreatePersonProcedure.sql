﻿IF EXISTS ( SELECT  *
            FROM    sys.objects
            WHERE   object_id = OBJECT_ID(N'P_GetPersonsByCarId')
                    AND type IN (N'P', N'PC') )
BEGIN
    DROP PROCEDURE dbo.P_GetPersonsByCarId
END

EXEC('
CREATE PROCEDURE dbo.P_GetPersonsByCarId (@CarId INT)
AS
    BEGIN
        SELECT  *
        FROM    Person
        WHERE   CarId = @CarId;
    END
')

IF EXISTS ( SELECT  *
            FROM    sys.objects
            WHERE   object_id = OBJECT_ID(N'P_GetPersonModelsByCarId')
                    AND type IN (N'P', N'PC') )
BEGIN
    DROP PROCEDURE dbo.P_GetPersonModelsByCarId
END

EXEC('
CREATE PROCEDURE dbo.P_GetPersonModelsByCarId (@CarId INT)
AS
    BEGIN
        SELECT  PersonName AS [Name]
               ,CarId
        FROM    Person
        WHERE   CarId = @CarId;
    END;
')

IF EXISTS ( SELECT  *
            FROM    sys.objects
            WHERE   object_id = OBJECT_ID(N'P_GetPersonMultipleModelsByCarId')
                    AND type IN (N'P', N'PC') )
BEGIN
    DROP PROCEDURE dbo.P_GetPersonMultipleModelsByCarId
END

EXEC('
CREATE PROCEDURE dbo.P_GetPersonMultipleModelsByCarId (@CarId INT)
AS
    BEGIN
        SELECT  *
        FROM    Person
        WHERE   CarId = @CarId;

        SELECT  PersonName AS [Name]
               ,CarId
        FROM    Person
        WHERE   CarId = @CarId;
    END;
')

IF EXISTS ( SELECT  *
            FROM    sys.objects
            WHERE   object_id = OBJECT_ID(N'P_GetPersonModelsByCarId_OutputCount')
                    AND type IN (N'P', N'PC') )
BEGIN
    DROP PROCEDURE dbo.P_GetPersonModelsByCarId_OutputCount
END

EXEC('
CREATE PROCEDURE dbo.P_GetPersonModelsByCarId_OutputCount (@CarId INT, @TotalCount INT Output)
AS
    BEGIN
        SELECT @TotalCount = COUNT(1)
        FROM    Person
        WHERE   CarId = @CarId;

        SELECT  PersonName AS [Name]
               ,CarId
        FROM    Person
        WHERE   CarId = @CarId;
    END;
')

IF EXISTS ( SELECT  *
            FROM    sys.objects
            WHERE   object_id = OBJECT_ID(N'P_SetPersonsByCarId')
                    AND type IN (N'P', N'PC') )
BEGIN
    DROP PROCEDURE dbo.P_SetPersonsByCarId
END

EXEC('
CREATE PROCEDURE dbo.P_SetPersonsByCarId (@CarId INT)
AS
    BEGIN
        update Person set IsActive = 1 where CarId = @CarId;
    END;
')


IF EXISTS ( SELECT  *
            FROM    sys.objects
            WHERE   object_id = OBJECT_ID(N'P_SetPersonsByCarId_OutputCount')
                    AND type IN (N'P', N'PC') )
BEGIN
    DROP PROCEDURE dbo.P_SetPersonsByCarId_OutputCount
END

EXEC('
CREATE PROCEDURE dbo.P_SetPersonsByCarId_OutputCount (@CarId INT, @TotalCount INT Output)
AS
    BEGIN
        SELECT @TotalCount = COUNT(1)
        FROM    Person
        WHERE   CarId = @CarId;

        update Person set IsActive = 1 where CarId = @CarId;
    END;
')
