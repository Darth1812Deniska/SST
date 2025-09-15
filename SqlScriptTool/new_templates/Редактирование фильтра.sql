/*!
Этот комментарий не входит в общий скрипт.
Список и описание параметров шаблона:
<FilterName>   - Название фильтра, 
<FilterUID>    - Уникальное наименование фильтра, 
<ScriptAuthor> - Автор скрипта, 
<TableName>    - название таблицы-основания, для которого создается фильтр
!*/

DO
$$
    DECLARE
        tmpFilterName   text     = '<FilterName>';
        tmpTableName    text     = '<TableName>';
        tmpUID          uuid     = '<FilterUID>'::uuid;
        tmpScriptName   text     = format('Редактирование фильтра %s', tmpFilterName);
        tmpScriptAuthor text= '<ScriptAuthor>';
        tmpScriptDate   timestamp= current_date;
        tmpScriptDescr  text     = format('Автор: %s; Дата: %s; %s', tmpScriptAuthor, tmpScriptDate, tmpScriptName);
        tmpErrorMsg     text;
        tmpErrorText    text;
        tmpErrorContext text;
        tmpFilterCond   text;
        tmpTableID      INT;
        tmpFilterID     INT;
        tmpObjectID     INT;
        tmpFilterCond1  text;
    begin
        tmpFilterCond1 := '1=1';
        tmpFilterCond := '<FILTER_COND><SqlText>' || tmpFilterCond1 || '</SqlText></FILTER_COND>';

        --Определяем идентификатор таблицы
        SELECT "ID" INTO tmpTableID FROM DBO.LDTABLE WHERE lower("Name") = lower(tmpTableName);
        IF tmpTableID IS NULL THEN
            tmpErrorMsg :=
                    'Таблица (представление) с именем "' || COALESCE(tmpTableName, '') || '" не найдена в LDTable';
            raise exception '%',tmpErrorMsg;
        END if;

        IF EXISTS(SELECT NULL FROM dbo.ldobject o WHERE o."UID" = tmpUID) THEN
            SELECT flt."ID"
            INTO tmpFilterID
            FROM dbo.LDFILTER flt
                     INNER JOIN dbo.LDOBJECT o ON flt."ObjectID" = o."ID"
            WHERE o."UID" = tmpUID;
        END IF;
        IF (COALESCE(tmpFilterID, 0) = 0 AND
            EXISTS(SELECT NULL FROM dbo.LDOBJECT o WHERE o."UID" = tmpUID AND o."ObjectTypeID" = 81)) THEN
            DELETE FROM dbo.LDOBJECT WHERE "UID" = tmpUID;
        END IF;
        IF COALESCE(tmpFilterID, 0) = 0 AND EXISTS(SELECT NULL FROM dbo.ldobject o WHERE o."UID" = tmpUID) THEN
            tmpErrorMsg = 'Уже существует объект UID которого равен "' || tmpUID::text || '"';
            raise exception '%',tmpErrorMsg;
        END IF;
        IF COALESCE(tmpFilterID, 0) = 0 THEN
            SELECT max("ID") + 1 INTO tmpFilterID FROM dbo.LDFILTER;
        END IF;
        IF NOT EXISTS(SELECT NULL FROM dbo.LDOBJECT o WHERE o."UID" = tmpUID) THEN
            --Создаем новый объект типа фильтр
            tmpObjectID := DBO.LDF_NEWOBJECT(NULL, 81, 1);
            --Добавляем запись в таблицу фильтров
            INSERT INTO DBO.LDFILTER ("ID", "ConditionText", "LD3ConditionText", "DisplayLabel", "IsSQL", "TableID",
                                      "ObjectID")
            VALUES (tmpFilterID, tmpFilterCond, tmpFilterCond, tmpFilterName, 1, tmpTableID, tmpObjectID);
            UPDATE dbo.LDOBJECT SET "UID" = tmpUID WHERE "ID" = tmpObjectID;
        ELSE
            --Обновляем значение фильтра
            UPDATE DBO.LDFILTER
            SET "ConditionText"    = tmpFilterCond
              , "LD3ConditionText" = tmpFilterCond
              , "IsSQL"            = 1
              , "TableID"          = tmpTableID
              , "DisplayLabel"     = tmpFilterName
            WHERE "ID" = tmpFilterID;
        END IF;
        INSERT INTO dbo.LD3CHANGELOG ("ObjectID", "Modified", "ObjectTypeID")
        SELECT o."ID", current_date, o."ObjectTypeID"
        FROM dbo.LDOBJECT o
        WHERE o."UID" = tmpUID;

        insert into DBO.LDUsedScripts (name, description, userlogin, ExecDate)
        select tmpScriptName, tmpScriptDescr, current_user, current_timestamp;
    exception
        when others then get stacked diagnostics tmpErrorMsg = message_text,
            tmpErrorText = pg_exception_detail,
            tmpErrorContext = pg_exception_context;
        tmpErrorMsg := 'При выполнении задачи "' || tmpScriptName || '" произошла ошибка: ' || tmpErrorMsg;
        insert into DBO.LDUsedScripts (name, description, userlogin, execdate)
        select tmpScriptName, tmpScriptDescr, current_user, current_timestamp;
        raise notice '%детально: %контекст: %',tmpErrorMsg || ',' || chr(10),tmpErrorText || ',' || chr(10),tmpErrorContext;
    end;
$$;