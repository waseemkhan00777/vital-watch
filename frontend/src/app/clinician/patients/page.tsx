"use client";

import Link from "next/link";
import { useQuery } from "@tanstack/react-query";
import { Card, CardHeader } from "@/components/ui/Card";
import { PageHeader } from "@/components/ui/PageHeader";
import { usersApi, alertsApi } from "@/lib/api";

export default function ClinicianPatientsPage() {
  const { data: users = [], isLoading, error } = useQuery({
    queryKey: ["users", "role", "patient"],
    queryFn: () => usersApi.list({ role: "patient", limit: 100 }),
  });

  const { data: alerts = [] } = useQuery({
    queryKey: ["alerts", "clinician"],
    queryFn: () => alertsApi.list({ limit: 500 }),
  });

  const alertCountByPatient = alerts.reduce<Record<string, number>>((acc, a) => {
    if (a.state !== "resolved" && a.state !== "archived") {
      acc[a.patientId] = (acc[a.patientId] ?? 0) + 1;
    }
    return acc;
  }, {});

  return (
    <>
      <PageHeader
        title="Patients"
        description="Assigned patients only. Role-based filtering enforced at API."
      />
      <Card>
        <CardHeader
          title="Patient list"
          description="Patients you can monitor. View vitals and alerts per patient."
        />
        {isLoading && <p className="text-sm text-slate-600">Loading…</p>}
        {error && (
          <p className="text-sm text-red-600">
            {error instanceof Error ? error.message : "Failed to load patients."}
          </p>
        )}
        {!isLoading && !error && (
          <div className="overflow-x-auto">
            <table className="w-full text-sm">
              <thead>
                <tr className="border-b border-slate-200 text-left text-slate-600">
                  <th className="pb-3 font-medium">Name</th>
                  <th className="pb-3 font-medium">Email</th>
                  <th className="pb-3 font-medium">Active alerts</th>
                  <th className="pb-3 font-medium">Actions</th>
                </tr>
              </thead>
              <tbody>
                {users.length === 0 ? (
                  <tr>
                    <td colSpan={4} className="py-4 text-slate-500">
                      No patients found.
                    </td>
                  </tr>
                ) : (
                  users.map((p) => (
                    <tr
                      key={p.id}
                      className="border-b border-slate-100 transition-colors hover:bg-slate-50/80"
                    >
                      <td className="py-3 font-medium text-slate-900">{p.name}</td>
                      <td className="py-3 text-slate-600">{p.email}</td>
                      <td className="py-3">
                        {(alertCountByPatient[p.id] ?? 0) > 0 ? (
                          <span className="text-amber-600">
                            {alertCountByPatient[p.id]}
                          </span>
                        ) : (
                          <span className="text-slate-500">0</span>
                        )}
                      </td>
                      <td className="py-3">
                        <Link
                          href={`/clinician/vitals?patientId=${p.id}`}
                          className="text-primary-600 hover:underline"
                        >
                          View vitals
                        </Link>
                        {" · "}
                        <Link
                          href="/clinician"
                          className="text-primary-600 hover:underline"
                        >
                          Alerts
                        </Link>
                      </td>
                    </tr>
                  ))
                )}
              </tbody>
            </table>
          </div>
        )}
      </Card>
    </>
  );
}
