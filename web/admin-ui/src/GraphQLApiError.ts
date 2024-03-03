export default class GraphQLApiError extends Error {
  constructor(private errors: Error[]) {
    const message = errors.map((e, i) => `${i + 1}) ${e.message}`).join(" ");
    super(message);
    this.name = this.constructor.name;
    this.errors = errors;
  }
}
