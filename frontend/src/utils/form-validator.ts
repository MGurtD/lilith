import { ObjectSchema, ValidationError } from "yup";

export class FormValidation {
  constructor(public schema: ObjectSchema<any>) {}

  validate = (formData: any): FormValidationResult => {
    let validation: FormValidationResult = {
      result: false,
      errors: {},
    };

    try {
      this.schema.validateSync(formData, { abortEarly: false });
      validation.result = true;
    } catch (err: any) {
      validation.errors = err.inner.reduce(
        (acc: Errors, cv: ValidationError) => {
          let { path, message } = cv;

          if (acc[path as string] === undefined) acc[path as string] = [];

          acc[path as string].push(message);
          return acc;
        },
        {}
      );
    }

    return validation;
  };
}

export interface FormValidationResult {
  result: boolean;
  errors: Errors;
}

interface Errors {
  [k: string]: Array<string>;
}
