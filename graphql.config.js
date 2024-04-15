module.exports = {
  projects: {
    adminUi: {
      schema: [
        "web/admin-ui/schema.graphql",
        "web/admin-ui/relay-compiler-directives-v10.0.1.graphql",
      ],
      documents: ["web/admin-ui/src/**/*.{graphql,js,ts,jsx,tsx}"],
    },
  },
};
