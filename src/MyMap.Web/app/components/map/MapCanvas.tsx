import { lazy, Suspense } from "react";

// Lazy-load Leaflet components only on the client to avoid SSR "window is not defined".
const MapCanvasInner = lazy(() => import("./MapCanvasInner"));

export function MapCanvas() {
 if (typeof window === "undefined") {
 // Render nothing during SSR
 return null;
 }
 return (
 <Suspense fallback={<div className="p-3 text-sm text-gray-600">Loading map…</div>}>
 <MapCanvasInner />
 </Suspense>
 );
}
