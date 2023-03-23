--
-- @depende-de: scriptpack.schema
--
CREATE OR REPLACE FUNCTION scriptpack.BIGINT_ARG(p_argument VARCHAR(128))
RETURNS BIGINT
LANGUAGE SQL
AS $$
  SELECT value::BIGINT FROM scriptpack.argument WHERE name = p_argument
$$;
