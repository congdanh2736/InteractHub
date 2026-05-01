import { StrictMode } from 'react';
import { createRoot } from 'react-dom/client';
import './index.css'; // DÒNG NÀY LÀ CẦU NỐI KÉO CSS VÀO GIAO DIỆN
import App from './App.tsx';

createRoot(document.getElementById('root')!).render(
    <StrictMode>
        <App />
    </StrictMode>,
);