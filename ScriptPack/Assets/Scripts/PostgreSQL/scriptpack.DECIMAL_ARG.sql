--
-- @depende-de: scriptpack.schema
--
CREATE OR REPLACE FUNCTION scriptpack.DECIMAL_ARG(p_argument VARCHAR(128))
RETURNS DECIMAL
LANGUAGE SQL
AS $$
  SELECT value::DECIMAL FROM scriptpack.arguments WHERE name = p_argument
$$;
