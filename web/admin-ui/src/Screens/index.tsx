import Layout from '../Layout';
import LoadingScreen from '../LoadingScreen';
import createRouterFactory from '../Router/createRouterFactory';
import withRelay from '../Router/withRelay';
import { route as ClientRoute } from './Client';
import { route as HomeRoute } from './Home';

const router = withRelay(
  createRouterFactory({}, Layout),
  [HomeRoute, ClientRoute],
  LoadingScreen
);

export default router;
