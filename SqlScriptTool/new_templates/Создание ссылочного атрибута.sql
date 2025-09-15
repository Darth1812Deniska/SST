/*!  
Этот комментарий не входит в общий скрипт.  
Список и описание параметров шаблона:  
<AttributeName>  - Имя нового или изменяемого атрибута,  
<DisplayLabel>   - Отображаемое имя в гриде,  
<FromColumnName> - Из какого поля используется ссылка,  
<FromTableName>  - В какой таблице располгается поле содержащее ссылку,  
<GUIField>       - Отображается ли поле в гриде ('+'/'-'),  
<ScriptAuthor>   - автор скрипта  
!*/  
  
do  
$$  
    declare  
        tmpLDUsedScript_Descr text ;  
        tmpLDUsedScript_Name  text ;  
        tmpAttributeID        int ;  
        tmpTCID               int ;  
        tmpTTID               int ;  
        tmpFCID               int ;  
        tmpErrorMsg           text ;  
        tmpFTID               int ;  
        tmpScriptName         text ;  
        tmpAttributeName      text= '<AttributeName>';  
        tmpDisplayLabel       text= '<DisplayLabel>';  
        tmpDeleted            text= '-';  
        tmpGUIField           char(1)= '<GUIField>';  
        tmpFTName             text= '<FromTableName>';  
        tmpFCName             text= '<FromColumnName>';  
        tmpTTName             text= 'LDVOCABULARY';  
        tmpTCName             text= 'Name';  
        tmpLength             int= 512;  
        tmpErrorContext       text ;  
        tmpErrorText          text ;  
        tmpScriptDate         timestamp= current_date;  
        tmpScriptAuthor       text= '<ScriptAuthor>';  
    begin  
        tmpScriptName := format('Редактирование атрибута %s:%s', tmpFTName, tmpAttributeName);  
        --Определяем идентификатор таблицы  
        SELECT "ID" into tmpFTID FROM DBO.LDTABLE WHERE lower("Name") = lower(tmpFTName) AND "Type" = 'T';  
        IF tmpFTID IS NULL  
        THEN            tmpErrorMsg := 'Таблица с именем "' || tmpFTName || '" не найдена в LDTABLE';  
            raise exception '%',tmpErrorMsg;  
        END IF;  
        --Определяем идентификатор колонки  
        SELECT "ID" into tmpFCID FROM DBO.LDCOLUMN WHERE "Name" = tmpFCName AND "TableID" = tmpFTID;  
        IF tmpFCID IS NULL  
        then            tmpErrorMsg := 'Колонка с именем "' || tmpFCName || '" таблицы "' || tmpFTName || '" не найдена в LDColumn';  
            raise exception '%',tmpErrorMsg;  
        END if;  
        --Определяем идентификатор присоединенной таблицы  
        SELECT "ID" into tmpTTID FROM DBO.LDTABLE WHERE lower("Name") = lower(tmpTTName);  
        IF tmpTTID IS NULL  
        then            tmpErrorMsg := 'Таблица с именем "' || tmpTTName || '" не найдена в LDTABLE';  
            raise exception '%',tmpErrorMsg;  
        END if;  
        --Определяем идентификатор присоединенной колонки  
        SELECT "ID" into tmpTCID FROM DBO.LDCOLUMN WHERE "Name" = tmpTCName AND "TableID" = tmpTTID;  
        IF tmpTCID IS NULL  
        then            tmpErrorMsg := 'Колонка с именем "' || tmpTCName || '" таблицы "' || tmpTTName || '" не найдена в LDColumn';  
            raise exception '%',tmpErrorMsg;  
        END if;  
        --Определяем идентификатор атрибута  
        SELECT "AttributeID"  
        into tmpAttributeID  
        FROM DBO.LD30Attribute  
        WHERE "AttributeName" = tmpAttributeName  
          AND "TableID" = tmpFTID;  
        IF tmpAttributeID IS NULL  
        then            SELECT MAX("AttributeID") + 1 into tmpAttributeID FROM DBO.LD30Attribute WHERE "AttributeID" > 20000;  
            tmpAttributeID := coalesce(tmpAttributeID, 20001);  
  
            INSERT INTO DBO.LD30Attribute ("AttributeID", "AttributeName", "AttributeType", "Deleted", "DisplayLabel",  
                                           "GUIField", "Length", "TableID")  
            VALUES (tmpAttributeID, tmpAttributeName, 'L', tmpDeleted, tmpDisplayLabel, tmpGUIField, tmpLength,  
                    tmpFTID);  
  
            INSERT INTO DBO.LD30LinkedAttribute ("AttributeID", "ColumnLinkID", "ToColumnID")  
            VALUES (tmpAttributeID, tmpFCID, tmpTCID);  
        ELSE  
            UPDATE DBO.LD30Attribute  
            SET "Deleted"      = tmpDeleted  
              , "DisplayLabel" = tmpDisplayLabel  
              , "Length"       = tmpLength  
              , "GUIField"     = tmpGUIField  
            WHERE "AttributeID" = tmpAttributeID;  
            UPDATE DBO.LD30LinkedAttribute  
            SET "ColumnLinkID" = tmpFCID  
              , "ToColumnID"   = tmpTCID  
            WHERE "AttributeID" = tmpAttributeID;  
        END if;  
  
        tmpLDUsedScript_Name :=  
                format('"Редактирование атрибута %s:%s" от "%s" автор "%s"', tmpFTName, tmpAttributeName, tmpScriptDate,  
                       tmpScriptAuthor);  
        tmpLDUsedScript_Descr := tmpScriptName;  
        insert into DBO.LDUsedScripts (name, description, userlogin, ExecDate)  
        select tmpLDUsedScript_Name, tmpLDUsedScript_Descr, current_user, current_timestamp;  
  
    exception  
        when others then            get stacked diagnostics tmpErrorMsg = message_text,  
                tmpErrorText = pg_exception_detail,  
                tmpErrorContext = pg_exception_context;  
            tmpErrorMsg := 'При выполнении задачи "' || tmpScriptName || '" произошла ошибка: ' || tmpErrorMsg;  
            tmpLDUsedScript_Name :=  
                    format('"Редактирование атрибута %s:%s" от "%s" автор "%s"', tmpFTName, tmpAttributeName,  
                           tmpScriptDate, tmpScriptAuthor);  
            tmpLDUsedScript_Descr := tmpErrorMsg;  
            insert into DBO.LDUsedScripts (name, description, userlogin, ExecDate)  
            SELECT tmpLDUsedScript_Name, tmpLDUsedScript_Descr, current_user, current_timestamp;  
            raise exception '%',tmpErrorMsg;  
    end;  
$$;