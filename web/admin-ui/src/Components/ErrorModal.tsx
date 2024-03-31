import { Box, Button, Typography } from '@mui/material';
import { useCallback, useState } from 'react';
import QuickModal from './QuickModal';
import CodeBlock from './CodeBlock';

export type ErrorModalProps = {
  error: Error;
  open: boolean;
  onClose: (event: {}, reason: 'backdropClick' | 'escapeKeyDown') => void;
};

const ErrorModal = ({ error, open, onClose }: ErrorModalProps) => {
  const [isOpen, setIsOpen] = useState(open);
  const [showStack, setShowStack] = useState(false);
  const handleClose = useCallback(
    (event: {}, reason: 'backdropClick' | 'escapeKeyDown') => {
      setIsOpen(false);
      onClose?.(event, reason);
    },
    [onClose]
  );
  return (
    <QuickModal
      title={`An ${error.name} Occurred`}
      open={isOpen}
      onClose={handleClose}
    >
      <>
        <Typography>{error.message}</Typography>
        {showStack && (
          <CodeBlock text={error.stack ?? '(no stack available)'} />
        )}
        <Box sx={{ display: 'flex', justifyContent: 'flex-end' }}>
          <Button onClick={() => setShowStack(!showStack)}>
            {showStack ? 'Hide' : 'Show'} Stack
          </Button>
        </Box>
      </>
    </QuickModal>
  );
};

export default ErrorModal;
