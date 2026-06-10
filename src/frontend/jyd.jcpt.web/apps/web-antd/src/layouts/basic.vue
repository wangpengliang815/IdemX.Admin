<script lang="ts" setup>
import type { NotificationItem } from '@vben/layouts';

import { computed, onBeforeUnmount, onMounted, ref, watch } from 'vue';
import { useRouter } from 'vue-router';

import { AuthenticationLoginExpiredModal } from '@vben/common-ui';
import { useWatermark } from '@vben/hooks';
import { createIconifyIcon } from '@vben/icons';
import { BasicLayout, LockScreen, Notification, UserDropdown } from '@vben/layouts';
import { preferences } from '@vben/preferences';
import { useAccessStore, useUserStore } from '@vben/stores';

import { message, Modal, Popover } from 'ant-design-vue';

import { convertToContractPartnerApi } from '#/api/profile';
// import { $t } from '#/locales';
import { rebuildMenus } from '#/router/guard';
import { useAuthStore, usePartnerContextStore } from '#/store';
import LoginForm from '#/views/_core/authentication/login.vue';

const headerToolbarTargetSelector = 'header .flex.h-full.min-w-0.flex-shrink-0.items-center';
const headerToolbarReady = ref(false);

const PurchaseIcon = createIconifyIcon('mdi:cart-outline');
const ContractIcon = createIconifyIcon('mdi:file-document-outline');
const WebsiteIcon = createIconifyIcon('mdi:web');
const MiniProgramIcon = createIconifyIcon('mdi:qrcode');

const notifications = ref<NotificationItem[]>([]);

const router = useRouter();

function goCompanyWebsite() {
  router.push({ name: 'CompanyWebsite' });
}

const userStore = useUserStore();
const authStore = useAuthStore();
const accessStore = useAccessStore();
const { destroyWatermark, updateWatermark } = useWatermark();
const partnerContextStore = usePartnerContextStore();

const openRoleApplyModal = ref(false);
const agreed = ref(false);
const loading = ref(false);

/** 切换采购/合约视角：写 store、主动重建菜单路由、跳默认首页（合约广场） */
async function applyViewAndReload(mode: 'buyer' | 'cp') {
  if (partnerContextStore.mode === mode) return;
  partnerContextStore.setMode(mode);
  await rebuildMenus(router);
  try {
    await router.replace({ path: preferences.app.defaultHomePath });
  } catch {}
}

const hasContractPartnerRole = computed(() => {
  const u: any = userStore.userInfo;
  const roles = u?.roles || u?.Roles || [];
  if (!Array.isArray(roles) || roles.length === 0) return false;

  return roles.some((r: any) => {
    const code = String(r?.roleCode || r?.code || '').toLowerCase();
    const name = String(r?.roleName || r?.name || '');
    // 后端固定合约伙伴 roleCode=cp
    return code === 'cp' || name.includes('合约伙伴');
  });
});

function handleClickPurchase() {
  void applyViewAndReload('buyer');
}

function handleClickContract() {
  if (hasContractPartnerRole.value) {
    void applyViewAndReload('cp');
    return;
  }
  agreed.value = false;
  openRoleApplyModal.value = true;
}

async function handleAgreeAndApply() {
  if (!agreed.value) {
    message.warning('请先阅读并同意《合约伙伴开通协议》');
    return;
  }
  if (loading.value) return;
  loading.value = true;
  try {
    const res = await convertToContractPartnerApi();
    if (res?.code === 0) {
      message.success(res?.msg || '开通成功');
      await authStore.getUserInfo();
      await applyViewAndReload('cp');
      openRoleApplyModal.value = false;
    } else {
      message.error(res?.msg || '开通失败');
    }
  } catch (error: any) {
    message.error(error?.message || '开通失败');
  } finally {
    loading.value = false;
  }
}

function updateHeaderToolbarReady() {
  headerToolbarReady.value = Boolean(document.querySelector(headerToolbarTargetSelector));
}

let headerObserver: MutationObserver | null = null;
onMounted(() => {
  updateHeaderToolbarReady();
  // BasicLayout 的 header 可能是异步渲染/切换布局后才挂载，使用观察器保证 Teleport 目标存在
  headerObserver = new MutationObserver(() => updateHeaderToolbarReady());
  headerObserver.observe(document.body, { childList: true, subtree: true });
});

onBeforeUnmount(() => {
  headerObserver?.disconnect();
  headerObserver = null;
});

const showDot = computed(() => notifications.value.some((item) => !item.isRead));

