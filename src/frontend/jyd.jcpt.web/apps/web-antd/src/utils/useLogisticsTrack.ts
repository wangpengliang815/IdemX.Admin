import type { Ref } from 'vue';

import type { LogisticsTrackResp } from '#/api/public/tools';

import { computed, ref, watch } from 'vue';

import { getLogisticsTrackApi, isValidLogisticsCompanyCode, isValidLogisticsTrackingNo } from '#/api/public/tools';

export interface LogisticsTrackSource {
  logisticsCompanyCode?: string;
  logisticsNo?: string;
  recipientPhone?: string;
}

export function useLogisticsTrack(order: Ref<LogisticsTrackSource | null | undefined>) {
  const loading = ref(false);
  const trackData = ref<LogisticsTrackResp | null>(null);
  const loadError = ref('');

  const canQuery = computed(() => {
    const code = order.value?.logisticsCompanyCode?.trim();
    const no = order.value?.logisticsNo?.trim();
    return Boolean(code && no && isValidLogisticsCompanyCode(code) && isValidLogisticsTrackingNo(no));
  });

  const hasLogisticsFields = computed(() => Boolean(order.value?.logisticsCompanyCode?.trim() && order.value?.logisticsNo?.trim()));

  const invalidLogisticsHint = computed(() => {
    if (!hasLogisticsFields.value) return '';
    return '物流单号或快递公司编码格式异常，无法查询（历史脏数据请核对单号与编码）';
  });

  async function loadTrack() {
    if (!canQuery.value) {
      trackData.value = null;
      loadError.value = '';
      return;
    }

    const code = order.value?.logisticsCompanyCode?.trim();
    const no = order.value?.logisticsNo?.trim();
    if (!code || !no) return;

    loading.value = true;
    loadError.value = '';
    try {
      const res = await getLogisticsTrackApi({
        companyCode: code,
        trackingNo: no,
        phone: order.value?.recipientPhone?.trim() || undefined,
      });
      if (res.code !== 0) {
        trackData.value = null;
        loadError.value = res.msg;
        return;
      }
      if (!res.data) {
        trackData.value = null;
        loadError.value = res.msg;
        return;
      }
      trackData.value = res.data;
    } catch (error) {
      trackData.value = null;
      loadError.value = error instanceof Error ? error.message : '查询物流轨迹失败';
    } finally {
      loading.value = false;
    }
  }

  watch(
    () => [order.value?.logisticsCompanyCode, order.value?.logisticsNo, order.value?.recipientPhone] as const,
    () => {
      trackData.value = null;
      loadError.value = '';
      if (canQuery.value) void loadTrack();
    },
    { immediate: true },
  );

  return { loading, trackData, loadError, canQuery, hasLogisticsFields, invalidLogisticsHint, loadTrack };
}
