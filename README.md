# IdemX.Admin

可复用的 **管理端项目骨架**，从业务系统中剥离出来，供新项目直接 fork / 复制后扩展。

**不是**某个具体业务产品。仓库里不含订单、合约、企业、钱包等业务模块；也不含开放注册、C 端自助开户等流程。

---

## 设计约定

| 约定 | 说明 |
|---|---|
| 账号来源 | 仅 **后台用户管理** 开通，无前台注册 |
| 登录方式 | **账号密码**为主；找回密码 / 换绑手机走短信（需配阿里云 SMS） |
| 权限模型 | **RBAC**（用户 → 角色 → 菜单），无「用户类型」字段 |
| 数据库 | **SQL 脚本**建表与种子（`schema.sql` + `seed.sql`），不做 CodeFirst 自动迁移 |
| 冷启动数据 | 默认 `admin` 角色与用户、系统菜单写在 `seed.sql`；省市区可选 API 导入 |

默认管理员：`admin` / `123456`（见 `seed.sql`，密码为 BCrypt 摘要）。

---

## 技术栈

| 层 | 技术 |
|---|---|
| 后端 | .NET 8、SqlSugar、JWT、Autofac、AutoMapper |
| 前端 | Vue 3、Vben Admin 5（Ant Design Vue）、pnpm monorepo |
| 数据库 | PostgreSQL（默认，可切 SqlServer） |

---

## 目录结构

```
IdemX.Admin/
├── fullstack.code-workspace        # VS Code / Cursor 多根工作区（推荐从此打开）
├── scripts/postgresql/
│   ├── schema.sql                  # 建表（与 Core.Model 实体对齐）
│   └── seed.sql                    # 种子：admin 角色/用户、系统菜单与权限
├── src/backend/
│   ├── Core.Model                  # 实体、DTO、枚举
│   ├── Core.Infrastructure         # 基础设施（JWT、SMS/OSS、AuthConstants 等）
│   ├── Core.DataAccess             # 仓储
│   ├── Core.Application            # 业务服务（System + Auth/Init/Tools/UserProfile）
│   └── IdemX.Admin.Api             # API 入口、Controllers、Startup
├── src/frontend/idemX.admin.web/   # Vben monorepo
│   └── apps/web-antd/              # 实际运行的 Ant Design 管理端
└── .cursor/rules/                  # Cursor 开发规范
```

---

## 骨架已包含

### 后端 API

- **鉴权**：密码登录、退出、找回密码（短信）
- **RBAC**：用户 / 角色 / 菜单 / 组织 / 登录与操作日志
- **个人中心**：资料修改、改密、换绑手机、头像上传
- **运维**：`InitAreas` 全量导入省市区（可选，需管理员登录）
- **公共**：Health、Tools（枚举 / 地区等）

### 前端页面

- `_core`：登录、找回密码、404、关于、个人中心
- `system`：用户、角色、菜单、组织、日志（菜单由后端动态下发）

### 可选集成（未配则对应功能不可用）

| 集成 | 用途 | 未配时 |
|---|---|---|
| 阿里云 SMS | 找回密码、换绑手机 | 密码登录仍可用 |
| 阿里云 OSS | 头像上传 | 头像功能不可用 |
| Hangfire | 定时任务 | 默认关闭 |

---

## 本地运行

### 环境要求

- .NET 8 SDK
- Node.js 20+、pnpm 9+
- PostgreSQL 14+
- VS Code / Cursor（可选：用 `fullstack.code-workspace` 打开整个仓库）

### 1. 创建数据库并初始化

```sql
CREATE DATABASE "IdemX.Admin.db";
```

> 库名含 `.`，PostgreSQL 里需加双引号。

```bash
psql -U postgres -d "IdemX.Admin.db" -f scripts/postgresql/schema.sql
psql -U postgres -d "IdemX.Admin.db" -f scripts/postgresql/seed.sql
```

> 增删改实体后，请手工同步 `scripts/postgresql/schema.sql`。  
> `seed.sql` 仅用于**空库首次**执行，重复运行可能主键冲突。

### 2. 配置后端

编辑 `src/backend/IdemX.Admin.Api/appsettings.json`：

```json
"ConnectionStrings": {
  "DbType": "PostgreSQL",
  "SqlConnection": "Host=localhost;Port=5432;Database=IdemX.Admin.db;Username=postgres;Password=123456;..."
},
"Environment": {
  "IsProd": false
}
```

- `AliyunOptions.Sms` / `Oss`：按需填写（见上表）
- `Environment:IsProd` 为 `true` 时关闭 Swagger/ReDoc 并启用 HTTPS 重定向

### 3. 启动后端

```bash
cd src/backend/IdemX.Admin.Api
dotnet run
```

| 地址 | 说明 |
|---|---|
| `http://localhost:5000` | API |
| `http://localhost:5000/api-doc` | Swagger UI |
| `http://localhost:5000/doc` | ReDoc |

### 4. 启动前端

```bash
cd src/frontend/idemX.admin.web
pnpm install
pnpm dev:antd
```

| 地址 | 说明 |
|---|---|
| `http://localhost:5666` | 管理端（见 `apps/web-antd/.env.development`） |
| 接口代理 | `VITE_GLOB_API_URL=http://localhost:5000/api/` |

### 5. 登录与后续操作

1. 浏览器打开前端，使用 `admin` / `123456` 登录
2. 在 **用户管理** 中新建账号并分配角色
3. （可选）管理员登录后调用 `POST /api/Init/InitAreas` 导入省市区

---

## 如何扩展新业务

按现有 **system/user** 模式复制，不必改框架层：

1. **后端**：`Core.Model` 加实体/DTO → `Core.Application` 加 Service → `IdemX.Admin.Api/Controllers` 加 Controller
2. **前端**：`src/api/` 加接口 → `src/views/` 加页面 → `seed.sql` 或菜单管理里配置路由

角色编码等常量与种子保持一致时，见 `Core.Infrastructure/Configuration/AuthConstants.cs` 与 `scripts/postgresql/seed.sql`。

框架层（Auth、RBAC、个人中心）尽量不动；业务逻辑放在各自模块目录。

---

## 已有库升级（从旧骨架迁移）

若库是早期版本建的，按需执行（列不存在则跳过）：

```sql
ALTER TABLE public.sys_user DROP COLUMN IF EXISTS real_name;
ALTER TABLE public.sys_user DROP COLUMN IF EXISTS wechat_no;
ALTER TABLE public.sys_user DROP COLUMN IF EXISTS user_type;

DELETE FROM public.sys_role WHERE role_code = 'registered';
```

全新 fork 建议直接删库重建并重新执行 `schema.sql` + `seed.sql`。

---

## 命名说明

| 名称 | 含义 |
|---|---|
| **IdemX.Admin** | 产品 / 仓库 / API 品牌名 |
| **Core.*** | 后端类库命名空间，历史沿用，与产品名无关 |
| **idemX.admin.web** | 前端 Vben monorepo 目录 |

---

## 开发规范

见 `.cursor/rules/`，用 Cursor 时会自动加载。
