import React, {
  createContext,
  useContext,
  useCallback,
  useMemo,
  useState,
} from 'react';

type AuthContextType = {
  token: string | null;
  storeToken: (token: string) => void;
};

const AuthContext = createContext<AuthContextType>({
  token: null,
  storeToken: (_: string) => {},
});

export const AuthProvider = ({ children }: React.PropsWithChildren) => {
  const [token, setToken] = useState(() => sessionStorage.getItem('token'));

  const storeToken = useCallback((newToken: string) => {
    sessionStorage.setItem('token', newToken);
    setToken(newToken);
  }, []);

  const state = useMemo(() => ({ token, storeToken }), [token, storeToken]);

  return <AuthContext.Provider value={state}>{children}</AuthContext.Provider>;
};

export const useAuth = () => useContext(AuthContext);
