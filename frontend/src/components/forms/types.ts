import type { FormProps } from "@primevue/forms/form";
import type { CheckboxProps } from "primevue/checkbox";
import type { DatePickerProps } from "primevue/datepicker";
import type { InputNumberProps } from "primevue/inputnumber";
import type { InputTextProps } from "primevue/inputtext";
import type { MultiSelectProps } from "primevue/multiselect";
import type { PasswordProps } from "primevue/password";
import type { SelectProps } from "primevue/select";
import type { TextareaProps } from "primevue/textarea";
import type { AnySchema } from "yup";

export enum FormFieldType {
  Text = "text",
  Password = "password",
  Number = "number",
  Currency = "currency",
  Textarea = "textarea",
  Select = "select",
  MultiSelect = "multiSelect",
  Checkbox = "checkbox",
  Date = "date",
  Custom = "custom",
}

export interface FormGridColumns {
  mobile?: number;
  desktop?: number;
}

export interface FormFieldSpan {
  mobile?: number;
  desktop?: number;
}

type ManagedControlProp =
  | "aria-describedby"
  | "aria-invalid"
  | "class"
  | "defaultValue"
  | "disabled"
  | "id"
  | "invalid"
  | "modelValue"
  | "name"
  | "onUpdate:modelValue";

export type FormControlProps<TProps> = Omit<TProps, ManagedControlProp>;

interface BaseFormFieldConfig<TType extends FormFieldType, TProps> {
  name: string;
  label: string;
  type: TType;
  defaultValue?: unknown;
  validation?: AnySchema;
  props?: TProps;
  disabled?: boolean;
  span?: FormFieldSpan;
}

export type FormFieldConfig =
  | BaseFormFieldConfig<FormFieldType.Text, FormControlProps<InputTextProps>>
  | BaseFormFieldConfig<FormFieldType.Password, FormControlProps<PasswordProps>>
  | BaseFormFieldConfig<
      FormFieldType.Number,
      FormControlProps<InputNumberProps>
    >
  | BaseFormFieldConfig<
      FormFieldType.Currency,
      FormControlProps<InputNumberProps>
    >
  | BaseFormFieldConfig<FormFieldType.Textarea, FormControlProps<TextareaProps>>
  | BaseFormFieldConfig<FormFieldType.Select, FormControlProps<SelectProps>>
  | BaseFormFieldConfig<
      FormFieldType.MultiSelect,
      FormControlProps<MultiSelectProps>
    >
  | BaseFormFieldConfig<FormFieldType.Checkbox, FormControlProps<CheckboxProps>>
  | BaseFormFieldConfig<FormFieldType.Date, FormControlProps<DatePickerProps>>
  | BaseFormFieldConfig<FormFieldType.Custom, Record<string, unknown>>;

export interface FormRowConfig {
  fields: FormFieldConfig[];
  columns?: FormGridColumns;
}

export type FormValues = Record<string, unknown>;
export type FormResolver = NonNullable<FormProps["resolver"]>;
