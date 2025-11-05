export function TopBar() {
 return (
 <header className="fixed top-0 left-0 right-0 h-12 border-b bg-white/70 dark:bg-gray-900/70 backdrop-blur flex items-center px-4 z-10">
 <div className="flex-1 font-semibold">MyMap</div>
 <nav className="flex items-center gap-3 text-sm">
 <button className="underline">Data info</button>
 <a className="underline" href="/login">Sign out</a>
 </nav>
 </header>
 );
}
