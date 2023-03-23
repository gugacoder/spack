--
-- @depende-de: scriptpack.schema
--
-- DROP TABLE scriptpack.argument
IF OBJECT_ID('scriptpack.argument') IS NULL BEGIN
  -- Tabela usada na parametriza��o de scripts executados com o ScriptPack.
  CREATE TABLE scriptpack.argument (
      -- Id da sess�o de conex�o. Equivale ao @@SPID da conex�o.
      session_id SMALLINT NOT NULL
      -- Data de in�cio da conex�o.
      -- Usado para diferenciar conex�es com mesmo @@SPID.
    , login_time DATETIME NOT NULL
      -- Nome do argumento.
    , name VARCHAR(128) NOT NULL
      -- Valor do argumento.
    , value NVARCHAR(MAX)
    , CONSTRAINT PK_argument PRIMARY KEY (session_id, login_time, name)
  )
END
GO
