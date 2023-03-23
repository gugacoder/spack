--
-- @depende-de: scriptpack.schema
--
-- DROP TABLE scriptpack.argument
IF OBJECT_ID('scriptpack.argument') IS NULL BEGIN
  -- Tabela usada na parametrização de scripts executados com o ScriptPack.
  CREATE TABLE scriptpack.argument (
      -- Id da sessão de conexão. Equivale ao @@SPID da conexão.
      session_id SMALLINT NOT NULL
      -- Data de início da conexão.
      -- Usado para diferenciar conexões com mesmo @@SPID.
    , login_time DATETIME NOT NULL
      -- Nome do argumento.
    , name VARCHAR(128) NOT NULL
      -- Valor do argumento.
    , value NVARCHAR(MAX)
    , CONSTRAINT PK_argument PRIMARY KEY (session_id, login_time, name)
  )
END
GO
