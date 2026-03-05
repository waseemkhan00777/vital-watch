"use client";

import { useAuth } from "@/context/AuthContext";
import { AppShell } from "@/components/layout/AppShell";
import { useRouter } from "next/navigation";
import { useEffect } from "react";

export default function AdminLayout({
  children,
}: {
  children: React.ReactNode;
}) {
  const { user, isAuthenticated, isLoading } = useAuth();
  const router = useRouter();

  useEffect(() => {
    if (isLoading) return;
    if (!isAuthenticated || !user) {
      router.replace("/login");
      return;
    }
    if (user.role !== "admin") {
      router.replace("/login");
    }
  }, [isAuthenticated, user, isLoading, router]);

  if (isLoading || !user || user.role !== "admin") {
    return null;
  }

  return <AppShell role="admin">{children}</AppShell>;
}
