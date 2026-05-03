import { Navigate } from 'react-router-dom';
import * as React from 'react';

// ham nay lam chot bao ve de kiem tra token
export default function ProtectedRoute({ children }: { children: React.ReactNode }) {
    const token = localStorage.getItem('token'); // kiem tra token khi co nguoi khac vao

    if (!token) {
        return <Navigate to="/login" replace />; // neu khong co token thi se dua ve trang login
    }

    return <>{children}</>;
}