const menus = computed(() => [
  // 个人中心（暂不展示）
  // {
  //   handler: () => {
  //     router.push('/profile');
  //   },
  //   icon: 'lucide:user',
  //   text: $t('page.auth.profile'),
  // },
]);

const avatar = computed(() => {
  const raw = userStore.userInfo?.avatar ?? preferences.app.defaultAvatar;
  if (!raw || typeof raw !== 'string' || raw.startsWith('data:') || raw.startsWith('blob:')) {
    return raw;
  }

  if (!raw.startsWith('http://') && !raw.startsWith('https://')) {
    return raw;
  }

  const nonceRaw: any = (authStore as any).avatarNonce;
  const t = typeof nonceRaw === 'number' ? nonceRaw : nonceRaw?.value;
  if (!t) return raw;
  return raw.includes('?') ? `${raw}&t=${t}` : `${raw}?t=${t}`;
});

const isAdminUser = computed(() => {
  const u: any = userStore.userInfo;
  return String(u?.userName || '').toLowerCase() === 'admin';
});

const roleCode = computed(() => {
  const userInfo = userStore.userInfo;
  if (!userInfo?.roles || userInfo.roles.length === 0) return '';
  // 取第一个角色的 roleCode，如果有多个角色可以拼接
  const firstRole = userInfo.roles[0] as any;
  return firstRole?.roleCode || '';
});

async function handleLogout() {
  await authStore.logout(false);
}

function handleNoticeClear() {
  notifications.value = [];
}

function markRead(id: number | string) {
  const item = notifications.value.find((item) => item.id === id);
  if (item) {
    item.isRead = true;
  }
}

function remove(id: number | string) {
  notifications.value = notifications.value.filter((item) => item.id !== id);
}

function handleMakeAll() {
  notifications.value.forEach((item) => (item.isRead = true));
}
watch(
  () => ({
    enable: preferences.app.watermark,
    content: preferences.app.watermarkContent,
  }),
  async ({ enable, content }) => {
    if (enable) {
      await updateWatermark({
        content: content || userStore.userInfo?.userName,
      });
    } else {
      destroyWatermark();
    }
  },
  {
    immediate: true,
  },
);
</script>

