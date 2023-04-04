--
-- @depende-de: scriptpack.schema
--
CREATE OR REPLACE FUNCTION scriptpack.SET_ARG(p_argument VARCHAR(128), p_value TEXT)
RETURNS VOID
AS $$
DECLARE
  v_session_id SMALLINT;
  v_login_time TIMESTAMP;
BEGIN
  SELECT pg_backend_pid() INTO v_session_id;
  SELECT backend_start INTO v_login_time FROM pg_stat_activity WHERE pid = v_session_id;
  
  -- Removendo argumentos de sessões expiradas
  DELETE FROM scriptpack.arguments
  WHERE NOT EXISTS (
      SELECT * FROM pg_stat_activity
      WHERE pg_stat_activity.pid = scriptpack.arguments.session_id
        AND pg_stat_activity.backend_start = scriptpack.arguments.login_time
  );
  
  -- Removendo valor corrente se houver
  DELETE FROM scriptpack.arguments
  WHERE session_id = v_session_id
    AND login_time = v_login_time
    AND name = p_argument;
  
  -- Inserindo o novo valor
  INSERT INTO scriptpack.arguments (session_id, login_time, name, value)
  VALUES (v_session_id, v_login_time, p_argument, p_value);
END;
$$ LANGUAGE plpgsql;
