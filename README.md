# IdemX.Admin

可复用的 **管理端项目骨架**，从业务系统中剥离出来，供新项目直接 fork / 复制后扩展。

**不是**某个具体业务产品。仓库里不含订单、合约、企业、钱包等业务模块；也不绑定身份证实名、邀请码等业务流程。

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
├── src/backend/                    # 后端（类库层仍叫 Core.*）
│   ├── Core.Model                  # 实体、DTO、枚举
│   ├── Core.Infrastructure         # 基础设施（Options、阿里云 SMS/OSS 等）
│   ├── Core.DataAccess             # 仓储
│   ├── Core.Application            # 业务服务（System + Auth/Init/Tools/UserProfile）
│   └── IdemX.Admin.Api             # API 入口、Controllers、Startup
├── src/frontend/jyd.jcpt.web/      # Vben monorepo（目录名待后续重命名为 web）
│   └── apps/web-antd/              # 实际运行的 Ant Design 管理端
└── .cursor/rules/                  # Cursor 开发规范
```

---

## 骨架已包含

### 后端 API

- **鉴权**：密码登录、短信验证码登录、注册（短信校验）
- **RBAC**：用户 / 角色 / 菜单 / 组织 / 操作日志
- **个人中心**：资料修改、改密、换绑手机、头像上传
- **运维**：Init 冷启动（省市区、默认管理员与系统菜单种子）
- **公共**：Health、Tools（枚举/地区等）

### 前端页面

- `_core`：登录、注册、找回密码、404、关于、个人中心
- `system`：用户、角色、菜单、组织、日志

### 可选集成（配置即用，未配则留空）

- 阿里云 SMS（注册 / 登录 / 换绑手机）
- 阿里云 OSS（头像上传）
- Hangfire（默认关闭，无业务定时任务）

---

## 本地运行

### 环境要求

- .NET 8 SDK
- Node.js 20+、pnpm 9+
- PostgreSQL 14+（本地）

### 1. 创建数据库

```sql
CREATE DATABASE "IdemX.Admin.db";
```

> 库名含 `.`，PostgreSQL 里需加双引号。

### 2. 配置连接串

编辑 `src/backend/IdemX.Admin.Api/appsettings.json`：

```json
"ConnectionStrings": {
  "DbType": "PostgreSQL",
  "SqlConnection": "Host=localhost;Port=5432;Database=IdemX.Admin.db;Username=postgres;Password=123456;..."
}
```

按需填写 `AliyunOptions.Sms` / `Oss`（不配短信时，注册与短信登录不可用，密码登录仍可用）。

默认种子管理员见 `InitConfig` 节（默认 `admin` / `123456`）。

### 3. 启动后端

```bash
cd src/backend/IdemX.Admin.Api
dotnet run
```

- API：`http://localhost:5000`
- Swagger：`http://localhost:5000/swagger`

> 表结构需已存在于库中（SqlSugar 按实体映射，启动时不自动建表）。新库请先导入/迁移 schema，再调用 Init 接口写入种子数据。

### 4. 启动前端

```bash
cd src/frontend/jyd.jcpt.web
pnpm install
pnpm dev:antd
```

- 前端：`http://localhost:5666`（见 `apps/web-antd/.env.development`）
- 接口代理：`VITE_GLOB_API_URL=http://localhost:5000/api/`

### 5. 冷启动（可选）

管理员登录后，在 Swagger 或 Postman 调用：

| 接口 | 说明 |
|---|---|
| `POST /api/Init/InitProject` | 创建默认管理员、角色、系统菜单 |
| `POST /api/Init/InitAreas` | 导入省市区数据 |

---

## 如何扩展新业务

按现有 **system/user** 模式复制即可，不必改框架层：

1. **后端**：`Core.Model` 加实体/DTO → `Core.Application` 加 Service → `IdemX.Admin.Api/Controllers` 加 Controller
2. **前端**：`src/api/` 加接口 → `src/views/` 加页面 → 后台菜单配置路由（或 `InitService` 种子菜单里加项）

框架层（Auth、RBAC、个人中心）尽量不动；业务逻辑放在各自模块目录。

---

## 命名说明

| 名称 | 含义 |
|---|---|
| **IdemX.Admin** | 产品 / 仓库 / API 品牌名 |
| **Core.*** | 后端类库命名空间，历史沿用，与产品名无关 |
| **jyd.jcpt.web** | 前端 monorepo 目录，待重命名为 `web`，不影响使用 |

---

## 开发规范

见 `.cursor/rules/`，用 Cursor 时会自动加载。
