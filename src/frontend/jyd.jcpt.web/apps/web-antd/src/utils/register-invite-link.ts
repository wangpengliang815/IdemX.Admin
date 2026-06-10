import type { RouteLocationRaw, Router } from 'vue-router';

/** 注册页 / 邀请落地页邀请码查询参数名 */
export const REGISTER_INVITE_QUERY_KEY = 'invitationCode';

function trimTrailingSlash(url: string) {
  return url.replace(/\/+$/, '');
}

function ensureLeadingSlash(path: string) {
  return path.startsWith('/') ? path : `/${path}`;
}

/**
 * 邀请链接使用的站点根地址。
 * 优先 `VITE_GLOB_SITE_URL`（生产打包进 _app.config.js），未配置则用当前浏览器地址栏域名。
 */
export function getPublicSiteOrigin(): string {
  const fromEnv = import.meta.env.VITE_GLOB_SITE_URL?.trim();
  const fromRuntime = import.meta.env.PROD ? window._VBEN_ADMIN_PRO_APP_CONF_?.VITE_GLOB_SITE_URL?.trim() : '';
  const configured = fromRuntime || fromEnv;
  if (configured) return trimTrailingSlash(configured);
  return window.location.origin;
}

function buildPublicRouteLink(router: Router, route: RouteLocationRaw) {
  const resolved = router.resolve(route);
  const origin = getPublicSiteOrigin();
  const base = import.meta.env.VITE_BASE || '/';
  const basePath = base === '/' ? '/' : `${ensureLeadingSlash(trimTrailingSlash(base))}/`;

  if (import.meta.env.VITE_ROUTER_HISTORY === 'hash') {
    const hashPath = ensureLeadingSlash(resolved.fullPath);
    return `${origin}${basePath === '/' ? '' : trimTrailingSlash(basePath)}#${hashPath}`;
  }

  return new URL(ensureLeadingSlash(resolved.fullPath), `${origin}${basePath}`).href;
}

/**
 * 生成带邀请码的邀请落地页完整 URL（复制后发给好友打开）
 */
export function buildRegisterInviteLink(router: Router, invitationCode: string) {
  const code = invitationCode.trim().toUpperCase();
  return buildPublicRouteLink(router, {
    name: 'Invite',
    query: { [REGISTER_INVITE_QUERY_KEY]: code },
  });
}
