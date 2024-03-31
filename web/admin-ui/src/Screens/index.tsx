import LoadingScreen from '../LoadingScreen';
import createRouterFactory from '../Router/createRouterFactory';
import withRelay, { RouteDefinition } from '../Router/withRelay';
import { route as ClientRoute } from './Client';
import { route as HomeRoute } from './Home';
import { route as LoginRoute } from './Login';
import { route as LogoutRoute } from './Logout';
import { route as AnnouncementsRoute } from './Announcements';

const router = withRelay(
  createRouterFactory(),
  [
    LogoutRoute,
    LoginRoute,
    HomeRoute,
    ClientRoute,
    AnnouncementsRoute as RouteDefinition,
  ],
  LoadingScreen
);

export default router;
