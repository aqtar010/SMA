import AdminGuard from "@/Components/AdminGuard";
import AdminNavbar from "@/Components/AdminNavbar";

export default function AdminLayout({
  children,
}: {
  children: React.ReactNode;
}) {
  return (
    <AdminGuard>
      <div className="min-h-screen bg-slate-50 text-slate-900">
        <AdminNavbar />
        <main className="mx-auto max-w-7xl px-4 py-8">{children}</main>
      </div>
    </AdminGuard>
  );
}
