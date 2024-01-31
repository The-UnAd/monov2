export interface ITransactionContext {
  beginTransaction(): void;
  commitTransaction(): Promise<void>;
  rollbackTransaction(): void;
}

export class TransactionContext {
  constructor(private readonly contexts: ITransactionContext[]) {}

  public beginTransaction(): void {
    this.contexts.forEach((context) => context.beginTransaction());
  }
  public async commitTransaction(): Promise<void> {
    this.contexts.forEach((context) => context.commitTransaction());
  }
  public rollbackTransaction(): void {
    this.contexts.forEach((context) => context.rollbackTransaction());
  }
}
