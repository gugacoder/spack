IF OBJECT_ID('scriptpack.bindings') IS NULL BEGIN
  CREATE TABLE scriptpack.bindings (
    kind VARCHAR(400) PRIMARY KEY,
    name VARCHAR(400)
  )
END
