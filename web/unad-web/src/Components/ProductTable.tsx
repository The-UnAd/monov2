import type { price_tier as PriceTier, Prisma } from '@unad/product-models';

type PlanWithTier = Prisma.planGetPayload<{
  include: { price_tier: true };
}>;

type ProductTableProps = Readonly<{
  plans: PlanWithTier[];
  onSelect: (tier: PriceTier) => void;
}>;

const ProductTable = ({ plans, onSelect }: ProductTableProps) => {
  return (
    <div>
      {plans.map((plan) => (
        <div key={plan.id}>
          {plan.name}
          <div>
            {plan.price_tier.map((t) => (
              <button onClick={() => onSelect(t)} key={t.id}>
                {t.name}
              </button>
            ))}
          </div>
        </div>
      ))}
    </div>
  );
};

export default ProductTable;
