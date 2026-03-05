"use client";

import { useAuth } from "@/context/AuthContext";
import { AppShell } from "@/components/layout/AppShell";
import { useRouter } from "next/navigation";
import { useEffect } from "react";

export default function PatientLayout({
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
    if (user.role !== "patient") {
      router.replace("/login");
    }
  }, [isAuthenticated, user, isLoading, router]);

  if (isLoading || !user || user.role !== "patient") {
    return null;
  }

  return <AppShell role="patient">{children}</AppShell>;
}
