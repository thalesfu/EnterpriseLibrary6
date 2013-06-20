@ECHO OFF

@ECHO CREATING DATABASE
sqlcmd -E -S (localdb)\v11.0 -i CreateDatabase.sql

@ECHO ADDING SCHEMA
sqlcmd -E -S (localdb)\v11.0 -i DataAccessDB.sql -d DataAccessExamples

@ECHO ADDING DATA
sqlcmd -E -S (localdb)\v11.0 -i Data.sql -d DataAccessExamples
