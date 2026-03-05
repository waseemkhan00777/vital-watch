"use client";

import { useQuery } from "@tanstack/react-query";
import { Card, CardHeader } from "@/components/ui/Card";
import { PageHeader } from "@/components/ui/PageHeader";
import { auditApi } from "@/lib/api";

function formatDate(iso: string) {
  return new Date(iso).toLocaleString(undefined, {
    dateStyle: "short",
    timeStyle: "short",
  });
}

export default function AdminAuditPage() {
  const { data: logs = [], isLoading, error } = useQuery({
    queryKey: ["audit", "admin"],
    queryFn: () => auditApi.list({ limit: 200 }),
  });

  return (
    <>
      <PageHeader
        title="Audit logs"
        description="System-wide audit trail. Immutable; all actions logged."
      />
      <Card>
        <CardHeader
          title="Audit log"
          description="User ID, role, resource, timestamp, IP, action."
        />
        {isLoading && <p className="text-sm text-slate-600">Loading…</p>}
        {error && (
          <p className="text-sm text-red-600">
            {error instanceof Error ? error.message : "Failed to load audit log."}
          </p>
        )}
        {!isLoading && !error && (
          <div className="overflow-x-auto">
            <table className="w-full text-sm">
              <thead>
                <tr className="border-b border-slate-200 text-left text-slate-600">
                  <th className="pb-3 font-medium">Time</th>
                  <th className="pb-3 font-medium">User / Email</th>
                  <th className="pb-3 font-medium">Role</th>
                  <th className="pb-3 font-medium">Resource</th>
                  <th className="pb-3 font-medium">Action</th>
                  <th className="pb-3 font-medium">IP</th>
                </tr>
              </thead>
              <tbody>
                {logs.length === 0 ? (
                  <tr>
                    <td colSpan={6} className="py-4 text-slate-500">
                      No audit entries yet.
                    </td>
                  </tr>
                ) : (
                  logs.map((r) => (
                    <tr key={r.id} className="border-b border-slate-100">
                      <td className="py-3 text-slate-600">{formatDate(r.timestamp)}</td>
                      <td className="py-3 text-slate-900">
                        {r.userEmail ?? r.userId ?? "—"}
                      </td>
                      <td className="py-3 capitalize text-slate-600">{r.role}</td>
                      <td className="py-3 text-slate-600">{r.resource}</td>
                      <td className="py-3 font-medium text-slate-900">{r.action}</td>
                      <td className="py-3 text-slate-500">{r.ipAddress ?? "—"}</td>
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
