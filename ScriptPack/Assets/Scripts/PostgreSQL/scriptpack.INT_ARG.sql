--
-- @depende-de: scriptpack.schema
--
CREATE OR REPLACE FUNCTION scriptpack.INT_ARG(p_argument VARCHAR(128))
RETURNS INT
LANGUAGE SQL
AS $$
  SELECT value::INT FROM scriptpack.arguments WHERE name = p_argument
$$;
