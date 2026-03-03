"use client";

import React, { createContext, useContext, useState, useCallback } from "react";
import type { User, Role } from "@/lib/types";

interface AuthState {
  user: User | null;
  isAuthenticated: boolean;
}

interface AuthContextValue extends AuthState {
  login: (email: string, password: string, role: Role) => void;
  logout: () => void;
  setUser: (user: User | null) => void;
}

const AuthContext = createContext<AuthContextValue | null>(null);

// Mock users for frontend-only demo
const MOCK_USERS: Record<Role, User> = {
  admin: {
    id: "admin-1",
    email: "admin@vitalwatch.demo",
    role: "admin",
    name: "Admin User",
  },
  clinician: {
    id: "clinician-1",
    email: "nurse@vitalwatch.demo",
    role: "clinician",
    name: "Jane Nurse",
  },
  patient: {
    id: "patient-1",
    email: "patient@vitalwatch.demo",
    role: "patient",
    name: "John Patient",
  },
  caregiver: {
    id: "caregiver-1",
    email: "caregiver@vitalwatch.demo",
    role: "caregiver",
    name: "Care Giver",
  },
};

export function AuthProvider({ children }: { children: React.ReactNode }) {
  const [state, setState] = useState<AuthState>({
    user: null,
    isAuthenticated: false,
  });

  const login = useCallback((email: string, _password: string, role: Role) => {
    const user = MOCK_USERS[role] ?? {
      id: "user-1",
      email,
      role,
      name: email.split("@")[0],
    };
    setState({ user, isAuthenticated: true });
  }, []);

  const logout = useCallback(() => {
    setState({ user: null, isAuthenticated: false });
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
