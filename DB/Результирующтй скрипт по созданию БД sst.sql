--1. Выполнить в консоли запросов
CREATE DATABASE sst
    WITH
    OWNER = dbo
    ENCODING = 'UTF8'
    CONNECTION LIMIT = -1
    IS_TEMPLATE = False;

COMMENT ON DATABASE sst
    IS 'create sst database owner dbo';
	
--2. Открыть консоль запросов на БД sst и выполнить скрипт
CREATE SCHEMA dbo
    AUTHORIZATION dbo;

COMMENT ON SCHEMA dbo
    IS 'create dbo schema';
	
--3. Далее выполнить остальное
BEGIN;


CREATE TABLE IF NOT EXISTS dbo.sst_dbtype
(
    id integer NOT NULL,
    type text COLLATE pg_catalog."default",
    CONSTRAINT dbtypee_pk PRIMARY KEY (id)
);

CREATE TABLE IF NOT EXISTS dbo.sst_ldobjecttype
(
    description text COLLATE pg_catalog."default",
    displaylabel text COLLATE pg_catalog."default",
    id integer NOT NULL,
    name text COLLATE pg_catalog."default",
    uid uuid,
    CONSTRAINT sst_ldobjecttype_pk PRIMARY KEY (id)
);

COMMENT ON TABLE dbo.sst_ldobjecttype
    IS 'Типы объектов';

CREATE TABLE IF NOT EXISTS dbo.sst_model
(
    name text COLLATE pg_catalog."default",
    displaylabel text COLLATE pg_catalog."default",
    description text COLLATE pg_catalog."default",
    "UUID" uuid NOT NULL,
    accesstype integer,
    version integer,
    creator uuid,
    rootmodelitem uuid,
    dbtype integer,
    CONSTRAINT model_pkey PRIMARY KEY ("UUID")
);

COMMENT ON TABLE dbo.sst_model
    IS 'Модели';

CREATE TABLE IF NOT EXISTS dbo.sst_modelaccesstype
(
    id integer NOT NULL,
    type text COLLATE pg_catalog."default",
    CONSTRAINT modelaccesstype_pkey PRIMARY KEY (id)
);

CREATE TABLE IF NOT EXISTS dbo.sst_modelitem
(
    parentmodelitem uuid,
    sqltemplate uuid,
    "UUID" uuid NOT NULL,
    CONSTRAINT modelitem_pkey PRIMARY KEY ("UUID")
);

CREATE TABLE IF NOT EXISTS dbo.sst_project
(
    "UUID" uuid NOT NULL,
    name text COLLATE pg_catalog."default",
    CONSTRAINT sst_project_pkey PRIMARY KEY ("UUID")
);

CREATE TABLE IF NOT EXISTS dbo.sst_sqltemplate
(
    name text COLLATE pg_catalog."default",
    description text COLLATE pg_catalog."default",
    displaylabel text COLLATE pg_catalog."default",
    dbtype integer,
    "UUID" uuid NOT NULL,
    sqltext text COLLATE pg_catalog."default",
    version integer DEFAULT 1,
    creator uuid,
    CONSTRAINT sqltemplate_pkey PRIMARY KEY ("UUID")
);

CREATE TABLE IF NOT EXISTS dbo.sst_stand
(
    "UUID" uuid NOT NULL,
    name text COLLATE pg_catalog."default",
    login text COLLATE pg_catalog."default",
    password text COLLATE pg_catalog."default",
    host text COLLATE pg_catalog."default",
    project_uid uuid,
    CONSTRAINT sst_stand_pkey PRIMARY KEY ("UUID")
);

CREATE TABLE IF NOT EXISTS dbo.sst_tag
(
    name text COLLATE pg_catalog."default",
    displaylabel text COLLATE pg_catalog."default",
    description text COLLATE pg_catalog."default",
    childmodelitem uuid,
    datatype integer,
    ldobjecttype integer,
    tagtemplate uuid,
    tagtype integer,
    "UUID" uuid NOT NULL,
    sqltemplateuid uuid,
    CONSTRAINT tag_pkey PRIMARY KEY ("UUID")
);

COMMENT ON TABLE dbo.sst_tag
    IS 'Тэги';

CREATE TABLE IF NOT EXISTS dbo.sst_tagdatatype
(
    id integer NOT NULL,
    type text COLLATE pg_catalog."default" NOT NULL,
    CONSTRAINT tagdatatype_pkey PRIMARY KEY (id),
    CONSTRAINT type_unic UNIQUE (type)
);

COMMENT ON TABLE dbo.sst_tagdatatype
    IS 'Типы данных тэгов';

CREATE TABLE IF NOT EXISTS dbo.sst_tagtemplate
(
    name text COLLATE pg_catalog."default",
    displaylabel text COLLATE pg_catalog."default",
    description text COLLATE pg_catalog."default",
    datatype integer,
    tagtype uuid,
    "UUID" uuid NOT NULL,
    CONSTRAINT tagtemplate_pkey PRIMARY KEY ("UUID")
);

COMMENT ON TABLE dbo.sst_tagtemplate
    IS 'Шаблоны тэгов';

CREATE TABLE IF NOT EXISTS dbo.sst_tagtype
(
    id integer NOT NULL,
    type uuid,
    CONSTRAINT tagtype_pkey PRIMARY KEY (id),
    CONSTRAINT sst_tagtype_pk UNIQUE (type)
);

COMMENT ON TABLE dbo.sst_tagtype
    IS 'Типы тэгов';

