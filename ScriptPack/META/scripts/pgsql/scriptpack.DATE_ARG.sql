--
-- @depende-de: scriptpack.schema
--
CREATE OR REPLACE FUNCTION scriptpack.DATE_ARG(p_argument VARCHAR(128))
RETURNS TIMESTAMP
LANGUAGE SQL
AS $$
  SELECT value::TIMESTAMP FROM scriptpack.arguments WHERE name = p_argument
$$;
