import './App.css';

import { RelayEnvironmentProvider } from 'react-relay';

import RelayEnvironment from './RelayEnvironment';
import Screens from './Screens';

function App() {
  return (
    <RelayEnvironmentProvider environment={RelayEnvironment}>
      <Screens />
    </RelayEnvironmentProvider>
  );
}

export default App;
