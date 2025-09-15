/*!  
Этот комментарий не входит в общий скрипт.  
Список и описание параметров шаблона:  
<TableName>    - Название таблицы, для которой создается поле,  
<ColumnName>   - Название нового поля, <DataTypeName> - Тип поля  
!*/  
do  
$$  
    begin  
        ALTER TABLE IF EXISTS dbo.<TableName>  
        ADD COLUMN IF NOT EXISTS "<ColumnName>" <DataTypeName>;  
    end;  
$$;