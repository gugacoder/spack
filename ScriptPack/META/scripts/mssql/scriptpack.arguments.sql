--
-- @depende-de: scriptpack.schema
--
IF OBJECT_ID('scriptpack.arguments') IS NULL BEGIN
  -- Tabela usada na parametrização de scripts executados com o ScriptPack.
  CREATE TABLE scriptpack.arguments (
      -- Id da sessão de conexão. Equivale ao @@SPID da conexão.
      session_id SMALLINT NOT NULL
      -- Data de início da conexão.
      -- Usado para diferenciar conexões com mesmo @@SPID.
    , login_time DATETIME NOT NULL
      -- Nome do argumento.
    , name VARCHAR(128) NOT NULL
      -- Valor do argumento.
    , value NVARCHAR(MAX)
    , CONSTRAINT PK_arguments PRIMARY KEY (session_id, login_time, name)
  )
END
GO
