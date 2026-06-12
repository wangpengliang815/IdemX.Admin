-- IdemX.Admin 骨架库表结构（PostgreSQL）
-- 与 Core.Model 实体 + SqlSugar 驼峰转下划线映射对齐
-- 用法：先 CREATE DATABASE，再依次执行 schema.sql、seed.sql

BEGIN;

CREATE TABLE IF NOT EXISTS public.sys_user (
    id                  BIGINT          NOT NULL PRIMARY KEY,
    user_name           VARCHAR(50)     NOT NULL,
    password            VARCHAR(100)    NOT NULL,
    nick_name           VARCHAR(200),
    avatar              VARCHAR(200),
    sex                 INTEGER         NOT NULL,
    phone               VARCHAR(50)     NOT NULL,
    email               VARCHAR(200),
    sys_org_id          BIGINT,
    status              INTEGER         NOT NULL DEFAULT 0,
    id_card_number      VARCHAR(18),
    create_time         TIMESTAMP       NOT NULL,
    create_by           BIGINT,
    create_by_username  VARCHAR(64),
    update_time         TIMESTAMP,
    update_by           BIGINT,
    update_by_username  VARCHAR(64),
    is_deleted          BOOLEAN         NOT NULL DEFAULT FALSE,
    delete_time         TIMESTAMP,
    delete_by           BIGINT,
    delete_by_username  VARCHAR(64)
);

CREATE TABLE IF NOT EXISTS public.sys_role (
    id                  BIGINT          NOT NULL PRIMARY KEY,
    role_name           VARCHAR(50)     NOT NULL,
    role_code           VARCHAR(50)     NOT NULL,
    memo                VARCHAR(255),
    create_time         TIMESTAMP       NOT NULL,
    create_by           BIGINT,
    create_by_username  VARCHAR(64),
    update_time         TIMESTAMP,
    update_by           BIGINT,
    update_by_username  VARCHAR(64),
    is_deleted          BOOLEAN         NOT NULL DEFAULT FALSE,
    delete_time         TIMESTAMP,
    delete_by           BIGINT,
    delete_by_username  VARCHAR(64)
);

CREATE TABLE IF NOT EXISTS public.sys_user_role (
    id                  BIGINT          NOT NULL PRIMARY KEY,
    user_id             BIGINT          NOT NULL,
    role_id             BIGINT          NOT NULL,
    create_time         TIMESTAMP       NOT NULL,
    create_by           BIGINT,
    create_by_username  VARCHAR(64),
    update_time         TIMESTAMP,
    update_by           BIGINT,
    update_by_username  VARCHAR(64),
    is_deleted          BOOLEAN         NOT NULL DEFAULT FALSE,
    delete_time         TIMESTAMP,
    delete_by           BIGINT,
    delete_by_username  VARCHAR(64)
);

CREATE TABLE IF NOT EXISTS public.sys_role_menu (
    id                  BIGINT          NOT NULL PRIMARY KEY,
    role_id             BIGINT          NOT NULL,
    menu_id             BIGINT          NOT NULL,
    create_time         TIMESTAMP       NOT NULL,
    create_by           BIGINT,
    create_by_username  VARCHAR(64),
    update_time         TIMESTAMP,
    update_by           BIGINT,
    update_by_username  VARCHAR(64),
    is_deleted          BOOLEAN         NOT NULL DEFAULT FALSE,
    delete_time         TIMESTAMP,
    delete_by           BIGINT,
    delete_by_username  VARCHAR(64)
);

CREATE TABLE IF NOT EXISTS public.sys_org (
    id                  BIGINT          NOT NULL PRIMARY KEY,
    parent_id           BIGINT,
    name                VARCHAR(200)    NOT NULL,
    sort                INTEGER         NOT NULL,
    create_time         TIMESTAMP       NOT NULL,
    create_by           BIGINT,
    create_by_username  VARCHAR(64),
    update_time         TIMESTAMP,
    update_by           BIGINT,
    update_by_username  VARCHAR(64),
    is_deleted          BOOLEAN         NOT NULL DEFAULT FALSE,
    delete_time         TIMESTAMP,
    delete_by           BIGINT,
    delete_by_username  VARCHAR(64)
);

