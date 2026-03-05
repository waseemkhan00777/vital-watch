"use client";

import React, {
  createContext,
  useContext,
  useState,
  useCallback,
  useEffect,
} from "react";
import type { User, Role } from "@/lib/types";
import { authApi, type ApiError } from "@/lib/api";

const AUTH_BROADCAST_CHANNEL = "vitalcare-auth";

interface AuthState {
  user: User | null;
  isAuthenticated: boolean;
  isLoading: boolean;
}

interface AuthContextValue extends AuthState {
  login: (email: string, password: string) => Promise<User>;
  logout: () => Promise<void>;
  setUser: (user: User | null) => void;
  refetchMe: () => Promise<void>;
}

const AuthContext = createContext<AuthContextValue | null>(null);

function mapUser(d: { id: string; email: string; role: string; name: string }): User {
  return {
    id: d.id,
    email: d.email,
    role: d.role as Role,
    name: d.name,
  };
}

export function AuthProvider({ children }: { children: React.ReactNode }) {
  const [state, setState] = useState<AuthState>({
    user: null,
    isAuthenticated: false,
    isLoading: true,
  });

  const refetchMe = useCallback(async () => {
    try {
      const d = await authApi.me();
      setState({
        user: mapUser(d),
        isAuthenticated: true,
        isLoading: false,
      });
    } catch {
      setState((prev) => ({ ...prev, user: null, isAuthenticated: false, isLoading: false }));
    }
  }, []);

  useEffect(() => {
    refetchMe();
  }, [refetchMe]);

  // Sync auth across tabs: when another tab logs in/out, refetch or clear
  useEffect(() => {
    if (typeof window === "undefined") return;
    const channel = new BroadcastChannel(AUTH_BROADCAST_CHANNEL);
    const handle = (e: MessageEvent<{ type: string }>) => {
      if (e.data?.type === "auth:login") refetchMe();
      if (e.data?.type === "auth:logout") setState({ user: null, isAuthenticated: false, isLoading: false });
    };
    channel.addEventListener("message", handle);
    return () => {
      channel.removeEventListener("message", handle);
      channel.close();
    };
  }, [refetchMe]);

  const login = useCallback(
    async (email: string, password: string): Promise<User> => {
      setState((prev) => ({ ...prev, isLoading: true }));
      const res = await authApi.login(email, password);
      const user = mapUser(res.user);
      setState({
        user,
        isAuthenticated: true,
        isLoading: false,
      });
      if (typeof window !== "undefined") {
        try {
          new BroadcastChannel(AUTH_BROADCAST_CHANNEL).postMessage({ type: "auth:login" });
        } catch { /* ignore */ }
      }
      return user;
    },
    []
  );

  const logout = useCallback(async () => {
    try {
      await authApi.logout();
    } catch {
      // Cookie may already be cleared or expired
    }
    setState({ user: null, isAuthenticated: false, isLoading: false });
    if (typeof window !== "undefined") {
      try {
        new BroadcastChannel(AUTH_BROADCAST_CHANNEL).postMessage({ type: "auth:logout" });
      } catch { /* ignore */ }
    }
  }, []);

  const setUser = useCallback((user: User | null) => {
    setState((prev) => ({
      ...prev,
      user,
      isAuthenticated: !!user,
    }));
  }, []);

  return (
    <AuthContext.Provider
      value={{
        ...state,
        login,
        logout,
        setUser,
        refetchMe,
      }}
    >
      {children}
    </AuthContext.Provider>
  );
}

export function useAuth() {
  const ctx = useContext(AuthContext);
  if (!ctx) throw new Error("useAuth must be used within AuthProvider");
  return ctx;
}

export function isApiError(e: unknown): e is ApiError {
  return typeof e === "object" && e != null && "status" in e;
}
