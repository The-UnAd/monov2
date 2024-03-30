import { Box, Container, Tab, Tabs } from '@mui/material';
import Link from './Components/Link';
import { usePathname } from 'wouter/use-browser-location';

export default function Layout({
  children,
}: Readonly<{ children: React.ReactNode }>) {
  const route = usePathname();
  return (
    <>
      <Tabs value={route}>
        <Tab label="Home" value="/" to="/" component={Link} />
        <Tab label="Other" value="/other" to="/other" component={Link} />
        <Tab label="Test" value="/test" to="/test" component={Link} />
      </Tabs>
      <Container maxWidth="lg">
        <Box sx={{ my: 4 }}>{children}</Box>
      </Container>
    </>
  );
}
