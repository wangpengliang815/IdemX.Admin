-- IdemX.Admin 骨架种子数据（PostgreSQL）
-- 用法：建表后执行 psql -d "IdemX.Admin.db" -f scripts/postgresql/seed.sql
-- 默认管理员：admin / 123456（密码为 BCrypt 摘要，非明文）
-- 仅用于空库首次初始化；重复执行可能主键冲突

BEGIN;

-- 固定 ID，便于菜单 parent / 权限关联
-- 1800000000000000001 admin 角色
-- 角色编码与 AuthConstants.AdminRoleCode / RegisteredUserRoleCode 一致
-- 1800000000000000002 admin 用户
-- 1800000000000000003 用户-角色
-- 1800000000000000004..11 系统菜单树
-- 1800000000000000101..108 角色-菜单

INSERT INTO public.sys_role (
    id, role_name, role_code, create_time, is_deleted
) VALUES
    (1800000000000000001, '管理员', 'admin', CURRENT_TIMESTAMP, FALSE),
    (1800000000000000012, '注册用户', 'registered', CURRENT_TIMESTAMP, FALSE);

INSERT INTO public.sys_user (
    id, user_name, password, nick_name, sex, phone, email,
    status, user_type, create_time, is_deleted
) VALUES (
    1800000000000000002,
    'admin',
    '$2a$11$wC9FJlTP.fD/004H2CC81ulW8mw8EDJMi3fBtlDoPNF3nQ/f1.xxu',
    'admin',
    1,
    '13800000000',
    'admin@example.com',
    0,
    0,
    CURRENT_TIMESTAMP,
    FALSE
);

INSERT INTO public.sys_user_role (
    id, user_id, role_id, create_time, is_deleted
) VALUES (
    1800000000000000003,
    1800000000000000002,
    1800000000000000001,
    CURRENT_TIMESTAMP,
    FALSE
);

INSERT INTO public.sys_menu (
    id, parent_id, name, path, component, title, icon, sort,
    affix_tab, is_external, keep_alive, menu_type, status, create_time, is_deleted
) VALUES
    (1800000000000000004, NULL, 'system', '/system', '/system/index', '后台管理', 'mdi:cog', 999,
        FALSE, FALSE, TRUE, 0, 1, CURRENT_TIMESTAMP, FALSE),
    (1800000000000000005, 1800000000000000004, 'system_menu', '/system/menu', '/system/menu/index', '菜单管理', 'mdi:menu', 1,
        FALSE, FALSE, TRUE, 0, 1, CURRENT_TIMESTAMP, FALSE),
    (1800000000000000006, 1800000000000000004, 'system_role', '/system/role', '/system/role/index', '角色管理', 'mdi:filter-cog', 2,
        FALSE, FALSE, TRUE, 0, 1, CURRENT_TIMESTAMP, FALSE),
    (1800000000000000007, 1800000000000000004, 'system_user', '/system/user', '/system/user/index', '用户管理', 'mdi:account', 3,
        FALSE, FALSE, TRUE, 0, 1, CURRENT_TIMESTAMP, FALSE),
    (1800000000000000008, 1800000000000000004, 'system_org', '/system/org', '/system/org/index', '组织管理', 'mdi:sitemap', 4,
        FALSE, FALSE, TRUE, 0, 1, CURRENT_TIMESTAMP, FALSE),
    (1800000000000000009, 1800000000000000004, 'system_record', '/system/record', '/system/record/login/index', '日志管理', 'mdi:clipboard-text-outline', 5,
        FALSE, FALSE, TRUE, 0, 1, CURRENT_TIMESTAMP, FALSE),
    (1800000000000000010, 1800000000000000009, 'system_record_login', '/system/record/login', '/system/record/login/index', '登录日志', 'mdi:login', 1,
        FALSE, FALSE, TRUE, 0, 1, CURRENT_TIMESTAMP, FALSE),
    (1800000000000000011, 1800000000000000009, 'system_record_nlog', '/system/record/nlog', '/system/record/nlog/index', '操作日志', 'mdi:text-box-outline', 2,
        FALSE, FALSE, TRUE, 0, 1, CURRENT_TIMESTAMP, FALSE);

INSERT INTO public.sys_role_menu (
    id, role_id, menu_id, create_time, is_deleted
) VALUES
    (1800000000000000101, 1800000000000000001, 1800000000000000004, CURRENT_TIMESTAMP, FALSE),
    (1800000000000000102, 1800000000000000001, 1800000000000000005, CURRENT_TIMESTAMP, FALSE),
    (1800000000000000103, 1800000000000000001, 1800000000000000006, CURRENT_TIMESTAMP, FALSE),
    (1800000000000000104, 1800000000000000001, 1800000000000000007, CURRENT_TIMESTAMP, FALSE),
    (1800000000000000105, 1800000000000000001, 1800000000000000008, CURRENT_TIMESTAMP, FALSE),
    (1800000000000000106, 1800000000000000001, 1800000000000000009, CURRENT_TIMESTAMP, FALSE),
    (1800000000000000107, 1800000000000000001, 1800000000000000010, CURRENT_TIMESTAMP, FALSE),
    (1800000000000000108, 1800000000000000001, 1800000000000000011, CURRENT_TIMESTAMP, FALSE);

COMMIT;
