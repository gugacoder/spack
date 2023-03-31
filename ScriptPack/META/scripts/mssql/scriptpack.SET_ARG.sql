--
-- @depende-de: scriptpack.schema
--
IF OBJECT_ID('scriptpack.SET_ARG') IS NOT NULL BEGIN
  DROP PROCEDURE scriptpack.SET_ARG
END
GO

CREATE PROCEDURE scriptpack.SET_ARG
    @argument VARCHAR(128)
  , @value SQL_VARIANT
AS
BEGIN
  DECLARE
      @session_id SMALLINT
    , @login_time DATETIME

  SELECT
      @session_id = session_id
    , @login_time = login_time
  FROM sys.dm_exec_sessions
  WHERE session_id = @@SPID

  -- Removendo argumentos de sessÃµes expiradas
  DELETE FROM scriptpack.arguments
  WHERE NOT EXISTS (
    SELECT * FROM sys.dm_exec_sessions
    WHERE session_id = scriptpack.arguments.session_id
      AND login_time = scriptpack.arguments.login_time
  )

  -- Removendo valor corrente se houver
  DELETE FROM scriptpack.arguments
  WHERE session_id = @session_id
    AND login_time = @login_time
    AND name = @argument

  -- Inserindo o novo valor
  INSERT INTO scriptpack.arguments (session_id, login_time, name, value)
  VALUES (
      @session_id
    , @login_time
    , @argument
    , CONVERT(NVARCHAR(MAX), @value, 120)
  )
END
GO
