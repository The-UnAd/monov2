module.exports = {
  root: true,
  plugins: ['simple-import-sort', 'import', 'prettier'],
  parser: '@typescript-eslint/parser',
  parserOptions: {
    ecmaVersion: '2015',
    sourceType: 'module',
    project: './tsconfig.json',
    tsconfigRootDir: './',
  },
  rules: {
    'simple-import-sort/imports': 'error',
    'simple-import-sort/exports': 'error',
  },
};
