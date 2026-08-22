import { AbstractControl, ValidationErrors, ValidatorFn } from '@angular/forms';

export function passwordMatchValidator(
  passwordControlName: string,
  confirmControlName: string,
): ValidatorFn {
  return (group: AbstractControl): ValidationErrors | null => {
    const password = group.get(passwordControlName);
    const confirmPassword = group.get(confirmControlName);

    if (!password || !confirmPassword) {
      return null;
    }

    if (confirmPassword.value !== password.value) {
      confirmPassword.setErrors({ ...confirmPassword.errors, mismatch: true });
      return { mismatch: true };
    }

    if (confirmPassword.hasError('mismatch')) {
      const { mismatch, ...rest } = confirmPassword.errors ?? {};
      confirmPassword.setErrors(Object.keys(rest).length ? rest : null);
    }

    return null;
  };
}
