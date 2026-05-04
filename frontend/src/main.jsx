import { createRoot } from 'react-dom/client'
import './index.css'
import { BrowserRouter } from "react-router-dom";
import App from "./App";
import faviconUrl from './assets/favicon.svg';

const faviconLink =
  document.querySelector("link[rel='icon']") ?? document.createElement('link');
faviconLink.rel = 'icon';
faviconLink.type = 'image/svg+xml';
faviconLink.href = faviconUrl;
if (!faviconLink.parentNode) document.head.appendChild(faviconLink);

createRoot(document.getElementById('root')).render(
  <BrowserRouter>
    <App />
  </BrowserRouter>
)