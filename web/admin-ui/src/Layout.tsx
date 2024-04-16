import { Box, Container, CssBaseline } from '@mui/material';
import { useLocation } from 'wouter';
import { useAuth } from './AuthProvider';
import { useEffect } from 'react';
import AppMenu from './Components/AppMenu';

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
    <Box sx={{ flexGrow: 1 }}>
      <CssBaseline />

      <AppMenu />
      <Container maxWidth="lg">{children}</Container>
    </Box>
  );
}
