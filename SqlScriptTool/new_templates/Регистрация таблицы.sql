/*!  
Этот комментарий не входит в общий скрипт.  
Список и описание параметров шаблона:  
<CardTable>                 - Относится ли таблица к карточкам или справочникам ('+'/'-'),  
<GUIView>                   - Отображение значений таблицы в клиенте, в гриде. ('+'/'-'),  
<ScriptAuthor>              - Автор скрипта для логгирования,  
<TableDisplayLabel>         - Отображаемое имя таблицы,  
<TableName>                 - Физическое наименование таблицы,  
<TableType>                 - тип таблицы: таблица или представление ('T'/'V')  
!*/  
do  
$$  
    declare  
        tmpTableName          text= '<TableName>';  
        tmpDisplayLabel       text= '<TableDisplayLabel>';  
        tmpCardTable          char(1)= '<CardTable>';  
        tmpGUIView            char(1)= '<GUIView>';  
        tmpTableType          char(1)= '<TableType>';  
        tmpTableID            int;  
        tmpLDUsedScript_Name  text;  
        tmpLDUsedScript_Descr text;  
        tmpScriptName         text = format('Регистрация таблицы "%s" в LDTABLE', tmpTableName);  
        tmpScriptDate         timestamp= current_date;  
        tmpScriptAuthor       text= '<ScriptAuthor>';  
    begin  
        --Регистрация таблицы <TableName> в LDTABLE  
        --РЕГИСТРАЦИЯ ТАБЛИЦЫ        --Регистрируем таблицу в LDTABLE        select "ID" into tmpTableID from DBO.LDTABLE where lower("Name") = lower(tmpTableName);  
        if tmpTableID is null then  
            select max("ID") + 1 into tmpTableID from DBO.LDTABLE;  
            insert into DBO.LDTABLE  
            ( "ID"  
            , "Name"  
            , "CardTable"  
            , "GUIView"  
            , "Type"  
            , "DisplayLabel"  
            , "Deleted")  
            values ( tmpTableID -- [ID] идентификатор таблицы в LDTABLE  
                   , tmpTableName -- [Name] наименование таблицы  
                   , tmpCardTable -- [CardTable] признак, что табл. содержит поля ГРК (+,-)  
                   , tmpGUIView -- [GUIView] признак возможности отображения (+,-)  
                   , tmpTableType -- [Type] Тип [V/T] - [представление/Таблица]  
                   , tmpDisplayLabel -- [DisplayLabel] описание таблицы  
                   , '-' -- [Deleted] признак пометки на удаление (+,-)  
                   );  
        else  
            update DBO.LDTABLE  
            set "CardTable"    = tmpCardTable,  
                "GUIView"      = tmpGUIView,  
                "DisplayLabel" = tmpDisplayLabel  
            where "ID" = tmpTableID;  
        end if;  
  
        tmpLDUsedScript_Name := format('"Регистрация таблицы "%s" в LDTABLE" от "%s" автор "%s"', tmpTableName, tmpScriptDate, tmpScriptAuthor) ;  
        tmpLDUsedScript_Descr := format('Регистрация таблицы "%s" в LDTABLE', tmpTableName) ;  
  
        insert into DBO.LDUsedScripts (name, description, userlogin, ExecDate)  
        select tmpLDUsedScript_Name, tmpLDUsedScript_Descr, current_user, current_timestamp;  
    end;  
$$;