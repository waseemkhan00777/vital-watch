"use client";

import Link from "next/link";
import { useQuery } from "@tanstack/react-query";
import { Card, CardHeader } from "@/components/ui/Card";
import { PageHeader } from "@/components/ui/PageHeader";
import { caregiverLinksApi } from "@/lib/api";

function formatDate(iso: string) {
  return new Date(iso).toLocaleString(undefined, {
    dateStyle: "short",
  });
}

export default function CaregiverDashboardPage() {
  const { data: links = [], isLoading, error } = useQuery({
    queryKey: ["caregiverlinks"],
    queryFn: () => caregiverLinksApi.list({ activeOnly: true }),
  });

  return (
    <>
      <PageHeader
        title="Caregiver dashboard"
        description="View-only access to consented patient vitals. You cannot modify data or acknowledge alerts."
      />
      <Card>
        <CardHeader
          title="Consented patients"
          description="Patients who have granted you access. Read-only."
        />
        {isLoading && <p className="text-sm text-slate-600">Loading…</p>}
        {error && (
          <p className="text-sm text-red-600">
            {error instanceof Error ? error.message : "Failed to load."}
          </p>
        )}
        {!isLoading && !error && (
          <>
            {links.length === 0 ? (
              <p className="text-sm text-slate-600">
                When a patient adds you as caregiver, their vitals will appear
                here. Revocation by patient takes effect immediately and is
                logged.
              </p>
            ) : (
              <ul className="space-y-2">
                {links.map((l) => (
                  <li
                    key={l.id}
                    className="flex flex-wrap items-center justify-between gap-2 rounded-xl border border-slate-200 bg-slate-50/50 px-4 py-3"
                  >
                    <span className="font-medium text-slate-900">
                      Patient
                    </span>
                    <span className="text-sm text-slate-600">
                      Consented {formatDate(l.consentedAt)}
                    </span>
                    <Link
                      href={`/caregiver/vitals?patientId=${l.patientId}`}
                      className="btn-secondary text-sm"
                    >
                      View vitals
                    </Link>
                  </li>
                ))}
              </ul>
            )}
          </>
        )}
      </Card>
    </>
  );
}
