import { Link as WouterLink, type LinkProps as WouterLinkProps } from 'wouter';
import { Link as MuiLink, type LinkProps as MuiLinkProps } from '@mui/material';
import React from 'react';

type LinkProps = WouterLinkProps & MuiLinkProps;

const Link = React.forwardRef<HTMLAnchorElement, LinkProps>(
  ({ to, ...props }, ref) => (
    <MuiLink component={WouterLink} to={to} ref={ref} {...props} />
  )
);
export default Link;
