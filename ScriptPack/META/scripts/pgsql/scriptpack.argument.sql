--
-- @depende-de: scriptpack.schema
--
-- Tabela usada na parametriza��o de scripts executados com o ScriptPack.
CREATE TABLE IF NOT EXISTS scriptpack.argument (
    -- Id da sess�o de conex�o. Equivale ao @@SPID da conex�o.
    session_id SMALLINT NOT NULL
    -- Data de in�cio da conex�o.
    -- Usado para diferenciar conex�es com mesmo @@SPID.
  , login_time TIMESTAMP NOT NULL
    -- Nome do argumento.
  , name VARCHAR(128) NOT NULL
    -- Valor do argumento.
  , value TEXT
  , CONSTRAINT PK_argument PRIMARY KEY (session_id, login_time, name)
);
