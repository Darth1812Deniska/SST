/*!  
Этот комментарий не входит в общий скрипт.  
Список и описание параметров шаблона:  
<TableName>    - Название таблицы, для которого добавляется внешний ключ  
<ColumnName>   - Название поля, для которого добавляется ограничение <RefTableName> - Таблица, на которое ссылается значение из поля   
!*/  
do  
$$  
    begin  
        ALTER TABLE dbo.<TableName> DROP CONSTRAINT IF EXISTS FK_<TableName>_<ColumnName>;  
        ALTER TABLE IF EXISTS dbo.<TableName>  
        ADD CONSTRAINT FK_<TableName>_<ColumnName> FOREIGN KEY ("<ColumnName>")  
        REFERENCES dbo.<RefTableName>("ID");  
    end;  
$$;