CREATE TABLE IF NOT EXISTS public.sys_menu (
    id                      BIGINT          NOT NULL PRIMARY KEY,
    parent_id               BIGINT,
    name                    VARCHAR(200)    NOT NULL,
    path                    VARCHAR(255),
    component               VARCHAR(255),
    redirect                VARCHAR(255),
    title                   VARCHAR(200)    NOT NULL,
    icon                    VARCHAR(100),
    sort                    INTEGER         NOT NULL,
    authority               VARCHAR(200),
    roles                   VARCHAR(500),
    affix_tab               BOOLEAN         NOT NULL DEFAULT FALSE,
    is_external             BOOLEAN         NOT NULL DEFAULT FALSE,
    external_url            VARCHAR(500),
    iframe_url              VARCHAR(500),
    keep_alive              BOOLEAN         NOT NULL DEFAULT TRUE,
    menu_type               INTEGER         NOT NULL,
    badge                   VARCHAR(50),
    badge_type              VARCHAR(50),
    badge_variants          VARCHAR(50),
    active_menu             VARCHAR(255),
    breadcrumb_parent_icon  VARCHAR(100),
    link                    VARCHAR(500),
    status                  INTEGER         NOT NULL,
    create_time             TIMESTAMP       NOT NULL,
    create_by               BIGINT,
    create_by_username      VARCHAR(64),
    update_time             TIMESTAMP,
    update_by               BIGINT,
    update_by_username      VARCHAR(64),
    is_deleted              BOOLEAN         NOT NULL DEFAULT FALSE,
    delete_time             TIMESTAMP,
    delete_by               BIGINT,
    delete_by_username      VARCHAR(64)
);

CREATE TABLE IF NOT EXISTS public.sys_area (
    id                  BIGINT          NOT NULL PRIMARY KEY,
    code                VARCHAR(64)     NOT NULL,
    name                VARCHAR(200)    NOT NULL,
    parent_code         VARCHAR(64)     NOT NULL,
    level               INTEGER         NOT NULL,
    create_time         TIMESTAMP       NOT NULL,
    create_by           BIGINT,
    create_by_username  VARCHAR(64),
    update_time         TIMESTAMP,
    update_by           BIGINT,
    update_by_username  VARCHAR(64),
    is_deleted          BOOLEAN         NOT NULL DEFAULT FALSE,
    delete_time         TIMESTAMP,
    delete_by           BIGINT,
    delete_by_username  VARCHAR(64)
);

CREATE TABLE IF NOT EXISTS public.sys_record_login (
    id                  BIGINT          NOT NULL PRIMARY KEY,
    user_name           VARCHAR(50)     NOT NULL,
    os                  VARCHAR(200)    NOT NULL,
    browser             TEXT,
    oper_type           INTEGER         NOT NULL,
    comments            VARCHAR(200),
    login_source        VARCHAR(200)    NOT NULL,
    create_time         TIMESTAMP       NOT NULL,
    create_by           BIGINT,
    create_by_username  VARCHAR(64),
    update_time         TIMESTAMP,
    update_by           BIGINT,
    update_by_username  VARCHAR(64),
    is_deleted          BOOLEAN         NOT NULL DEFAULT FALSE,
    delete_time         TIMESTAMP,
    delete_by           BIGINT,
    delete_by_username  VARCHAR(64)
);

CREATE TABLE IF NOT EXISTS public.sys_record_nlog (
    id                          INTEGER         GENERATED BY DEFAULT AS IDENTITY PRIMARY KEY,
    log_date                    TIMESTAMP,
    log_level                   VARCHAR(64),
    log_type                    VARCHAR(128),
    log_title                   VARCHAR(256),
    logger                      VARCHAR(256),
    message                     TEXT,
    exception                   TEXT,
    machine_name                VARCHAR(256),
    machine_ip                  VARCHAR(64),
    net_request_method          VARCHAR(16),
    net_request_url             TEXT,
    net_user_isauthenticated    VARCHAR(16),
    net_user_authtype           VARCHAR(64),
    net_user_identity           TEXT
);

COMMIT;
