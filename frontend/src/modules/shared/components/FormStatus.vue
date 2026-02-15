<template>
  <form>
    <section class="three-columns">
      <BaseInput
        class="mb-2"
        label="Nom"
        v-model="status.name"
        :class="{
          'p-invalid': validation.errors.baseAmount,
        }"
      ></BaseInput>
      <div class="mb-4">
        <label class="block text-900 mb-2">Color</label>
        <Select
          v-model="status.color"
          :options="colors"
          optionValue="id"
          optionLabel="value"
          class="w-full"
          :class="{
            'p-invalid': validation.errors.statusId,
          }"
        />
      </div>
      <div class="mb-4">
        <label class="block text-900 mb-2">Deshabilitat</label>
        <Checkbox v-model="status.disabled" :binary="true" />
      </div>
    </section>

    <section v-if="availableTags.length > 0">
      <div class="mb-4">
        <label class="block text-900 mb-2">Etiquetes</label>
        <MultiSelect
          v-model="selectedTagIds"
          :options="availableTags"
          optionValue="id"
          optionLabel="name"
          placeholder="Selecciona etiquetes"
          class="w-full"
          display="chip"
        >
          <template #option="slotProps">
            <div class="flex align-items-center">
              <i
                v-if="slotProps.option.icon"
                :class="slotProps.option.icon"
                class="mr-2"
              ></i>
              <Tag
                v-if="slotProps.option.color"
                :severity="slotProps.option.color as any"
                class="mr-2"
              >
                {{ slotProps.option.name }}
              </Tag>
              <span v-else>{{ slotProps.option.name }}</span>
            </div>
          </template>
        </MultiSelect>
      </div>

      <div
        v-if="status.lifecycleTags && status.lifecycleTags.length > 0"
        class="mb-4"
      >
        <label class="block text-900 mb-2">Etiquetes assignades</label>
        <div class="flex gap-2 flex-wrap">
          <Tag
            v-for="tag in status.lifecycleTags"
            :key="tag.id"
            :severity="tag.color as any"
          >
            <i v-if="tag.icon" :class="tag.icon" class="mr-1"></i>
            {{ tag.name }}
          </Tag>
        </div>
      </div>
    </section>

    <Button label="Confirmar" @click="submitForm" style="float: right" />
  </form>
</template>

<script setup lang="ts">
import { ref, onMounted } from "vue";
import { Status, LifecycleTag } from "../types";
import * as Yup from "yup";
import {
  FormValidation,
  FormValidationResult,
} from "../../../utils/form-validator";
import { useToast } from "primevue/usetoast";
import { FormActionMode } from "../../../types/component";
import SharedServices from "../services";

const toast = useToast();

const props = defineProps<{
  formAction: FormActionMode;
  status: Status;
}>();
const emit = defineEmits<{
  (
    e: "submit",
    status: Status,
    tagChanges: { assign: string[]; remove: string[] },
  ): void;
}>();

const availableTags = ref<LifecycleTag[]>([]);
const selectedTagIds = ref<string[]>([]);
const initialTagIds = ref<string[]>([]);

onMounted(async () => {
  // Load tags associated with the status
  const statusTags = await SharedServices.Lifecycle.getTagsByStatus(
    props.status.id,
  );
  if (statusTags) {
    props.status.lifecycleTags = statusTags;
  }

  // Load available tags from lifecycle
  if (props.status.lifecycleId) {
    const tags = await SharedServices.Lifecycle.getTagsByLifecycle(
      props.status.lifecycleId,
    );
    if (tags) {
      availableTags.value = tags;
    }
  }

  // Initialize selected tags
  console.log("Initializing selected tags:", props.status.lifecycleTags);
  if (props.status.lifecycleTags) {
    selectedTagIds.value = props.status.lifecycleTags.map((t) => t.id);
    initialTagIds.value = [...selectedTagIds.value];
  }
});

const schema = Yup.object().shape({
  name: Yup.string().required("El nom és obligatori"),
});
const validation = ref({
  result: false,
  errors: {},
} as FormValidationResult);

const validate = () => {
  const formValidation = new FormValidation(schema);
  validation.value = formValidation.validate(props.status);
};

const submitForm = async () => {
  validate();
  if (validation.value.result) {
    // Calculate tag changes
    const tagsToAssign = selectedTagIds.value.filter(
      (id) => !initialTagIds.value.includes(id),
    );
    const tagsToRemove = initialTagIds.value.filter(
      (id) => !selectedTagIds.value.includes(id),
    );

    emit("submit", props.status, {
      assign: tagsToAssign,
      remove: tagsToRemove,
    });
  } else {
    let errors = "";
    Object.entries(validation.value.errors).forEach((e) => {
      errors += `${e[1].map((e) => e)}.   `;
    });
    toast.add({
      severity: "warn",
      summary: "Formulari invàlid",
      detail: errors,
      life: 5000,
    });
  }
};

type Color = {
  id: string;
  value: string;
};

const colors: Color[] = [
  { id: "", value: "Cap" },
  { id: "info", value: "Blau" },
  { id: "secondary", value: "Gris" },
  { id: "help", value: "Lila" },
  { id: "contrast", value: "Negre" },
  { id: "warn", value: "Taronja" },
  { id: "success", value: "Verd" },
  { id: "danger", value: "Vermell" },
];
</script>
