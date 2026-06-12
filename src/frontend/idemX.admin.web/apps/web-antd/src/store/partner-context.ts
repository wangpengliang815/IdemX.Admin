import { computed, ref } from 'vue';

import { defineStore } from 'pinia';

export type PartnerContextMode = 'buyer' | 'cp';
const PARTNER_CONTEXT_MODE_KEY = 'partner-context-mode';

function getInitialMode(): PartnerContextMode {
  const raw = localStorage.getItem(PARTNER_CONTEXT_MODE_KEY);
  return raw === 'cp' ? 'cp' : 'buyer';
}

/**
 * 顶部“采购伙伴/合约伙伴”全局切换状态
 */
export const usePartnerContextStore = defineStore('partner-context', () => {
  const mode = ref<PartnerContextMode>(getInitialMode());

  const isPurchase = computed(() => mode.value === 'buyer');
  const isContract = computed(() => mode.value === 'cp');

  function setMode(next: PartnerContextMode) {
    mode.value = next;
    localStorage.setItem(PARTNER_CONTEXT_MODE_KEY, next);
  }

  /** 退出登录时被 resetAllStores 调用，setup 语法需手动实现 */
  function $reset() {
    mode.value = 'buyer';
    localStorage.removeItem(PARTNER_CONTEXT_MODE_KEY);
  }

  return {
    $reset,
    isContract,
    isPurchase,
    mode,
    setMode,
  };
});
