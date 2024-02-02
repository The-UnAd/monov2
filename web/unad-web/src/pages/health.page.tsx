import { createModelFactory } from '@/lib/redis';

function Health({ health }: { health: boolean }) {
  return (
    <section className="app">
      <div className="container-app h100 d-flex align-items-center">
        <div className="col-12 text-center">
          <div className="box box-1">
            <h1
              className="primary mb-1"
              style={{
                textTransform: 'initial',
              }}
            >
              {health ? 'Healthy' : 'Unhealthy'}
            </h1>
          </div>
        </div>
      </div>
    </section>
  );
}

export async function getServerSideProps() {
  const redis = createModelFactory();
  const health = await redis.healthCheck();
  return {
    props: {
      health,
    },
  };
}

export default Health;
