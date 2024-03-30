import { Box, Modal, ModalProps, Typography } from '@mui/material';
import { useCallback, useState } from 'react';
const style = {
  position: 'absolute' as 'absolute',
  top: '50%',
  left: '50%',
  transform: 'translate(-50%, -50%)',
  width: 400,
  bgcolor: 'background.paper',
  border: '2px solid #000',
  boxShadow: 24,
  p: 4,
};

type QuickModalProps = {
  title: string;
  children: React.ReactNode;
  open: boolean;
} & ModalProps;

const QuickModal = ({
  title,
  children,
  open,
  onClose,
  ...props
}: QuickModalProps) => {
  const [isOpen, setIsOpen] = useState(open);
  const handleClose = useCallback(
    (event: {}, reason: 'backdropClick' | 'escapeKeyDown') => {
      setIsOpen(false);
      onClose?.(event, reason);
    },
    [onClose]
  );
  return (
    <Modal open={isOpen} onClose={handleClose} {...props}>
      <Box sx={style}>
        <Typography variant="h6" component="h2">
          {title}
        </Typography>
        {children}
      </Box>
    </Modal>
  );
};

export default QuickModal;
