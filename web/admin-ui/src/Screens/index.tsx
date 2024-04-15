import LoadingScreen from '../LoadingScreen';
import createRouterFactory from '../Router/createRouterFactory';
import withRelay from '../Router/withRelay';
import { route as ClientRoute } from './Client';
import { route as HomeRoute } from './Home';
import { route as LoginRoute } from './Login';
import { route as LogoutRoute } from './Logout';
import { route as AnnouncementsRoute } from './Announcements';
import { route as ClientsRoute } from './Clients';

export const routes = [
  LogoutRoute,
  LoginRoute,
  HomeRoute,
  ClientRoute,
  ClientsRoute,
  AnnouncementsRoute,
];

const router = withRelay(createRouterFactory(), routes, LoadingScreen);

export default router;
