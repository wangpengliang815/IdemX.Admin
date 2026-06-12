<script lang="ts" setup>
interface Props {
  companyName?: string;
  companySiteLink?: string;
  date?: string;
  icp?: string;
  icpLink?: string;
  telecomLicense?: string;
  telecomLicenseLink?: string;
}

defineOptions({
  name: 'Copyright',
});

withDefaults(defineProps<Props>(), {
  companyName: 'Vben Admin',
  companySiteLink: '',
  date: '2024',
  icp: '',
  icpLink: '',
  telecomLicense: '',
  telecomLicenseLink: '',
});
</script>

<template>
  <div class="text-md flex-center">
    <!-- ICP Link -->
    <a
      v-if="icp"
      :href="icpLink || 'javascript:void(0)'"
      class="hover:text-primary-hover mx-1"
      target="_blank"
    >
      {{ icp }}
    </a>

    <a
      v-if="telecomLicense"
      :href="telecomLicenseLink || 'javascript:void(0)'"
      class="hover:text-primary-hover mx-1"
      target="_blank"
    >
      {{ telecomLicense }}
    </a>

    <!-- Copyright Text -->
    <template v-if="date || companyName">
      <span v-if="date">Copyright © {{ date }}</span>

      <!-- Company Link -->
      <a
        v-if="companyName"
        :href="companySiteLink || 'javascript:void(0)'"
        class="hover:text-primary-hover mx-1"
        target="_blank"
      >
        {{ companyName }}
      </a>
    </template>
  </div>
</template>
