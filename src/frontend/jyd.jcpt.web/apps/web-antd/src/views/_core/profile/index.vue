<script setup lang="ts">
import { computed, onMounted, ref } from 'vue';
import { useRouter } from 'vue-router';

import { createIconifyIcon } from '@vben/icons';
import { preferences } from '@vben/preferences';
import { useUserStore } from '@vben/stores';

import { Avatar, Button, Card, Col, message, Row, Space, Statistic, Tag, Upload } from 'ant-design-vue';

import { uploadAvatarApi } from '#/api/profile';
import { useAuthStore } from '#/store/auth';

const userStore = useUserStore();
const authStore = useAuthStore();
const router = useRouter();

const uploading = ref(false);

const AVATAR_SIZE = 72;

const extendedUserInfo = computed(() => {
  const userInfo = userStore.userInfo;
  if (!userInfo) {
    return null;
  }
  return userInfo;
});

const avatarUrl = computed(() => {
  const raw = (extendedUserInfo.value as any)?.avatar || preferences.app.defaultAvatar;

  // base64 / blob 不做处理；仅对 http(s) 的 URL 做缓存穿透
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

const roleTags = computed(() => {
  const u: any = extendedUserInfo.value;
  const roles = u?.roles ?? [];
  return roles
    .map((r: any) => r?.roleName || r?.name || r?.roleCode)
    .filter(Boolean)
    .slice(0, 6);
});

const userTypeLabel = computed(() => {
  const userType = extendedUserInfo.value?.userType;
  if (userType === 0) return '内部用户';
  if (userType === 1) return '注册用户';
  return '';
});

const QuickIconProfile = createIconifyIcon('mdi:account-outline');
const QuickIconSecurity = createIconifyIcon('mdi:shield-check-outline');
const IconVerified = createIconifyIcon('mdi:check-decagram');
const QuickIconPermission = createIconifyIcon('mdi:key-outline');
const QuickIconBank = createIconifyIcon('mdi:credit-card-outline');
const QuickIconPoints = createIconifyIcon('mdi:star-circle-outline');
const QuickIconVip = createIconifyIcon('mdi:crown-outline');
const QuickIconRelease = createIconifyIcon('mdi:clipboard-text-clock-outline');
const IconUserType = createIconifyIcon('mdi:account-outline');
const IconPhone = createIconifyIcon('mdi:phone-outline');
const IconCamera = createIconifyIcon('mdi:camera-outline');
const IconLoading = createIconifyIcon('mdi:loading');

type QuickEntry = {
  badgeText?: string;
  badgeTone?: 'default' | 'info' | 'success' | 'warning';
  desc: string;
  disabled?: boolean;
  icon: any;
  iconBg: string;
  key: string;
  onClick: () => Promise<void> | void;
  title: string;
};

async function go(path: string) {
  await router.push(path);
}

function resolveBadgeColor(tone?: QuickEntry['badgeTone']) {
  return tone === 'success' ? 'success' : 'default';
}

async function handleQuickClick(item: QuickEntry) {
  if (item.disabled) return;
  await item.onClick();
}

const quickEntries = computed<QuickEntry[]>(() => [
  {
    desc: '管理个人基本信息',
    icon: QuickIconProfile,
    iconBg: 'rgba(24, 144, 255, 0.12)',
    key: 'profile',
    onClick: () => go('/profile/basic'),
    title: '个人资料',
  },
  {
    desc: '修改密码、绑定手机',
    icon: QuickIconSecurity,
    iconBg: 'rgba(82, 196, 26, 0.12)',
    key: 'security',
    onClick: () => go('/profile/security'),
    title: '账号安全',
  },
  {
    desc: '余额与积分明细',
    icon: QuickIconPoints,
    iconBg: 'rgba(245, 158, 11, 0.15)',
    key: 'points',
    onClick: () => go('/profile/point'),
    title: '积分中心',
  },
  {
    desc: '余额与流水',
    icon: QuickIconBank,
    iconBg: 'rgba(235, 47, 150, 0.12)',
    key: 'bank',
    onClick: () => go('/profile/wallet'),
    title: '钱包中心',
  },
  {
    desc: '会员等级与权益参数',
    icon: QuickIconVip,
    iconBg: 'rgba(139, 92, 246, 0.15)',
    key: 'vip',
    onClick: () => go('/profile/paid'),
    title: '我的权益',
  },
  {
    desc: '配置公司收货地址',
    icon: createIconifyIcon('mdi:map-marker-radius-outline'),
    iconBg: 'rgba(59, 130, 246, 0.12)',
    key: 'consignee',
    onClick: () => go('/profile/consignees'),
    title: '收货地址',
  },
  {
    desc: '查看合约伙伴开通协议',
    icon: QuickIconPermission,
    iconBg: 'rgba(19, 194, 194, 0.12)',
    key: 'permission',
    onClick: () => go('/profile/cp-partner-agreement'),
    title: '合约伙伴开通须知',
  },
  {
    desc: '查看新功能与问题修复',
    icon: QuickIconRelease,
    iconBg: 'rgba(180, 83, 9, 0.12)',
    key: 'release-log',
    onClick: () => go('/profile/release-log'),
    title: '版本更新日志',
  },
]);

onMounted(async () => {
  await authStore.getUserInfo();
});

function beforeUpload(file: File) {
  const isImage = file.type.startsWith('image/');
  if (!isImage) {
    message.error('只能上传图片文件');
    return false;
  }
  const isLt3M = file.size / 1024 / 1024 < 3;
  if (!isLt3M) {
    message.error('图片大小不能超过 3MB');
    return false;
  }
  return true;
}

async function customRequest(options: Record<string, any>) {
  const { file, onSuccess, onError, onProgress } = options;
  const blob = file instanceof Blob ? file : ((file as unknown as { originFileObj?: Blob }).originFileObj as Blob | undefined);
  if (!blob) {
    message.error('文件无效');
    onError?.(new Error('文件无效'));
    return;
  }

  uploading.value = true;

  try {
    onProgress?.({ percent: 50 });

    const result = await uploadAvatarApi(blob);

    onProgress?.({ percent: 80 });

    if (result.code === 0) {
      onProgress?.({ percent: 100 });

      if (result.msg) {
        message.success(result.msg);
      }
      await authStore.getUserInfo();
      onSuccess?.(result, file);
    } else {
      const errorMsg = result.msg || '头像上传失败';
      message.error(errorMsg);
      onError?.(new Error(errorMsg));
    }
  } catch (error: any) {
    const errorMsg = error?.response?.data?.msg || error?.msg || error?.message;
    if (errorMsg) {
      message.error(errorMsg);
    }
    onError?.(error);
  } finally {
    uploading.value = false;
  }
}

function handleAvatarChange(info: any) {
  const { file } = info;
  if (file.status === 'uploading') {
    uploading.value = true;
  } else if (file.status === 'done' || file.status === 'removed' || file.status === 'error') {
    uploading.value = false;
  }
}
</script>
<template>
  <div class="p-3 md:p-5">
    <Card :bordered="false" :body-style="{ padding: '20px' }" :style="{ borderRadius: '16px' }">
      <Row :gutter="[16, 16]" align="middle">
        <Col :xs="24" :md="18">
          <Space :size="14" align="center">
            <div
              :style="{
                position: 'relative',
                width: `${AVATAR_SIZE}px`,
                height: `${AVATAR_SIZE}px`,
                flex: `0 0 ${AVATAR_SIZE}px`,
              }"
            >
              <Avatar
                :size="AVATAR_SIZE"
                :src="avatarUrl"
                :style="{
                  backgroundColor: '#eef2ff',
                  boxShadow: '0 6px 18px rgba(17, 24, 39, 0.08)',
                }"
              />

              <div :style="{ position: 'absolute', right: '0px', bottom: '0px' }">
                <Upload :before-upload="beforeUpload" :custom-request="customRequest" :show-upload-list="false" accept="image/*" @change="handleAvatarChange">
                  <Button
                    shape="circle"
                    size="middle"
                    type="primary"
                    :disabled="uploading"
                    :style="{
                      width: '30px',
                      height: '30px',
                      minWidth: '30px',
                      fontSize: '12px',
                      boxShadow: '0 4px 12px rgba(24, 144, 255, 0.30)',
                    }"
                  >
                    <template #icon>
                      <component :is="uploading ? IconLoading : IconCamera" :style="{ fontSize: '20px' }" />
                    </template>
                  </Button>
                </Upload>
              </div>
            </div>

            <div style="min-width: 0">
              <Space :size="10" align="baseline">
                <span style="font-size: 20px; line-height: 28px; font-weight: 700; color: #111827">
                  {{ (extendedUserInfo as any)?.userName }}
                </span>
                <Tag color="success" class="mt-[2px]">
                  <span class="inline-flex items-center gap-1">
                    <component :is="IconVerified" class="text-[14px]" />
                    已实名认证
                  </span>
                </Tag>
              </Space>

              <div class="mt-1 flex flex-col gap-1">
                <div v-if="userTypeLabel" style="color: #6b7280; font-size: 13px; line-height: 18px">
                  <Space :size="6" align="center">
                    <component :is="IconUserType" />
                    <span>{{ userTypeLabel }}</span>
                  </Space>
                </div>

                <div v-if="(extendedUserInfo as any)?.phone" style="color: #6b7280; font-size: 13px; line-height: 18px">
                  <Space :size="6" align="center">
                    <component :is="IconPhone" />
                    <span>{{ (extendedUserInfo as any)?.phone }}</span>
                  </Space>
                </div>
              </div>

              <div v-if="roleTags.length > 0" style="margin-top: 10px">
                <Space :size="8" wrap>
                  <Tag v-for="t in roleTags" :key="t">{{ t }}</Tag>
                </Space>
              </div>
            </div>
          </Space>
        </Col>

        <Col :xs="24" :md="6" class="self-start">
          <div class="w-full text-right">
            <Statistic
              title="注册时间"
              :value="String((extendedUserInfo as any)?.createTime ?? '')"
              :value-style="{
                fontSize: '14px',
                fontWeight: 600,
                color: '#111827',
              }"
              style="text-align: right"
            />
          </div>
        </Col>
      </Row>
    </Card>

    <div class="mt-4">
      <Row :gutter="[16, 16]">
        <Col v-for="item in quickEntries" :key="item.key" :xs="24" :sm="12" :md="12" :lg="6" :xl="6">
          <Card
            :bordered="false"
            :hoverable="!item.disabled"
            :style="{
              borderRadius: '16px',
              cursor: item.disabled ? 'not-allowed' : 'pointer',
              opacity: item.disabled ? 0.6 : 1,
            }"
            :body-style="{ padding: '22px' }"
            @click="handleQuickClick(item)"
          >
            <Space :size="14" align="center" style="width: 100%">
              <Avatar
                :size="56"
                shape="square"
                :style="{
                  backgroundColor: item.iconBg,
                  borderRadius: '16px',
                  color: '#111827',
                  display: 'flex',
                  alignItems: 'center',
                  justifyContent: 'center',
                  fontSize: '28px',
                }"
              >
                <component :is="item.icon" />
              </Avatar>

              <div style="flex: 1; min-width: 0">
                <Space :size="10" align="center">
                  <span style="font-size: 16px; font-weight: 700; color: #111827">
                    {{ item.title }}
                  </span>
                  <Tag v-if="item.badgeText" :color="resolveBadgeColor(item.badgeTone)">
                    {{ item.badgeText }}
                  </Tag>
                </Space>
                <div style="margin-top: 6px; color: #6b7280; line-height: 22px">
                  {{ item.desc }}
                </div>
              </div>
            </Space>
          </Card>
        </Col>
      </Row>
    </div>
  </div>
</template>
