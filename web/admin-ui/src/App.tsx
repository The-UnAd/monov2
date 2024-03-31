import './App.css';

import { RelayEnvironmentProvider } from 'react-relay';

import RelayEnvironment from './RelayEnvironment';
import Screens from './Screens';
import { AuthProvider } from './AuthProvider';
import { Router } from 'wouter';
import Layout from './Layout';

function App() {
  return (
    <AuthProvider>
      <Router>
        <RelayEnvironmentProvider environment={RelayEnvironment}>
          <Layout>
            <Screens />
          </Layout>
        </RelayEnvironmentProvider>
      </Router>
    </AuthProvider>
  );
}

export default App;
