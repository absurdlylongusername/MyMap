import { useState } from "react";
import { apiPost } from "../../lib/api";

export function LoginForm({ onSuccess }: { onSuccess: () => void }) {
 const [email, setEmail] = useState("");
 const [password, setPassword] = useState("");
 const [error, setError] = useState<string | null>(null);
 const [busy, setBusy] = useState(false);

 async function onSubmit(e: React.FormEvent) {
 e.preventDefault();
 setError(null);
 setBusy(true);
 const res = await apiPost("/auth/login-cookie", { email, password });
 setBusy(false);
 if (res.status ===200) {
 onSuccess();
 } else {
 setError("Invalid email or password.");
 }
 }

 return (
 <form onSubmit={onSubmit} className="max-w-sm space-y-3">
 <div>
 <label className="block text-sm">Email</label>
 <input
 className="border rounded p-2 w-full"
 type="email"
 autoComplete="username"
 value={email}
 onChange={(e) => setEmail(e.target.value)}
 required
 />
 </div>
 <div>
 <label className="block text-sm">Password</label>
 <input
 className="border rounded p-2 w-full"
 type="password"
 autoComplete="current-password"
 value={password}
 onChange={(e) => setPassword(e.target.value)}
 required
 />
 </div>
 <div className="flex items-center gap-3">
 <button className="border rounded px-3 py-1" disabled={busy}>
 {busy ? "Signing in…" : "Sign in"}
 </button>
 {error && <span className="text-sm text-red-600">{error}</span>}
 </div>
 </form>
 );
}
