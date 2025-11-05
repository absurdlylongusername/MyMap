import { Sidebar } from "../components/layout/Sidebar";
import { FooterStatus } from "../components/layout/FooterStatus";
import { MapCanvas } from "../components/map/MapCanvas";
import { RequireAuth } from "../components/auth/RequireAuth";

export function meta() {
  return [
    { title: "Map" },
  ];
}

export default function MapPage() {
  return (
    <RequireAuth>
      <main className="pt-16 p-4 container mx-auto">
        <div className="flex gap-4">
          <Sidebar />
          <section className="flex-1 border rounded-md min-h-[60vh] grid">
            <MapCanvas />
          </section>
        </div>
        <FooterStatus />
      </main>
    </RequireAuth>
  );
}
