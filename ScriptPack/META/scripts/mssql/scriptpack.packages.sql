IF OBJECT_ID('scriptpack.packages') IS NULL BEGIN
  CREATE TABLE scriptpack.packages (
      name NVARCHAR(128) NOT NULL PRIMARY KEY
    , title NVARCHAR(128) NOT NULL
    , description NVARCHAR(4000) NOT NULL
    , version VARCHAR(50) NOT NULL
    , install_date DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP
  )
END