<template>
  <BasicLayout @clear-preferences-and-logout="handleLogout">
    <template #user-dropdown>
      <UserDropdown
        :avatar
        :menus
        :text="userStore.userInfo?.userName"
        :description="userStore.userInfo?.email"
        :tag-text="roleCode"
        @logout="handleLogout"
      />
    </template>
    <template #notification>
      <Notification
        :dot="showDot"
        :notifications="notifications"
        @clear="handleNoticeClear"
        @read="(item) => item.id && markRead(item.id)"
        @remove="(item) => item.id && remove(item.id)"
        @make-all="handleMakeAll"
      />
    </template>
    <template #extra>
      <!-- 全局顶部：搜索框左侧“采购伙伴/合约伙伴”切换 -->
      <Teleport v-if="headerToolbarReady" :to="headerToolbarTargetSelector">
        <div class="mr-4 flex h-8 flex-shrink-0 items-center gap-3" style="order: -1">
          <button
            type="button"
            class="inline-flex h-8 items-center gap-1 rounded-2xl border border-border bg-background/70 px-3 text-xs font-medium text-foreground/80 shadow-sm backdrop-blur transition-colors hover:bg-accent hover:text-foreground"
            @click="goCompanyWebsite"
          >
            <WebsiteIcon class="text-[14px]" />
            <span>平台介绍</span>
          </button>
          <Popover :arrow="false" placement="bottomRight" trigger="hover" :overlay-inner-style="{ padding: '0' }">
            <template #content>
              <img alt="伙歌行小程序" class="block h-auto w-[200px] max-w-none" src="/wechat.png" />
            </template>
            <button
              type="button"
              class="inline-flex h-8 items-center gap-1 rounded-2xl border border-border bg-background/70 px-3 text-xs font-medium text-foreground/80 shadow-sm backdrop-blur transition-colors hover:bg-accent hover:text-foreground"
            >
              <MiniProgramIcon class="text-[14px]" />
              <span>访问小程序</span>
            </button>
          </Popover>
          <div v-if="!isAdminUser" class="flex h-8 items-center overflow-hidden rounded-2xl border border-border bg-background/70 shadow-sm backdrop-blur">
            <button
              class="inline-flex h-8 items-center gap-1 rounded-l-2xl px-2 text-xs font-medium transition-colors"
              :class="
                partnerContextStore.isPurchase ? 'bg-primary text-primary-foreground shadow-sm' : 'text-foreground/70 hover:bg-accent hover:text-foreground'
              "
              type="button"
              @click="handleClickPurchase"
            >
              <PurchaseIcon class="text-[14px]" />
              <span>采购方</span>
            </button>
            <button
              class="inline-flex h-8 items-center gap-1 rounded-r-2xl px-2 text-xs font-medium transition-colors"
              :class="
                partnerContextStore.isContract ? 'bg-primary text-primary-foreground shadow-sm' : 'text-foreground/70 hover:bg-accent hover:text-foreground'
              "
              type="button"
              @click="handleClickContract"
            >
              <ContractIcon class="text-[14px]" />
              <span>供货方</span>
            </button>
          </div>
        </div>
      </Teleport>

      <Modal v-model:open="openRoleApplyModal" title="开通合约伙伴" :footer="null" width="800px">
        <div class="flex max-h-[70vh] flex-col">
          <!-- 协议内容区域（可滚动） -->
          <div class="flex-1 space-y-4 overflow-y-auto px-1">
            <!-- 警告提示 -->
            <div class="rounded-lg border border-orange-200 bg-orange-50 px-4 py-3">
              <h4 class="mb-2 text-sm font-semibold text-orange-700">暂未开通合约伙伴权限</h4>
              <p class="text-xs text-orange-600">你正在尝试访问合约相关功能，需要先开通合约伙伴身份后才能使用。</p>
            </div>

            <!-- 重要提醒 -->
            <div class="rounded-lg border border-orange-200 bg-orange-50 px-4 py-3">
              <h4 class="mb-2 text-sm font-semibold text-orange-700">重要提醒</h4>
              <div class="space-y-1 text-xs text-orange-600">
                <p>1) 开通合约伙伴为用户维度能力；公司协议为公司维度能力</p>
                <p>2) 在采购/合约功能选择公司时，会校验该公司对应协议是否已生效</p>
                <p>3) 未生效的公司不可选，可能导致下单/确认等流程无法继续</p>
              </div>
            </div>

            <!-- 协议正文 -->
            <div class="rounded-lg border border-gray-200 bg-white p-4">
              <h3 class="mb-3 text-base font-semibold text-gray-900">一、合约伙伴角色说明</h3>
              <p class="mb-3 text-sm text-gray-700">伙歌行平台合约伙伴分为两种类型：</p>
              <div class="space-y-2 rounded-lg bg-gray-50 p-3">
                <div class="flex items-start gap-2">
                  <span class="text-green-600"></span>
                  <div>
                    <span class="font-medium text-gray-900">种草官：</span>
                    <span class="text-sm text-gray-700">负责挖掘优质商品、创建推广合约</span>
                  </div>
                </div>
                <div class="flex items-start gap-2">
                  <span class="text-yellow-600">🤝</span>
                  <div>
                    <span class="font-medium text-gray-900">商务官：</span>
                    <span class="text-sm text-gray-700">负责合约执行与履约</span>
                  </div>
                </div>
              </div>

              <h3 class="mb-3 mt-5 text-base font-semibold text-gray-900">三、种草官权利义务</h3>
              <p class="mb-3 text-sm text-gray-700">
                作为种草官，申请开通合约伙伴后，伙歌行平台授权您与供货方洽谈合约上架、服务费、合约产品的售前、售中、售后服务等事宜，您的具体权利义务包括：
              </p>
              <div class="space-y-2 rounded-lg bg-gray-50 p-3 text-sm text-gray-700">
                <p>(1) 负责在伙歌行平台授权范围内采用伙歌行平台固定模版和供货方洽谈产品推广合约；</p>
                <p>(2) 负责根据供货方和伙歌行的审核意见修改供货方产品推广合约，对伙歌行平台展示的合约及商品内容进行编辑、修改、提交；</p>
                <p>(3) 负责在产品推广合约经供货方和伙歌行审批通过后，将该合约上架到伙歌行平台销售；</p>
                <p>(4) 负责确保实际商品和合约商品信息一致，并根据实际商品的变更而更新合约相关信息；</p>
                <p>
                  (5)
                  在供货方发生违约、欺诈、侵权等行为时，您亦承担相应的责任，伙歌行平台有权行使包括关闭注册用户账号、扣除劳务打赏、提起诉讼追究赔偿责任等权力；
                </p>
                <p>(6) 种草合约商品对应的订单完成后，即可获得基础服务费的打赏（基础服务费×种草官分配比例×打赏系数）；</p>
                <p>(7) 种草合约商品对应的合约完成后，即可获得达量服务费的打赏（达量服务费×种草官分配比例×打赏系数）。</p>
              </div>

              <h3 class="mb-3 mt-5 text-base font-semibold text-gray-900">四、商务官权利义务</h3>
              <p class="mb-3 text-sm text-gray-700">
                作为商务官，您将以给供货方代表的身份落实合约内容审核、合约订单履约、合约服务费支付、对账，合约订单售前、售中、售后服务等事宜，您的具体权利义务包括：
              </p>
              <div class="space-y-2 rounded-lg bg-gray-50 p-3 text-sm text-gray-700">
                <p>
                  (1) 负责合约及商品的审核，负责平台订单确认、售前咨询服务、售中跟单（发货确认、上传物流信息，物流跟进、对账开票跟进）以及售后服务跟进落实；
                </p>
                <p>(2) 负责合约商品交易各项节点的落实，沟通并解决采购方订单执行；</p>
                <p>(3) 对合约内容及商品信息进行审核，对描述不符的有权驳回并说明理由；</p>
                <p>(4) 实施供货方的价格体系维护，对合约商品价格进行线上或线下巡视，对发生破坏价格体系的事件应当及时向供货方及伙歌行平台反馈并积极干预解决；</p>
                <p>
                  (5)
                  在供货方发生违约、欺诈、侵权等行为时，您亦承担相应的责任，伙歌行平台有权行使包括关闭注册用户账号、扣除劳务打赏、提起诉讼追究赔偿责任等权力；
                </p>
                <p>(6) 所负责合约商品对应订单完成后，即可获得基础服务费的打赏（基础服务费×商务官分配比例×打赏系数）；</p>
                <p>(7) 所负责合约商品对应合约完成后，即可获得达量服务费的打赏（达量服务费×商务官分配比例×打赏系数）。</p>
              </div>

              <h3 class="mb-3 mt-5 text-base font-semibold text-gray-900">六、禁止行为</h3>
              <p class="mb-3 text-sm text-gray-700">合约伙伴严禁从事以下行为：</p>
              <div class="space-y-2 rounded-lg bg-gray-50 p-3 text-sm text-gray-700">
                <p>❌ 发布虚假合约或商品信息</p>
                <p>❌ 进行价格欺诈或不正当竞争</p>
                <p>❌ 收款后不履约或失联</p>
                <p>❌ 侵犯他人知识产权</p>
                <p>❌ 恶意骚扰采购方或平台用户</p>
                <p>❌ 刷单套取平台奖励</p>
                <p>❌ 协助或默许供货方违规</p>
                <p>❌ 违反法律法规</p>
              </div>

              <h3 class="mb-3 mt-5 text-base font-semibold text-gray-900">六、协议确认</h3>
              <p class="mb-3 text-sm text-gray-700">开通合约伙伴即表示您已：</p>
              <div class="space-y-2 rounded-lg bg-gray-50 p-3 text-sm text-gray-700">
                <p>✅ 仔细阅读并理解本须知全部内容</p>
                <p>✅ 同意遵守伙歌行平台各项规则</p>
                <p>✅ 承诺履行相应的权利与义务</p>
                <p>✅ 接受平台监管和争议处理安排</p>
              </div>
            </div>

            <!-- 同意勾选 -->
            <div class="flex items-start gap-2">
              <input id="agree-checkbox" v-model="agreed" type="checkbox" class="mt-1 h-4 w-4 rounded border-gray-300 text-primary focus:ring-primary" />
              <label for="agree-checkbox" class="text-sm text-gray-700"> 我已阅读并同意《合约伙伴开通协议》 </label>
            </div>
          </div>

          <!-- 底部操作按钮 -->
          <div class="flex justify-end gap-3 border-t border-gray-200 pt-4">
            <button
              class="rounded-lg border border-gray-300 bg-white px-5 py-2 text-sm font-medium text-gray-700 hover:bg-gray-50"
              @click="openRoleApplyModal = false"
            >
              取消
            </button>
            <button
              class="rounded-lg bg-primary px-5 py-2 text-sm font-medium text-white hover:bg-primary/90 disabled:cursor-not-allowed disabled:opacity-50"
              :disabled="!agreed || loading"
              @click="handleAgreeAndApply"
            >
              {{ loading ? '开通中...' : '同意并开通合约伙伴' }}
            </button>
          </div>
        </div>
      </Modal>

      <AuthenticationLoginExpiredModal v-model:open="accessStore.loginExpired" :avatar>
        <LoginForm />
      </AuthenticationLoginExpiredModal>
    </template>
    <template #lock-screen>
      <LockScreen :avatar @to-login="handleLogout" />
    </template>
  </BasicLayout>
</template>
