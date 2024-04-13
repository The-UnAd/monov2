import { yupResolver } from '@hookform/resolvers/yup';
import { useTranslations } from 'next-intl';
import { FormProvider, useForm } from 'react-hook-form';
import * as yup from 'yup';

import { PhoneRegex } from '../lib/regex';
import CheckboxInput from './CheckboxInput';
import TextInput from './TextInput';

export type SubscribeData = {
  phone: string;
  terms: NonNullable<boolean | undefined>;
}; // A bit hacky, but it works

type TFunc = ReturnType<typeof useTranslations<''>>;

export type SubscribeFormProps = Readonly<{
  onSubmit: (data: SubscribeData) => void;
  tFunc: TFunc;
}>;

function SubscribeForm({ onSubmit, tFunc: t }: SubscribeFormProps) {
  const schema = yup
    .object({
      phone: yup.string().matches(PhoneRegex, t('errors.phone')).required(),
      terms: yup.boolean().oneOf([true], t('errors.terms')).required(),
    })
    .required();
  const methods = useForm<SubscribeData>({
    resolver: yupResolver(schema),
    mode: 'onBlur',
  });
  const {
    handleSubmit,
    formState: { isValid, isSubmitting },
  } = methods;

  return (
    <FormProvider {...methods}>
      <div className="form my-2">
        <form onSubmit={handleSubmit(onSubmit)}>
          <TextInput
            data-testid="SubscribeForm__phone"
            type="tel"
            label={t('inputs.phone.label')}
            placeholder={t('inputs.phone.placeholder')}
            name="phone"
            minLength={10}
            maxLength={10}
          />
          <div className="col-12">
            <div className="terms">
              <CheckboxInput
                data-testid="SubscribeForm__terms"
                name="terms"
                className="form-check-input"
                type="checkbox"
                labelText={t.rich('terms.disclaimer', {
                  link: (text) => (
                    <a
                      key="0"
                      className="links link-4"
                      href={'https://theunad.com/docs/tos'}
                      target="_blank"
                      rel="noopener noreferrer"
                    >
                      {text}
                    </a>
                  ),
                })}
              />
            </div>
          </div>
          <button
            className="btn btn-lg btn-1"
            data-testid="SubscribeForm__submit"
            type="submit"
            disabled={!isValid || isSubmitting}
          >
            {isSubmitting
              ? t('buttons.submit.loading')
              : t('buttons.submit.unpressed')}
          </button>
        </form>
        <p className="disclaimer">
          {t.rich('privacy.disclaimer', {
            link: (text) => (
              <a
                key="0"
                className="links link-4"
                href={'https://theunad.com/docs/privacy'}
                target="_blank"
                rel="noopener noreferrer"
              >
                {text}
              </a>
            ),
          })}
        </p>
      </div>
    </FormProvider>
  );
}

export default SubscribeForm;
