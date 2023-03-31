--
-- @depende-de: scriptpack.schema
--
IF OBJECT_ID('scriptpack.DATE_ARG') IS NULL BEGIN
  EXEC ('
    CREATE FUNCTION scriptpack.DATE_ARG(@argument VARCHAR(128))
    RETURNS DATETIME
    AS
    BEGIN
      DECLARE @result DATETIME

      SELECT @result = CAST(value AS DATETIME)
      FROM scriptpack.arguments
      JOIN sys.dm_exec_sessions
        ON sys.dm_exec_sessions.session_id = scriptpack.arguments.session_id
        AND sys.dm_exec_sessions.login_time = scriptpack.arguments.login_time
      WHERE name = @argument
        AND sys.dm_exec_sessions.session_id = @@SPID

      RETURN @result
    END
  ')
END
GO
