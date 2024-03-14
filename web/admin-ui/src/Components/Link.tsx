import { Link as WouterLink, type LinkProps as WouterLinkProps } from 'wouter';
import { Link as MuiLink, type LinkProps as MuiLinkProps } from '@mui/material';

type LinkProps = WouterLinkProps & MuiLinkProps;

export default function Link({ to, ...props }: LinkProps) {
  return <MuiLink component={WouterLink} to={to} {...props} />;
}
