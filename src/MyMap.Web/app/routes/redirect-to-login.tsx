import { redirect } from "react-router";
export async function clientLoader() { throw redirect("/login"); }
export async function loader() { throw redirect("/login"); }
export default function RedirectToLogin() { return null; }
