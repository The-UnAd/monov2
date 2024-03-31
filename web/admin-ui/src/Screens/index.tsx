import Layout from '../Layout';
import LoadingScreen from '../LoadingScreen';
import createRouterFactory from '../Router/createRouterFactory';
import withRelay, { RouteDefinition } from '../Router/withRelay';
import { route as ClientRoute } from './Client';
import { route as HomeRoute } from './Home';
import { route as AnnouncementsRoute } from './Announcements';

const router = withRelay(
  createRouterFactory({}, Layout),
  [HomeRoute, ClientRoute, AnnouncementsRoute as RouteDefinition],
  LoadingScreen
);

export default router;
