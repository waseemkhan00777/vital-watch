"use client";

import { useAuth } from "@/context/AuthContext";
import { AppShell } from "@/components/layout/AppShell";
import { useRouter } from "next/navigation";
import { useEffect } from "react";

export default function CaregiverLayout({
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
    if (user.role !== "caregiver") {
      router.replace("/login");
    }
  }, [isAuthenticated, user, router]);

  if (!user || user.role !== "caregiver") {
    return null;
  }

  return <AppShell role="caregiver">{children}</AppShell>;
}
