export interface ReleaseNote {
  date: string;
  title: string;
  features: string[];
  fixes: string[];
}

/** 骨架项目版本说明（按需维护） */
export function listReleaseNotes(): ReleaseNote[] {
  return [
    {
      date: '2026-06-12',
      title: 'IdemX.Admin 骨架',
      features: ['系统管理：用户、角色、菜单、组织、日志', '鉴权与个人中心', 'Init 冷启动与 RBAC 种子菜单'],
      fixes: [],
    },
  ];
}
