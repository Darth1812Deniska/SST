/*!
Этот комментарий не входит в общий скрипт.
Список и описание параметров шаблона:
<ControlClass> - Тип контрола для поля: 'TLDLblEditBtn' - выбор из списка, 'TLDLblEditDate' -выбор даты, 'TLDLblComboBox' - комбобокс (не работает), 'TLDLblEdit' - текстовое поле, 
<DisplayLabel> - Отображаемое название поля, 
<FilterUID>    - Уникальный идентификатор фильтра, 
<FromViewName> - Представление из которого возвращается список значений, 
<ParamName>    - Физическое название параметра, 
<ParamTypeID>  - Тип данных параметра, 
<ScriptAuthor> - Автор изменения
!*/
-- добавление параметров фильтров
--'TLDLblEditBtn', 'TLDLblEditDate', 'TLDLblComboBox', 'TLDLblEdit'
do
$$
    declare
        tmpUID          uuid     = '<FilterUID>'::uuid;
        tmpParamName    text='<ParamName>';
        tmpOrdNum       int;
        tmpParamTypeID  int      = <ParamTypeID>;
        tmpDisplayLabel text     ='<DisplayLabel>';
        tmpControlClass text     = '<ControlClass>';
        tmpFromViewName text     = '<FromViewName>';
        tmpProperties   text;
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
        tmpScriptName := format('Добавление параметра "%s" фильтра %s', tmpParamName, tmpFilterName);
        tmpScriptDescr := format('Автор: %s; Дата: %s; "%s"', tmpScriptAuthor, tmpScriptDate, tmpScriptName);
        raise notice '%', tmpScriptName;

        -- Задание порядкового номера
        if tmpOrdNum is null
        then
            select max(fp."Number") + 1
            into tmpOrdNum
            from dbo.ldfilterparam fp
            where fp."FilterID" = tmpFilterID;
        end if;

        if tmpControlClass in ('TLDLblEditBtn', 'TLDLblComboBox')
        then
            tmpProperties :=
                    format(N'TableID=''%s'';KeyPressMode=2;InfoField=''Name'';PKeyField=''ID''', tmpFromViewName);
        end if;

        INSERT INTO DBO.LDFILTERPARAM ("FilterID", "Number", "Name", "TypeID", "DisplayLabel", "ControlClass",
                                       "Properties", "LD3Properties")
        SELECT tmpFilterID,
               tmpOrdNum,
               tmpParamName,
               tmpParamTypeID,
               tmpDisplayLabel,
               tmpControlClass,
               tmpProperties,
               NULL;

        UPDATE dbo.LDFILTERPARAM
        SET "Properties"    = 'TypedParam=1',
            "LD3Properties" = 'TypedParam=1'
        WHERE "FilterID" = tmpFilterID
          AND "TypeID" in (3, 4, 12, 14);

        IF tmpFilterID IS NULL THEN
            tmpErrorMsg := format('Фильтр  с UID "%s" не найден в ldobject', tmpUID);
            raise exception '%',tmpErrorMsg;
        END if;
        tmpScriptName := format('Добавление параметра для фильтра %s', tmpFilterName);
        tmpScriptDescr := format('Автор: %s; Дата: %s; %s', tmpScriptAuthor, tmpScriptDate, tmpScriptName);
        raise notice '%', tmpScriptName;

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
