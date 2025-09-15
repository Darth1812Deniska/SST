/*!  
Этот комментарий не входит в общий скрипт.  
Список и описание параметров шаблона:  
<ObjectTypeDisplayLabel> - Отображаемое имя типа объекта,  
<ObjectTypeID>           - Заданный идентификатор итпа объекта,  
<ObjectTypeName>         - Физическое имя типа объекта,  
<ObjectTypeParent>       - Родительский тип объекта,  
<ObjectTypeUID>          - Уникальный идентификатор типа объекта (UID),  
<ObjTpCategoryID>        - Категория типа объекта,  
<TableName>              - Таблица основание типа объекта  
!*/  
do  
$$  
    declare  
        tmpName         text= '<ObjectTypeName>';  
        tmpObjectTypeID int= <ObjectTypeID>;  
        tmpDisplayLabel text= '<ObjectTypeDisplayLabel>';  
        tmpTable        text= '<TableName>';  
        tmpView         text= '<TableName>';  
        tmpParent       text= '<ObjectTypeParent>';  
        tmpUID          uuid= '<ObjectTypeUID>'::uuid;  
        tmpObjTpCategoryID int= <ObjTpCategoryID>;  
        tmpParentID     int ;  
        tmpTableID      int ;  
        tmpViewID       int;  
        tmpErrorMsg     text ;      
    begin  
        IF EXISTS(SELECT NULL FROM DBO.LDOBJECTTYPE WHERE "Name" = tmpName AND "ID" != tmpObjectTypeID)  
        then  
            tmpErrorMsg := 'Тип данных с именем "' || tmpName || '" уже существует, но под другим идентификатором';  
            raise exception '%',tmpErrorMsg;  
        END if;  
  
        IF EXISTS(SELECT NULL FROM DBO.LDOBJECTTYPE WHERE "Name" != tmpName AND "ID" = tmpObjectTypeID)  
        then  
            tmpErrorMsg := 'Тип данных с идентификатором "' || CAST(tmpObjectTypeID as varchar) ||  
                           '" уже существует, но под другим именем';  
            raise exception '%',tmpErrorMsg;  
        END if;  
  
        --Определяем TableID  
        SELECT "ID" into tmpTableID FROM DBO.LDTABLE WHERE lower("Name") = lower(tmpTable);  
        IF tmpTableID IS NULL  
        then            tmpErrorMsg := 'Таблица с именем "' || tmpTable || '" не найдена в LDTable';  
            raise exception '%',tmpErrorMsg;  
        END if;  
  
        --Определяем ViewTableID  
        SELECT "ID" into tmpViewID FROM DBO.LDTABLE WHERE lower("Name") = lower(tmpView);  
  
        --Определяем ParentID  
        SELECT "ID" into tmpParentID FROM DBO.LDOBJECTTYPE WHERE "Name" = tmpParent;  
        IF tmpParentID IS NULL  
        then            tmpErrorMsg := 'Тип объекта с именем "' || tmpParent || '" не найден в LDObjectType';  
            raise exception '%',tmpErrorMsg;  
        END if;  
  
        IF EXISTS(SELECT NULL FROM DBO.LDOBJECTTYPE WHERE "Name" = tmpName AND "ID" = tmpObjectTypeID)  
        then  
            update DBO.LDOBJECTTYPE  
            set "DisplayLabel" = tmpDisplayLabel,  
                "ParentID"     = tmpParentID,  
                "ViewTableID"  = tmpViewID,  
                "TableID"      = tmpTableID  
            where "ID" = tmpObjectTypeID;  
        end if;  
  
        IF NOT EXISTS(SELECT NULL FROM DBO.LDOBJECTTYPE WHERE "Name" = tmpName AND "ID" = tmpObjectTypeID)  
        then  
            --Добавляем запись в LDOBJECTTYPE  
            INSERT INTO DBO.LDOBJECTTYPE  
            ( "ID"  
            , "Name"  
            , "DisplayLabel"  
            , "AccessMode"  
            , "ParentID"  
            , "System"  
            , "FormID"  
            , "ViewTableID"  
            , "ResName"  
            , "VersionESign"  
            , "TableID"  
            , "ObjTpCategoryID"  
            , "AddFormID"  
            , "UID"  
            , "VersionAllow")  
            VALUES ( tmpObjectTypeID  
                   , tmpName  
                   , tmpDisplayLabel  
                   , 0  
                   , tmpParentID  
                   , '-'  
                   , NULL  
                   , tmpViewID  
                   , ''  
                   , 0  
                   , tmpTableID  
                   , tmpObjTpCategoryID  
                   , NULL  
                   , tmpUID  
                   , '-');  
        END if;  
    end;  
$$;