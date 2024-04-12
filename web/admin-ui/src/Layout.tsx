import { Box, Container, Tab, Tabs } from '@mui/material';
import Link from './Components/Link';
import { useLocation } from 'wouter';
import { useAuth } from './AuthProvider';
import { useEffect } from 'react';

export default function Layout({
  children,
}: Readonly<{ children: React.ReactNode }>) {
  const { token } = useAuth();
  const [location, setLocation] = useLocation();

  useEffect(() => {
    if (!token) {
      setLocation('/login');
    }
  }, [token, location, setLocation]);

  return (
    <>
      {token && (
        <Tabs value={location}>
          <Tab label="Home" value="/" to="/" component={Link} />
          <Tab
            label="Announcements"
            value="/announcements"
            to="/announcements"
            component={Link}
          />
          <Tab
            label="Logout"
            value="/logout"
            to="/logout"
            component={Link}
            style={{ marginLeft: 'auto', display: token ? 'initial' : 'none' }}
          />
          <Tab
            label="Login"
            value="/login"
            to="/login"
            component={Link}
            style={{ marginLeft: 'auto', display: token ? 'none' : 'initial' }}
          />
        </Tabs>
      )}
      <Container maxWidth="lg">
        <Box sx={{ my: 4 }}>{children}</Box>
      </Container>
    </>
  );
}
