import Layout from '../Layout';
import LoadingScreen from '../LoadingScreen';
import createRouterFactory from '../Router/createRouterFactory';
import withRelay, { RouteDefinition } from '../Router/withRelay';
import { route as ClientRoute } from './Client';
import { route as HomeRoute } from './Home';
import { route as TestRoute } from './Test';

const router = withRelay(
  createRouterFactory({}, Layout),
  [HomeRoute, ClientRoute, TestRoute as RouteDefinition],
  LoadingScreen
);

export default router;
