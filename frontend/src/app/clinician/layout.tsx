"use client";

import { useAuth } from "@/context/AuthContext";
import { AppShell } from "@/components/layout/AppShell";
import { useRouter } from "next/navigation";
import { useEffect } from "react";

export default function ClinicianLayout({
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
    if (user.role !== "clinician") {
      router.replace("/login");
    }
  }, [isAuthenticated, user, isLoading, router]);

  if (isLoading || !user || user.role !== "clinician") {
    return null;
  }

  return <AppShell role="clinician">{children}</AppShell>;
}
