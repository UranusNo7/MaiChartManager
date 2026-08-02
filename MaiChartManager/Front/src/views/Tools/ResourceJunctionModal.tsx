import api from '@/client/api';
import { ResourceJunctionItem, ResourceJunctionStatus } from '@/client/apiGen';
import { Button, Modal, addToast, showTransactionalDialog } from '@munet/ui';
import { computed, defineComponent, ref } from 'vue';
import { useI18n } from 'vue-i18n';

export default defineComponent({
  setup(_, { expose }) {
    const { t } = useI18n();
    const show = ref(false);
    const loading = ref(false);
    const items = ref<ResourceJunctionItem[]>([]);

    const canCreate = computed(() => items.value.some(item => item.status === 'Ready'));
    const canRemove = computed(() => items.value.some(item => item.status === 'AlreadyLinked'));

    const request = async (action: 'status' | 'create' | 'remove') => {
      loading.value = true;
      try {
        const params = action === 'status'
          ? undefined
          : { headers: { 'X-MCM-Local-Action': 'resource-junction' } };
        const response = action === 'status'
          ? await api.GetResourceJunctionStatus()
          : action === 'create'
            ? await api.CreateResourceJunctions(params)
            : await api.RemoveResourceJunctions(params);
        items.value = response.data;
      } catch (error) {
        console.error(error);
        addToast({ message: t('tools.resourceJunction.requestFailed'), type: 'error' });
      } finally {
        loading.value = false;
      }
    };

    const refresh = () => request('status');

    const run = async (action: 'create' | 'remove') => {
      const removing = action === 'remove';
      const confirmed = await showTransactionalDialog(
        t('common.confirm'),
        t(removing ? 'tools.resourceJunction.removeConfirm' : 'tools.resourceJunction.createConfirm'),
        [
          { text: t('common.confirm'), action: true },
          { text: t('common.cancel'), action: false },
        ],
      );
      if (!confirmed) return;
      await request(action);
    };

    const trigger = () => {
      show.value = true;
      refresh();
    };
    expose({ trigger });

    const statusClass = (status?: ResourceJunctionStatus) => {
      if (['Created', 'AlreadyLinked', 'Removed'].includes(status)) return 'text-green-700';
      if (status === 'Ready') return 'text-blue-700';
      return 'text-red-700';
    };

    return () => (
      <Modal
        width="min(92vw,52em)"
        title={t('tools.resourceJunction.title')}
        v-model:show={show.value}
      >
        <div class="flex flex-col gap-4">
          <div class="grid gap-2 text-sm">
            <div>
              <div class="font-medium">{t('tools.resourceJunction.source')}</div>
              <div class="break-all op-65">{items.value[0]?.source?.replace(/\\[^\\]+$/, '')}</div>
            </div>
            <div>
              <div class="font-medium">{t('tools.resourceJunction.target')}</div>
              <div class="break-all op-65">{items.value[0]?.target?.replace(/\\[^\\]+$/, '')}</div>
            </div>
          </div>

          <div class="border border-solid border-gray-200 rounded-md overflow-hidden">
            {items.value.map((item, index) => (
              <div
                key={item.name}
                class={[
                  'grid grid-cols-[minmax(0,1fr)_auto] gap-3 px-4 py-3 items-center',
                  index > 0 && 'border-t border-t-solid border-t-gray-200',
                ]}
              >
                <div class="min-w-0">
                  <div class="font-medium break-all">{item.name}</div>
                  {item.detail && <div class="text-xs op-60 break-all mt-1">{item.detail}</div>}
                </div>
                <div class={['text-sm font-medium whitespace-nowrap', statusClass(item.status)]}>
                  {t(`tools.resourceJunction.status.${item.status}`)}
                </div>
              </div>
            ))}
            {!items.value.length && (
              <div class="px-4 py-6 text-center op-60">{t('tools.resourceJunction.loading')}</div>
            )}
          </div>

          <div class="flex flex-wrap justify-end gap-2">
            <Button onClick={refresh} ing={loading.value}>
              <span class="i-mdi-refresh text-5" />
              {t('tools.resourceJunction.refresh')}
            </Button>
            <Button
              danger
              variant="secondary"
              disabled={!canRemove.value || loading.value}
              onClick={() => run('remove')}
            >
              <span class="i-mdi-link-off text-5" />
              {t('tools.resourceJunction.remove')}
            </Button>
            <Button
              variant="primary"
              disabled={!canCreate.value || loading.value}
              onClick={() => run('create')}
            >
              <span class="i-mdi-link-variant-plus text-5" />
              {t('tools.resourceJunction.create')}
            </Button>
          </div>
        </div>
      </Modal>
    );
  },
});
