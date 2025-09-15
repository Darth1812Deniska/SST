/*!  
Этот комментарий не входит в общий скрипт.  
Список и описание параметров шаблона:  
<TableName>    - Название создаваемой таблицы,  
<RefTableName> - таблица родитель  
  
!*/  
  
do $$  
begin  
   CREATE TABLE IF NOT EXISTS dbo.<TableName> (  
    "ID" BIGINT NOT NULL,  
    CONSTRAINT PK_<TableName>_ID PRIMARY KEY ("ID"),  
    CONSTRAINT FK_<TableName>_ID FOREIGN KEY ("ID")  
    REFERENCES dbo.<RefTableName>("ID") ON DELETE CASCADE  
);       
end;  
$$;