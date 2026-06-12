/**
 * 统一的日志工具
 * 开发环境：输出到控制台
 * 生产环境：静默或发送到日志服务
 */
const isDev = import.meta.env.DEV;

export const logger = {
  /**
   * 错误日志
   */
  error: (...args: any[]) => {
    if (isDev) {
      console.error(...args);
    } else {
      // 生产环境可以发送到日志服务
      // sendToLogService('error', args);
    }
  },

  /**
   * 警告日志
   */
  warn: (...args: any[]) => {
    if (isDev) {
      console.warn(...args);
    } else {
      // 生产环境可以发送到日志服务
      // sendToLogService('warn', args);
    }
  },

  /**
   * 信息日志
   */
  log: (...args: any[]) => {
    if (isDev) {
      console.warn(...args);
    } else {
      // 生产环境可以发送到日志服务
      // sendToLogService('log', args);
    }
  },

  /**
   * 调试日志
   */
  debug: (...args: any[]) => {
    if (isDev) {
      console.warn(...args);
    }
  },
};
