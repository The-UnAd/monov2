module.exports = {
  root: true,
  extends: ['plugin:jest-dom/recommended'],
  plugins: ['testing-library', 'jest-dom', 'simple-import-sort', 'import'],
  overrides: [
    {
      files: ['**/__tests__/**/*.[jt]s?(x)', '**/?(*.)+(spec).[jt]s?(x)'],
      extends: ['plugin:testing-library/react'],
    },
  ],
  rules: {
    'simple-import-sort/imports': 'error',
    'simple-import-sort/exports': 'error',
  },
};
