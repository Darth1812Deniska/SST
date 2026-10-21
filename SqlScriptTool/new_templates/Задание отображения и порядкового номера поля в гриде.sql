/*!  
Этот комментарий не входит в общий скрипт.  
Список и описание параметров шаблона:  
<AttributeName>  - ИМя поля или атрибута,  
<DisplayLabel>   - Отображаемое имя поля или атрибута в гриде,  
<ObjectTypeName> - Тип объекта для которого применяется изменения,  
<ScriptAuthor>   - Автор изменения,  
<VisIndex>       - порядковый номер поля или атрибута в гриде  
!*/  
  
do  
$$  
    declare  
        tmpErrorContext       text ;  
        tmpErrorText          text ;  
        tmpLDUsedScript_Descr text ;  
        tmpLDUsedScript_Name  text ;  
        tmpErrorMsg           text ;  
        tmpObjectTypeID       int ;  
        tmpAttributeName      text= '<AttributeName>';  
        tmpObjectTypeName     text= '<ObjectTypeName>';  
        tmpDisplayLabel       text= <DisplayLabel>; -- Если поле не выводится => tmpDisplayLabel    := NULL  
        tmpVisIndex           int= <VisIndex>; -- Если выводится по умолчанию => tmpVisIndex > -1  
        tmpScriptName         text ;  
        tmpScriptDate         timestamp= current_date;  
        tmpScriptAuthor       text= '<ScriptAuthor>';  
    begin  
        tmpScriptName := format('Редактирование LD3Overlay и ld30visibleindex %s.%s',  
                                tmpObjectTypeName, tmpAttributeName);  
        raise notice '%',tmpScriptName;  
        --Определяем тип объекта LanDocs  
        SELECT "ID"  
        into tmpObjectTypeID  
        FROM DBO.LDOBJECTTYPE  
        WHERE trim(lower("Name")) = trim(lower(tmpObjectTypeName));  
        IF tmpObjectTypeID IS NULL  
        then            tmpErrorMsg := format('Тип объекта с именем "%s" не найден в LDOBJECTTYPE', tmpObjectTypeName);  
            raise exception '%',tmpErrorMsg;  
        END if;  
  
        INSERT INTO dbo.ld3overlay  
            ("ObjectTypeID", "AttributeName", "DisplayLabel", "VisibleIndex")  
        values (tmpObjectTypeID, tmpAttributeName, tmpDisplayLabel, tmpVisIndex)  
        ON CONFLICT ("ObjectTypeID", "AttributeName")  
            DO UPDATE  
            SET "DisplayLabel"=EXCLUDED."DisplayLabel", "VisibleIndex"=EXCLUDED."VisibleIndex";  
  
        if COALESCE(tmpDisplayLabel, '') = ''  
        then  
            delete            from dbo.ld30visibleindex l30v  
            where l30v."ObjectTypeID" = tmpObjectTypeID  
              and trim(lower(l30v."Name")) = trim(lower(tmpAttributeName));  
        else  
            INSERT INTO dbo.ld30visibleindex  
                ("ObjectTypeID", "Name", "OrdNum", "Deleted")  
            values (tmpObjectTypeID, tmpAttributeName, tmpVisIndex, '-')  
            ON CONFLICT ("ObjectTypeID", "Name")  
                DO UPDATE  
                SET "OrdNum"=EXCLUDED."OrdNum";  
        end if;  
        tmpLDUsedScript_Name :=  
                format('"Редактирование LD3Overlay %s.%s" от "%s" автор "%s"', tmpObjectTypeName,  
                       tmpAttributeName, tmpScriptDate, tmpScriptAuthor);  
        tmpLDUsedScript_Descr := tmpScriptName;  
        insert into DBO.LDUsedScripts (name, description, userlogin, ExecDate)  
        select tmpLDUsedScript_Name, tmpLDUsedScript_Descr, current_user, current_timestamp;  
    exception  
        when others then            get stacked diagnostics tmpErrorMsg = message_text,  
                tmpErrorText = pg_exception_detail,  
                tmpErrorContext = pg_exception_context;  
            tmpErrorMsg := 'При выполнении задачи "' || tmpScriptName || '" произошла ошибка: ' || tmpErrorMsg;  
            tmpLDUsedScript_Name :=  
                    format('"Редактирование LD3Overlay %s.%s" от "%s" автор "%s"', tmpObjectTypeName,  
                           tmpAttributeName, tmpScriptDate, tmpScriptAuthor);  
            tmpLDUsedScript_Descr := tmpErrorMsg;  
            insert into DBO.LDUsedScripts (name, description, userlogin, execdate)  
            select tmpLDUsedScript_Name, tmpLDUsedScript_Descr, current_user, current_timestamp;  
            raise notice '%детально: %контекст: %',tmpErrorMsg || ',' || chr(10),tmpErrorText || ',' || chr(10),tmpErrorContext;  
    end;  
  
$$;