import Mixpanel from 'mixpanel';
const mixpanel = Mixpanel.init(process.env.MIXPANEL_TOKEN!, {
  // test: process.env.NODE_ENV === 'development',
});
export default mixpanel;
