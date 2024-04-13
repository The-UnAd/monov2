import { yupResolver } from '@hookform/resolvers/yup';
import { useTranslations } from 'next-intl';
import { FormProvider, useForm } from 'react-hook-form';
import * as yup from 'yup';

import { PhoneRegex } from '../lib/regex';
import TextInput from './TextInput';

export type PhoneNumberData = {
  phone: string;
};

export type PhoneNumberFormProps = Readonly<{
  onSubmit: (data: PhoneNumberData) => void;
}>;

function PhoneNumberForm({ onSubmit }: PhoneNumberFormProps) {
  const t = useTranslations('Components/PhoneNumberForm');
  const schema = yup
    .object({
      phone: yup
        .string()
        .matches(PhoneRegex, t('errors.invalidPhoneNumber'))
        .required(),
    })
    .required();
  const methods = useForm<PhoneNumberData>({
    resolver: yupResolver(schema),
  });
  const {
    handleSubmit,
    formState: { isValid, isSubmitting },
  } = methods;

  return (
    <FormProvider {...methods}>
      <form onSubmit={handleSubmit(onSubmit)}>
        <TextInput
          data-testid="PhoneNumberForm__phone"
          type="tel"
          label={t('inputs.phone.label')}
          placeholder={t('inputs.phone.placeholder')}
          name="phone"
          minLength={10}
          maxLength={10}
        />

        <button
          className="btn btn-lg btn-1"
          data-testid="PhoneNumberForm__submit"
          type="submit"
          disabled={!isValid || isSubmitting}
        >
          {isSubmitting
            ? t('buttons.submit.loading')
            : t('buttons.submit.unpressed')}
        </button>
      </form>
    </FormProvider>
  );
}

export default PhoneNumberForm;
