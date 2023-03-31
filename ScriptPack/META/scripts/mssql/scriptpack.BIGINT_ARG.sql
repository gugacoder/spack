--
-- @depende-de: scriptpack.schema
--
IF OBJECT_ID('scriptpack.BIGINT_ARG') IS NULL BEGIN
  EXEC ('
    CREATE FUNCTION scriptpack.BIGINT_ARG(@argument VARCHAR(128))
    RETURNS BIGINT
    AS
    BEGIN
      DECLARE @result BIGINT

      SELECT @result = CAST(value AS BIGINT)
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
