import Head from 'next/head';

function Health() {
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
              Healthy
            </h1>
          </div>
        </div>
      </div>
    </section>
  );
}

export default Health;
