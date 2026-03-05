"use client";

import { useState } from "react";
import { useQuery } from "@tanstack/react-query";
import { Card, CardHeader } from "@/components/ui/Card";
import { PageHeader } from "@/components/ui/PageHeader";
import { usersApi } from "@/lib/api";

export default function AdminUsersPage() {
  const [role, setRole] = useState<string>("");
  const [search, setSearch] = useState("");

  const { data: users = [], isLoading, error } = useQuery({
    queryKey: ["users", "admin", role, search],
    queryFn: () =>
      usersApi.list({
        role: role || undefined,
        search: search || undefined,
        limit: 100,
      }),
  });

  return (
    <>
      <PageHeader
        title="Users"
        description="Create users and assign roles. Enforcement at API and service layer."
      />
      <Card>
        <CardHeader
          title="User list"
          description="Admin only. Deny-by-default RBAC."
        />
        <div className="mb-4 flex flex-wrap gap-2">
          <input
            type="text"
            value={search}
            onChange={(e) => setSearch(e.target.value)}
            placeholder="Search by email"
            className="input-field max-w-xs"
          />
          <select
            value={role}
            onChange={(e) => setRole(e.target.value)}
            className="input-field max-w-[140px]"
          >
            <option value="">All roles</option>
            <option value="admin">Admin</option>
            <option value="clinician">Clinician</option>
            <option value="patient">Patient</option>
            <option value="caregiver">Caregiver</option>
          </select>
        </div>
        {isLoading && <p className="text-sm text-slate-600">Loading…</p>}
        {error && (
          <p className="text-sm text-red-600">
            {error instanceof Error ? error.message : "Failed to load users."}
          </p>
        )}
        {!isLoading && !error && (
          <div className="overflow-x-auto">
            <table className="w-full text-sm">
              <thead>
                <tr className="border-b border-slate-200 text-left text-slate-600">
                  <th className="pb-3 font-medium">Name</th>
                  <th className="pb-3 font-medium">Email</th>
                  <th className="pb-3 font-medium">Role</th>
                  <th className="pb-3 font-medium">ID</th>
                </tr>
              </thead>
              <tbody>
                {users.length === 0 ? (
                  <tr>
                    <td colSpan={4} className="py-4 text-slate-500">
                      No users found.
                    </td>
                  </tr>
                ) : (
                  users.map((u) => (
                    <tr
                      key={u.id}
                      className="border-b border-slate-100 transition-colors hover:bg-slate-50/80"
                    >
                      <td className="py-3 font-medium text-slate-900">{u.name}</td>
                      <td className="py-3 text-slate-600">{u.email}</td>
                      <td className="py-3 capitalize text-slate-600">{u.role}</td>
                      <td className="py-3 font-mono text-xs text-slate-500">{u.id}</td>
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
