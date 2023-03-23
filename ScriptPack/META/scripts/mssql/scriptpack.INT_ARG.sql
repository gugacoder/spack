--
-- @depende-de: scriptpack.schema
--
IF OBJECT_ID('scriptpack.INT_ARG') IS NULL BEGIN
  EXEC ('
    CREATE FUNCTION scriptpack.INT_ARG(@argument VARCHAR(128))
    RETURNS INT
    AS
    BEGIN
      DECLARE @result INT

      SELECT @result = CAST(value AS INT)
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
