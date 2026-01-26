/*!
Этот комментарий не входит в общий скрипт.
Список и описание параметров шаблона:
<ReportName>      - Название отчета, 
<ReportPlaceName> - Место отображения отчета: 'Document' - 'Из документа' , 'Sidebar' - 'Из узла "Избранные отчеты" сайдбара'
<ReportUID>       - Уникальный идентификатор отчета, 
<ReportExportMode>- Формат экспорта Отчета
<ScriptAuthor>    - Автор скрипта
!*/
do
$$
    DECLARE
        tmpScriptName           text;
        tmpScriptDescr          text;
        tmpErrorMsg             text;
        tmpErrorText            text;
        tmpErrorContext         text;
        tmpReportID             INT;
        tmpReportName           text= '<ReportName>';
        tmpReportTemplateName   text= '<ReportName>';
        tmpReportTypeName       text= 'Fast Report .Net';
        tmpReportTypeID         INT;
        tmpUID                  UUID= '<ReportUID>'::uuid;
        tmpReportPlaceName      text = '<ReportPlaceName>';
        tmpReportExportModeName text = '<ReportExportMode>';
        tmpOpertTypeID          INT;
        tmpReportPlaceID        int;
        tmpReportExportID       int;
        tmpOperID               INT;
        tmpLogMsg               text;
        tmpOrdNum               INT;
        tmpOperTypeName         text;
        tmpScriptAuthor         text = '<ScriptAuthor>';
        tmpScriptDate           timestamp= current_date;
        tmpNewReportVersion     int;
    BEGIN
        tmpScriptName := format('Разработка отчета "%s"', tmpReportName);
        tmpScriptDescr := format('Автор: %s; Дата: %s; "%s"', tmpScriptAuthor, tmpScriptDate, tmpScriptName);

        --Определяем тип отчета
        tmpReportTypeID :=
                (SELECT "ID" FROM dbo.LDREPORTTYPE WHERE trim(lower("Name")) = trim(lower(tmpReportTypeName)));
        IF (tmpReportTypeID IS NULL) THEN
            tmpErrorMsg := 'Не удалось определить идентификатор типа отчета с именем "' || tmpReportTypeName || '"';
            raise exception '%',tmpErrorMsg;
        END IF;

        --Определяем идентификатор отчета
        tmpReportID := (SELECT "ID" FROM dbo.LDOBJECT WHERE "UID" = tmpUID);
        IF (tmpReportID IS NOT NULL AND
            NOT EXISTS(SELECT NULL FROM dbo.LDREPORTNAMES WHERE "ID" = tmpReportID)) THEN
            tmpErrorMsg := 'В базе данных уже есть объект с идентификатором "' || CAST(tmpUID as varchar) || '"';
            raise exception '%',tmpErrorMsg;
        END IF;
        IF (tmpReportID IS NULL) THEN
            --Регистрируем отчет
            tmpReportID :=
                    dbo.LDF_NEREPORT(NULL, NULL, tmpReportName, tmpReportTemplateName, tmpReportTypeID, N'', NULL);
            UPDATE dbo.LDOBJECT SET "UID" = tmpUID WHERE "ID" = tmpReportID;
            update dbo.ldreportnames rn set "TemplateUid" = tmpUID where rn."ID" = tmpReportID;
            --Делаем запись в логе
            tmpOpertTypeID := NULL;
            tmpOpertTypeID := (SELECT "ID" FROM dbo.LDOPERTYPE WHERE "Name" = 'NEWREPORTVOC');
            tmpLogMsg := format('Регистрация кастомного отчета. Автор: "%s"', tmpScriptAuthor);
            tmpOperID := dbo.LDF_SAVEOPERTOLOG(NULL, NULL, tmpOpertTypeID, 2, NULL, tmpLogMsg, tmpReportID);
        ELSE
            UPDATE dbo.LDREPORTNAMES
            SET "Name"         = tmpReportName,
                "TemplateName" = tmpReportTemplateName,
                "ReportTypeID" = tmpReportTypeID,
                "TemplateUid"  = tmpUID
            WHERE "ID" = tmpReportID;
        END IF;
        
        -- Добавление в ld3reportinfo
        select rem."ID"
        into tmpReportExportID
        from dbo.ld3reportexportmode rem
        where trim(lower(rem."Name")) = trim(lower(tmpReportExportModeName));
        
        if not exists (select null from dbo.ld3reportinfo l3r where l3r."TemplateID" = tmpReportID)
        then
            tmpNewReportVersion := ldf_newobject(null, 305, 1);
            INSERT INTO dbo.ld3reportinfo
            ("ID", "TemplateID", "VersionN", "CreateDateTime", "IsActive", "VersionAuthor", "ReportExportMode")
            VALUES (tmpNewReportVersion,
                    tmpReportID,
                    1,
                    current_date,
                    '+',
                    'SUPERVISOR',
                    tmpReportExportID);
        else
            update dbo.ld3reportinfo ri
            set "ReportExportMode" = tmpReportExportID
            where ri."TemplateID" = tmpReportID;
        end if;

        -- Добавление в ld3reportplacelink
        select pr."ID"
        into tmpReportPlaceID
        from dbo.ld3reportplace pr
        where trim(lower(pr."Name")) = trim(lower(tmpReportPlaceName));
        
        if not exists(select null from dbo.ld3reportplacelink rpl where rpl."ReportID" = tmpReportID)
        then
            insert into dbo.ld3reportplacelink ("ReportID", "ReportPlaceID")
            values (tmpReportID, tmpReportPlaceID);
        else
            update dbo.ld3reportplacelink l3r
            set "ReportPlaceID" = tmpReportPlaceID
            where l3r."ReportID" = tmpReportID;
        end if;

        --Привязка отчета к операциям
        tmpOpertTypeID := NULL;
        tmpOrdNum := NULL;
        tmpOperTypeName := N'DOCUMENTLIST';

        --Опеределяем идентификатор операции
        tmpOpertTypeID := (SELECT "ID" FROM dbo.LDOPERTYPE WHERE "Name" = tmpOperTypeName);
        IF (tmpOpertTypeID IS NULL) THEN
            tmpErrorMsg := 'Не удалось определить идентификатор типа операции с именем "' || tmpOperTypeName || '"';
            raise exception '%',tmpErrorMsg;
        END IF;

        --Прикрепляем отчет к операции
        IF NOT EXISTS(SELECT NULL
                      FROM dbo.LDOPERREPORT
                      WHERE "OperTypeID" = tmpOpertTypeID
                        AND "ReportID" = tmpReportID) THEN
            tmpOrdNum := (SELECT (MAX("OrdNum") + 1) FROM dbo.LDOPERREPORT);
            INSERT INTO dbo.LDOPERREPORT ("OperTypeID", "OrdNum", "ReportID")
            VALUES (tmpOpertTypeID, coalesce(tmpOrdNum, 1), tmpReportID);
        END IF;

        INSERT INTO dbo.LDUsedScripts (name, description, userlogin, ExecDate)
        SELECT tmpScriptName, tmpScriptDescr, current_user, current_timestamp;
    exception
        when others then
            get stacked diagnostics tmpErrorMsg = message_text,
                tmpErrorText = pg_exception_detail,
                tmpErrorContext = pg_exception_context;
            tmpErrorMsg := 'При выполнении задачи "' || tmpScriptName || '" произошла ошибка: ' || tmpErrorMsg;
            insert into dbo.LDUsedScripts (name, description, userlogin, execdate)
            select tmpScriptName, tmpScriptDescr, current_user, current_timestamp;
            raise notice '%детально: %контекст: %',tmpErrorMsg || ',' || chr(10),tmpErrorText || ',' || chr(10),tmpErrorContext;
    end;
$$;