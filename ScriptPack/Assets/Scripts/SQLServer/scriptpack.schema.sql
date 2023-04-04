IF NOT EXISTS (SELECT * FROM sys.schemas WHERE NAME = 'scriptpack') BEGIN
  EXEC ('CREATE SCHEMA scriptpack')
END