CREATE TABLE IF NOT EXISTS dbo.sst_user
(
    "UUID" uuid NOT NULL,
    login text COLLATE pg_catalog."default",
    password text COLLATE pg_catalog."default",
    name text COLLATE pg_catalog."default",
    CONSTRAINT sst_user_pkey PRIMARY KEY ("UUID")
);

COMMENT ON TABLE dbo.sst_user
    IS 'users logins and passwords';

ALTER TABLE IF EXISTS dbo.sst_model
    ADD CONSTRAINT fk_rootmodelitem FOREIGN KEY (rootmodelitem)
    REFERENCES dbo.sst_modelitem ("UUID") MATCH SIMPLE
    ON UPDATE NO ACTION
    ON DELETE CASCADE
    NOT VALID;


ALTER TABLE IF EXISTS dbo.sst_model
    ADD CONSTRAINT model_dbtype_id_fk FOREIGN KEY (dbtype)
    REFERENCES dbo.sst_dbtype (id) MATCH SIMPLE
    ON UPDATE NO ACTION
    ON DELETE NO ACTION;


ALTER TABLE IF EXISTS dbo.sst_model
    ADD CONSTRAINT sst_model_modelaccesstype_id_fk FOREIGN KEY (accesstype)
    REFERENCES dbo.sst_modelaccesstype (id) MATCH SIMPLE
    ON UPDATE NO ACTION
    ON DELETE CASCADE;


ALTER TABLE IF EXISTS dbo.sst_model
    ADD CONSTRAINT "sst_model_sst_user_UUID_fk" FOREIGN KEY (creator)
    REFERENCES dbo.sst_user ("UUID") MATCH SIMPLE
    ON UPDATE NO ACTION
    ON DELETE CASCADE;


ALTER TABLE IF EXISTS dbo.sst_modelitem
    ADD CONSTRAINT "sst_modelitem_sqltemplate_UUID_fk" FOREIGN KEY (sqltemplate)
    REFERENCES dbo.sst_sqltemplate ("UUID") MATCH SIMPLE
    ON UPDATE NO ACTION
    ON DELETE NO ACTION;


ALTER TABLE IF EXISTS dbo.sst_sqltemplate
    ADD CONSTRAINT sqltemplate_dbtype_id_fk FOREIGN KEY (dbtype)
    REFERENCES dbo.sst_dbtype (id) MATCH SIMPLE
    ON UPDATE NO ACTION
    ON DELETE NO ACTION;


ALTER TABLE IF EXISTS dbo.sst_sqltemplate
    ADD CONSTRAINT "sst_sqltemplate_sst_user_UUID_fk" FOREIGN KEY (creator)
    REFERENCES dbo.sst_user ("UUID") MATCH SIMPLE
    ON UPDATE NO ACTION
    ON DELETE NO ACTION;


ALTER TABLE IF EXISTS dbo.sst_stand
    ADD CONSTRAINT "sst_stand_sst_project_UUID_fk" FOREIGN KEY (project_uid)
    REFERENCES dbo.sst_project ("UUID") MATCH SIMPLE
    ON UPDATE NO ACTION
    ON DELETE NO ACTION;


ALTER TABLE IF EXISTS dbo.sst_tag
    ADD CONSTRAINT "sst_tag_sst_sqltemplate_UUID_fk" FOREIGN KEY (sqltemplateuid)
    REFERENCES dbo.sst_sqltemplate ("UUID") MATCH SIMPLE
    ON UPDATE NO ACTION
    ON DELETE NO ACTION;


ALTER TABLE IF EXISTS dbo.sst_tag
    ADD CONSTRAINT sst_tag_tagdatatype_id_fk FOREIGN KEY (datatype)
    REFERENCES dbo.sst_tagdatatype (id) MATCH SIMPLE
    ON UPDATE NO ACTION
    ON DELETE NO ACTION;


ALTER TABLE IF EXISTS dbo.sst_tag
    ADD CONSTRAINT "sst_tag_tagtemplate_UUID_fk" FOREIGN KEY (tagtemplate)
    REFERENCES dbo.sst_tagtemplate ("UUID") MATCH SIMPLE
    ON UPDATE NO ACTION
    ON DELETE NO ACTION;


ALTER TABLE IF EXISTS dbo.sst_tag
    ADD CONSTRAINT sst_tag_tagtype_id_fk FOREIGN KEY (tagtype)
    REFERENCES dbo.sst_tagtype (id) MATCH SIMPLE
    ON UPDATE NO ACTION
    ON DELETE NO ACTION;


ALTER TABLE IF EXISTS dbo.sst_tag
    ADD CONSTRAINT "tag_modelitem_UUID_fk" FOREIGN KEY (childmodelitem)
    REFERENCES dbo.sst_modelitem ("UUID") MATCH SIMPLE
    ON UPDATE NO ACTION
    ON DELETE NO ACTION;


ALTER TABLE IF EXISTS dbo.sst_tagtemplate
    ADD CONSTRAINT sst_tagtemplate_tagtype_type_fk FOREIGN KEY (tagtype)
    REFERENCES dbo.sst_tagtype (type) MATCH SIMPLE
    ON UPDATE NO ACTION
    ON DELETE NO ACTION;


ALTER TABLE IF EXISTS dbo.sst_tagtemplate
    ADD CONSTRAINT tagtemplate_tagdatatype_id_fk FOREIGN KEY (datatype)
    REFERENCES dbo.sst_tagdatatype (id) MATCH SIMPLE
    ON UPDATE NO ACTION
    ON DELETE NO ACTION;

END;