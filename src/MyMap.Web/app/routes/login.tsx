import { useNavigate } from "react-router";
import { LoginForm } from "../components/auth/LoginForm";

export function meta() {
 return [
 { title: "Sign in" },
 ];
}

export default function Login() {
 const navigate = useNavigate();
 return (
 <main className="pt-16 p-4 container mx-auto">
 <h1 className="text-xl font-semibold mb-4">Sign in</h1>
 <LoginForm onSuccess={() => navigate("/map")} />
 </main>
 );
}
