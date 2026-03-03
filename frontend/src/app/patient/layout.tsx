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
  const { user, isAuthenticated } = useAuth();
  const router = useRouter();

  useEffect(() => {
    if (!isAuthenticated || !user) {
      router.replace("/login");
      return;
    }
    if (user.role !== "patient") {
      router.replace("/login");
    }
  }, [isAuthenticated, user, router]);

  if (!user || user.role !== "patient") {
    return null;
  }

  return <AppShell role="patient">{children}</AppShell>;
}
