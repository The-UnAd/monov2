import './App.css';

import { RelayEnvironmentProvider } from 'react-relay';
import { Link } from 'wouter';

import RelayEnvironment from './RelayEnvironment';
import Screens from './Screens';

function App() {
  return (
    <RelayEnvironmentProvider environment={RelayEnvironment}>
      <Link to="/">Home</Link>
      <Link to="/other">Other</Link>
      <Screens />
    </RelayEnvironmentProvider>
  );
}

export default App;
