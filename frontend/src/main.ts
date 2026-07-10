import { App as VueApp, createApp } from "vue";
import { createPinia } from "pinia";
import App from "./App.vue";
import { i18n } from "./i18n";
import router from "./router";

const pinia = createPinia();
const app: VueApp<Element> = createApp(App).use(router).use(pinia).use(i18n);

import "primeicons/primeicons.css";
import "primeflex/primeflex.css";

// PrimeVue v4 theme configuration
import Lara from "@primeuix/themes/lara";

import PrimeVue from "primevue/config";
import { useBrandingStore } from "@/store/branding";
import { applyBrandingPreset } from "@/config/branding-presets";
import { getActiveEnterpriseId } from "@/composables/useActiveEnterprise";
import Tooltip from "primevue/tooltip";
import ToastService from "primevue/toastservice";
import Toast from "primevue/toast";
import Button from "primevue/button";
import Card from "primevue/card";
import Chart from "primevue/chart";

import DataTable from "primevue/datatable";
import Column from "primevue/column";
import Dialog from "primevue/dialog";
import InputText from "primevue/inputtext";
import InputNumber from "primevue/inputnumber";
import Checkbox from "primevue/checkbox";
import ConfirmDialog from "primevue/confirmdialog";

import Textarea from "primevue/textarea";
import RadioButton from "primevue/radiobutton";
import ColorPicker from "primevue/colorpicker";
import SplitButton from "primevue/splitbutton";
import ConfirmPopup from "primevue/confirmpopup";
import BaseInput from "./components/BaseInput.vue";
import BooleanColumn from "./components/tables/BooleanColumn.vue";
import InfoPanel from "./components/InfoPanel.vue";
import ConfirmationService from "primevue/confirmationservice";

import TreeTable from "primevue/treetable";
import Tag from "primevue/tag";
import Panel from "primevue/panel";
import catalan from "./i18n/primevue/catalan";
import Badge from "primevue/badge";
import InputGroupAddon from "primevue/inputgroupaddon";
import InputGroup from "primevue/inputgroup";
import InputIcon from "primevue/inputicon";
import IconField from "primevue/iconfield";
import MultiSelect from "primevue/multiselect";
// PrimeVue v4 renamed/new components
import Select from "primevue/select";
import DatePicker from "primevue/datepicker";
import Popover from "primevue/popover";
import Drawer from "primevue/drawer";
import Tabs from "primevue/tabs";
import TabList from "primevue/tablist";
import Tab from "primevue/tab";
import TabPanels from "primevue/tabpanels";
import TabPanel from "primevue/tabpanel";
import ProgressBar from "primevue/progressbar";
import { globalToast } from "@/utils/global-toast";

// Base preset as fallback — will be overridden by branding at boot
app.use(PrimeVue, {
  locale: catalan,
  theme: {
    preset: Lara,
    options: {
      darkModeSelector: false,
      cssLayer: false,
    },
  },
});
app.use(ToastService);
app.use(ConfirmationService);

// Initialize global toast service for axios interceptor
globalToast.init(app);

app.directive("tooltip", Tooltip);

app
  .component("Toast", Toast)
  .component("Button", Button)
  .component("SplitButton", SplitButton)
  .component("Card", Card)
  .component("Chart", Chart)
  .component("DataTable", DataTable)
  .component("Column", Column)
  .component("Dialog", Dialog)
  .component("ConfirmPopup", ConfirmPopup)
  .component("InputText", InputText)
  .component("InputNumber", InputNumber)
  .component("Checkbox", Checkbox)
  .component("ConfirmDialog", ConfirmDialog)
  .component("Textarea", Textarea)
  .component("RadioButton", RadioButton)
  .component("BaseInput", BaseInput)
  .component("BooleanColumn", BooleanColumn)
  .component("InfoPanel", InfoPanel)
  .component("ColorPicker", ColorPicker)
  .component("TreeTable", TreeTable)
  .component("Tag", Tag)
  .component("Badge", Badge)
  .component("Panel", Panel)
  .component("InputGroup", InputGroup)
  .component("InputGroupAddon", InputGroupAddon)
  .component("InputIcon", InputIcon)
  .component("IconField", IconField)
  .component("MultiSelect", MultiSelect)
  // PrimeVue v4 renamed components (new names)
  .component("Select", Select)
  .component("DatePicker", DatePicker)
  .component("Popover", Popover)
  .component("Drawer", Drawer)
  // PrimeVue v4 new Tabs components
  .component("Tabs", Tabs)
  .component("TabList", TabList)
  .component("Tab", Tab)
  .component("TabPanels", TabPanels)
  .component("TabPanel", TabPanel)
  .component("ProgressBar", ProgressBar);

app.mount("#app");

// Load branding before mount is impossible (Pinia is needed), so we mount
// with the base Lara preset and then apply the dynamic preset as soon as
// branding data is resolved. This avoids a hard flash because PrimeVue v4
// re-applies theme tokens when `theme:change` is emitted (the
// `applyBrandingPreset` helper triggers that via `usePreset` /
// `updatePrimaryPalette`).
(async () => {
  try {
    const enterpriseId = await getActiveEnterpriseId();
    if (enterpriseId) {
      const brandingStore = useBrandingStore();
      await brandingStore.load(enterpriseId);
      applyBrandingPreset(brandingStore.branding);
    }
  } catch (err) {
    // Branding is non-critical — fall back to Lara + "Lilith" defaults.
    console.warn("[Branding] No s'ha pogut carregar el branding:", err);
  }
})();
