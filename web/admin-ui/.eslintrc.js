module.exports = {
  root: true,
  extends: ['plugin:jest-dom/recommended'],
  plugins: ['testing-library', 'jest-dom'],
  overrides: [
    {
      files: ['**/__tests__/**/*.[jt]s?(x)', '**/?(*.)+(spec).[jt]s?(x)'],
      extends: ['plugin:testing-library/react'],
    },
  ],
};
