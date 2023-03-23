--
-- @depende-de: scriptpack.schema
--
IF OBJECT_ID('scriptpack.DECIMAL_ARG') IS NULL BEGIN
  EXEC ('
    CREATE FUNCTION scriptpack.DECIMAL_ARG(@argument VARCHAR(128))
    RETURNS DECIMAL
    AS
    BEGIN
      DECLARE @result DECIMAL

      SELECT @result = CAST(value AS DECIMAL)
      FROM scriptpack.argument
      JOIN sys.dm_exec_sessions
        ON sys.dm_exec_sessions.session_id = scriptpack.argument.session_id
        AND sys.dm_exec_sessions.login_time = scriptpack.argument.login_time
      WHERE name = @argument
        AND sys.dm_exec_sessions.session_id = @@SPID

      RETURN @result
    END
  ')
END
GO
