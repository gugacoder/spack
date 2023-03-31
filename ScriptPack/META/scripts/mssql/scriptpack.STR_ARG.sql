--
-- @depende-de: scriptpack.schema
--
IF OBJECT_ID('scriptpack.STR_ARG') IS NULL BEGIN
  EXEC ('
    CREATE FUNCTION scriptpack.STR_ARG(@argument VARCHAR(128))
    RETURNS NVARCHAR(MAX)
    AS
    BEGIN
      DECLARE @result NVARCHAR(MAX)

      SELECT @result = CAST(value AS NVARCHAR(MAX))
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
