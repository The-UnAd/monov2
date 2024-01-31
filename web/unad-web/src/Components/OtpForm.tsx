import { yupResolver } from '@hookform/resolvers/yup';
import { useTranslations } from 'next-intl';
import { FormProvider, useForm } from 'react-hook-form';
import * as yup from 'yup';

import TextInput from './TextInput';

const schema = yup
  .object({
    otp: yup.string().length(6).required(),
  })
  .required();
export type OtpFormData = yup.InferType<typeof schema>;

export interface OtpFormProps {
  onSubmit: (data: OtpFormData) => void;
}

function OtpForm({ onSubmit }: OtpFormProps) {
  const t = useTranslations('Components/OtpForm');
  const methods = useForm<OtpFormData>({
    resolver: yupResolver(schema),
    mode: 'onBlur',
  });

  const {
    handleSubmit,
    formState: { isValid, isSubmitting },
  } = methods;

  return (
    <div className="form my-2" data-testid="OtpForm__container">
      <div className="mb-4">
        <p>{t('instructions')}</p>
      </div>
      <FormProvider {...methods}>
        <form onSubmit={handleSubmit(onSubmit)}>
          <TextInput
            data-testid="OtpForm__otp"
            type="tel"
            label={t('inputs.otp.label')}
            name="otp"
            placeholder={t('inputs.otp.placeholder') as string}
            minLength={6}
            maxLength={6}
          />
          <button
            data-testid="OtpForm__submit"
            type="submit"
            className="btn btn-lg btn-1"
            disabled={!isValid || isSubmitting}
          >
            {isSubmitting
              ? t('buttons.submit.loading')
              : t('buttons.submit.unpressed')}
          </button>
        </form>
      </FormProvider>
    </div>
  );
}

export default OtpForm;
