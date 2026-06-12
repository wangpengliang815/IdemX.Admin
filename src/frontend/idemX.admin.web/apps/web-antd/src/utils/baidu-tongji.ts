declare global {
  interface Window {
    _hmt?: unknown[];
  }
}

const siteId = import.meta.env.VITE_BAIDU_TONGJI_ID?.trim() ?? '';

/** 单页应用路由切换后手动上报 PV（hm.js 由 index.html 官方脚本注入） */
export function trackBaiduPageview() {
  if (!import.meta.env.PROD || !siteId) return;

  window._hmt = window._hmt || [];
  const page =
    import.meta.env.VITE_ROUTER_HISTORY === 'hash'
      ? `${window.location.pathname}${window.location.search}${window.location.hash}`
      : `${window.location.pathname}${window.location.search}`;
  window._hmt.push(['_trackPageview', page]);
}
