import { defineOverridesPreferences } from '@vben/preferences';

/**
 * @description 项目配置文件
 * 只需要覆盖项目中的一部分配置，不需要的配置不用覆盖，会自动使用默认配置
 * 更改配置后请清空缓存，否则可能不生效
 */
const logoUrl = `${import.meta.env.BASE_URL}logo.png`;

export const overridesPreferences = defineOverridesPreferences({
  // overrides
  app: {
    name: import.meta.env.VITE_APP_TITLE,
    locale: 'zh-CN', // 设置默认语言为中文
    accessMode: 'backend', // 使用后端动态路由模式
    defaultHomePath: '/home', // 默认首页：合约广场（静态路由 views/home）
    enableRefreshToken: false, // 不使用 Token 刷新
    loginExpiredMode: 'modal', // Token 过期时弹出模态框提示
  },
  theme: {
    mode: 'light', // 设置默认主题为亮色
  },
  logo: {
    source: logoUrl,
    sourceDark: logoUrl,
  },
  widget: {
    languageToggle: false, // 隐藏界面上的语言切换按钮
    timezone: false, // 隐藏时区选择按钮
    globalSearch: false, // 隐藏顶部全局搜索
    notification: false, // 隐藏顶部消息通知
  },
  footer: {
    enable: false,
  },
  copyright: {
    enable: true,
    companyName: '',
    companySiteLink: '',
    date: '',
    icp: '京ICP备2025155896号-1',
    icpLink: 'https://beian.miit.gov.cn/',
    settingShow: false,
    telecomLicense: 'EDI 京B2-20261329',
    telecomLicenseLink: 'https://dxzhgl.miit.gov.cn/',
  },
});
