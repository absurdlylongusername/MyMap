export function Sidebar() {
 return (
 <aside className="w-80 shrink-0 border rounded-md p-3">
 <h2 className="font-semibold mb-2">Filters</h2>
 <div className="space-y-2">
 <label className="block text-sm">Category</label>
 <select className="border rounded p-2 w-full">
 <option>All</option>
 <option>Retail</option>
 <option>Cafe</option>
 <option>EV</option>
 <option>School</option>
 </select>
 <div className="pt-3">
 <button className="border rounded px-3 py-1 mr-2">Select by area</button>
 <button className="border rounded px-3 py-1">Clear selection</button>
 </div>
 </div>
 </aside>
 );
}
