/*!  
Этот комментарий не входит в общий скрипт.  
Список и описание параметров шаблона:  
<TableName>          - Название таблицы, для которого регистрируется поле  
<ColumnName>         - Название поля, которое регистрируется  
<ColumnDisplayLabel> - Отображаемое наименование поля,
<CurrentDate>        - текущая дата,  
<DataTypeID>         - идентификатор типа данных поля,  
<FieldLength>        - длина размера поля: int-4, дата или число с точкой-8, строка-количество символов  
<GUIField>           - отображается ли поле в гриде ('+'/'-'),  
<InfoField>          - То же значение, что и GUIField,  
<NotNull>            - Является ли обязательным ('+'/'-'),  
<PKey>               - Является ли первичным ключом ('+'/'-'),  
<RefObjectTypeName>  - Название типа объекта, на которе ссылается поле,  
<ScriptAuthor>       - Автор скрипта,  
  
!*/  
  
do  
$$  
    declare  
        tmpTableName text = '<TableName>';  
        tmpColumnName         text= '<ColumnName>';  
        tmpRefObjectTypeName  text= '<RefObjectTypeName>';  
        tmpDataTypeID         int= <DataTypeID>;  
        tmpFieldLength        int= <FieldLength>;  
        tmpPKey               char(1)= '<PKey>';  
        tmpDisplayLabel       text= '<ColumnDisplayLabel>';  
        tmpGUIField          char(1)= '<GUIField>';  
        tmpInfoField         char(1)= '<InfoField>';  
        tmpNotNull              char(1)= '<NotNull>';  
        tmpResField             char(1)= '-';  
        tmpTableID            integer ;  
        tmpLDUsedScript_Name  text ;  
        tmpLDUsedScript_Descr text ;  
        tmpScriptName         text ;  
        tmpErrorMsg           text ;  
        tmpColumnID           int    = NULL;  
        tmpRefObjectTypeID    int= NULL;  
        tmpScriptDate         timestamp= current_date;  
        tmpScriptAuthor       text= '<ScriptAuthor>';  
  
    begin  
        tmpScriptName := format('Регистрация поля "%s"', tmpColumnName);  
        -- Определяем Таблицу для которой регистрируется поле  
        select "ID" into tmpTableID from DBO.LDTABLE where lower("Name") = lower(tmpTableName);  
        if tmpTableID is null  
        then            tmpErrorMsg :=  
                    'При выполнении задачи "' || tmpScriptName || '" произошла ошибка: ' || 'Для колонки "' ||  
                    tmpColumnName || '" таблица "' || tmpTableName ||  
                    '" не найдена в таблице LDTABLE';  
            tmpLDUsedScript_Name :=  
                    format('"Регистрация поля "%s" в LDCOLUMN" от "%s" автор "%s"', tmpColumnName, tmpScriptDate,  
                           tmpScriptAuthor);  
            tmpLDUsedScript_Descr := tmpErrorMsg;  
  
            insert into DBO.LDUsedScripts (name, description, userlogin, execdate)  
            select tmpLDUsedScript_Name, tmpLDUsedScript_Descr, current_user, current_timestamp;  
            raise exception '%',tmpErrorMsg;  
        end if;  
        --Определяем идентификатор типа связанного объекта  
        if coalesce(tmpRefObjectTypeName, '') != ''  
        then  
            select "ID" into tmpRefObjectTypeID from DBO.LDOBJECTTYPE where "Name" = tmpRefObjectTypeName;  
            if tmpRefObjectTypeID is null  
            then                tmpErrorMsg :=  
                        'При выполнении задачи "' || tmpScriptName || '" произошла ошибка: ' || 'Для колонки "' ||  
                        tmpColumnName || '" тип связанного объекта "' || tmpRefObjectTypeName ||  
                        '" не найден в таблице LDOBJECTTYPE';  
                tmpLDUsedScript_Name :=  
                        format('"Регистрация поля "%s" в LDCOLUMN" от "%s" автор "%s"', tmpColumnName, tmpScriptDate,  
                               tmpScriptAuthor);  
                tmpLDUsedScript_Descr := tmpErrorMsg;  
  
                insert into DBO.LDUsedScripts (name, description, userlogin, execdate)  
                select tmpLDUsedScript_Name, tmpLDUsedScript_Descr, current_user, current_timestamp;  
                raise exception '%',tmpErrorMsg;  
            end if;  
        end if;  
  
        select "ID" into tmpColumnID from DBO.LDCOLUMN where "TableID" = tmpTableID and "Name" = tmpColumnName;  
        if tmpColumnID is null  
        then            select max("ID") + 1 into tmpColumnID from DBO.LDCOLUMN;  
            insert into DBO.LDCOLUMN  
            ( "ID"  
            , "TableID"  
            , "DataTypeID"  
            , "ViewColumnID"  
            , "Name"  
            , "Length"  
            , "PKey"  
            , "Alignment"  
            , "DisplayLabel"  
            , "GUIField"  
            , "InfoField"  
            , "NotNull"  
            , "ResField"  
            , "SortFields"  
            , "ControlTypeID"  
            , "RefObjectTypeID"  
            , "Deleted"  
            , "FullDisplayLabel")  
            select tmpColumnID,        -- [ID]                идентификатор колонки  
                   tmpTableID,         -- [TableID]           идентификатор таблицы/представления из LDTable  
                   tmpDataTypeID,      -- [DataTypeID]        идентификатор типа данных из LDDataType  
                   NULL,               -- [ViewColumnID]      не используется  
                   tmpColumnName,      -- [Name]              название столбца  
                   tmpFieldLength,     -- [Length]            длина типа данных колонки (актуально для строк) (число)  
                   tmpPKey,            -- [PKey]              признак первичного ключа (+,-)  
                   0,                  -- [Alignment]         выравнивание (0 - по пр. краю, 1 по лев. краю, 2 - по центру, -1 - не использ. )  
                   tmpDisplayLabel,    -- [DisplayLabel]      отображаемое наименование (строка)  
                   tmpGUIField,        -- [GUIField]          признак показывает, может ли столбец учав. в запросах (+,-)  
                   tmpInfoField,       -- [InfoField]         признак отображения столбца пользователю в гридах, комбобоксах и т.п. (+,-)  
                   tmpNotNull,         -- [NotNull]           признак не пустого поля (+,-)  
                   tmpResField,        -- [ResField]          иконка (+,-)  
                   NULL,               -- [SortFields]        перечень полей сортировки (строка)  
                   NULL,               -- [ControlTypeID]     не используется  
                   tmpRefObjectTypeID, -- [RefObjectTypeID]   идентификатор типа связанного объекта  
                   '-',                -- [Deleted]           пометка на удаление (+,-)  
                   NULL; -- [FullDisplayLabel]  полное отображаемое наименование (строка)  
        else  
            update DBO.LDCOLUMN  
            set "DataTypeID"      = tmpDataTypeID,  
                "Length"          = tmpFieldLength,  
                "PKey"            = tmpPKey,  
                "DisplayLabel"    = tmpDisplayLabel,  
                "GUIField"        = tmpGUIField,  
                "InfoField"       = tmpInfoField,  
                "NotNull"         = tmpNotNull,  
                "ResField"        = tmpResField,  
                "RefObjectTypeID" = tmpRefObjectTypeID  
            where "ID" = tmpColumnID;  
        end if;  
  
    end;  
$$;