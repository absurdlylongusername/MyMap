import { MapContainer, TileLayer } from "react-leaflet";

export default function MapCanvasInner() {
 return (
 <MapContainer
 className="w-full h-[60vh]"
 center={[51.5074, -0.1278]}
 zoom={12}
>
 <TileLayer
 attribution='&copy; <a href="https://www.openstreetmap.org/copyright">OSM</a>'
 url="https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png"
 />
 </MapContainer>
 );
}
