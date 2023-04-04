--
-- @depende-de: scriptpack.schema
--
CREATE OR REPLACE FUNCTION scriptpack.STR_ARG(p_argument VARCHAR(128))
RETURNS TEXT
LANGUAGE SQL
AS $$
  SELECT value::TEXT FROM scriptpack.arguments WHERE name = p_argument
$$;
