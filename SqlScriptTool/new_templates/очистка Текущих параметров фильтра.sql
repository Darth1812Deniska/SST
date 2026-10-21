/*!
Этот комментарий не входит в общий скрипт.
Список и описание параметров шаблона:
<FilterUID>    - Уникальный идентификатор фильтра, 
<ScriptAuthor> - Автор скрипта
!*/


-- Обновление параметров фильтра
-- очистка Текущих параметров фильтра
do
$$
    declare
        tmpUID          uuid     = '<FilterUID>'::uuid;
        tmpFilterID     INT;
        tmpScriptAuthor text     = '<ScriptAuthor>';
        tmpScriptDate   timestamp= current_date;
        tmpScriptName   text ;
        tmpScriptDescr  text ;
        tmpFilterName   text;
        tmpErrorMsg     text;
        tmpErrorText    text;
        tmpErrorContext text;
    begin
        select o."ID", f."DisplayLabel"
        into tmpFilterID, tmpFilterName
        from dbo.ldobject o
                 inner join dbo.ldfilter f on f."ID" = o."ID"
        where o."UID" = tmpUID;

        IF tmpFilterID IS NULL THEN
            tmpErrorMsg := format('Фильтр  с UID "%s" не найден в ldobject', tmpUID);
            raise exception '%',tmpErrorMsg;
        END if;
        tmpScriptName := format('Очистка параметров фильтра %s', tmpFilterName);
        tmpScriptDescr := format('Автор: %s; Дата: %s; %s', tmpScriptAuthor, tmpScriptDate, tmpScriptName);
        raise notice '%', tmpScriptName;

        --Удаление старых параметров
        DELETE
        FROM DBO.LDFILTERPARAM
        WHERE "FilterID" = tmpFilterID;
        raise notice 'Параметры фильтра "%" очищены', tmpFilterName;

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