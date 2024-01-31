import { yupResolver } from '@hookform/resolvers/yup';
import { useTranslations } from 'next-intl';
import { FormProvider,useForm } from 'react-hook-form';
import * as yup from 'yup';

import { PhoneRegex } from '../lib/regex';
import CheckboxInput from './CheckboxInput';
import TextInput from './TextInput';

export type RegisterData = {
  name: string;
  phone: string;
  terms: NonNullable<boolean | undefined>;
}; // A bit hacky, but it works

export interface RegisterFormProps {
  onSubmit: (data: RegisterData) => void;
}

function RegisterForm({ onSubmit }: RegisterFormProps) {
  const t = useTranslations('Components/RegisterForm');
  const schema = yup
    .object({
      name: yup.string().min(3, t('errors.name')).max(1000).required(),
      phone: yup.string().matches(PhoneRegex, t('errors.phone')).required(),
      terms: yup.boolean().oneOf([true], t('errors.terms')).required(),
    })
    .required();
  const methods = useForm<RegisterData>({
    resolver: yupResolver(schema),
    mode: 'onBlur',
  });
  const {
    handleSubmit,
    formState: { isValid, isSubmitting },
  } = methods;

  return (
    <div data-testid="RegisterForm__container">
      <FormProvider {...methods}>
        <div className="form my-2">
          <form onSubmit={handleSubmit(onSubmit)}>
            <div className="row">
              <TextInput
                data-testid="RegisterForm__name"
                type="text"
                label={t('inputs.name')}
                placeholder={t('inputs.name') as string}
                name="name"
              />

              <TextInput
                data-testid="RegisterForm__phone"
                type="tel"
                label={t('inputs.phone')}
                placeholder={t('inputs.phone') as string}
                name="phone"
                minLength={10}
                maxLength={10}
              />
              <div className="col-12">
                <div className="terms">
                  <CheckboxInput
                    data-testid="RegisterForm__terms"
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
            </div>
            <button
              className="btn btn-lg btn-1"
              data-testid="RegisterForm__submit"
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
    </div>
  );
}

export default RegisterForm;
