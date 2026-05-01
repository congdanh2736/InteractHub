import { createContext, useContext, useState, useEffect, ReactNode } from 'react';
import { useNavigate } from 'react-router-dom';

// 1. Định nghĩa "Hình dáng" của User và Trạm phát sóng
interface User {
    id: string;
    name: string;
    email: string;
}

interface AuthContextType {
    user: User | null;
    login: (token: string) => void;
    logout: () => void;
}

// 2. Tạo Trạm phát sóng (Context)
const AuthContext = createContext<AuthContextType | undefined>(undefined);

// 3. Tạo Cái loa (Provider) để bọc toàn bộ ứng dụng
export function AuthProvider({ children }: { children: ReactNode }) {
    const [user, setUser] = useState<User | null>(null);
    const navigate = useNavigate();

    // Hàm giải mã Token (Chỉ viết 1 lần duy nhất ở đây)
    const decodeToken = (token: string) => {
        try {
            const payload = JSON.parse(atob(token.split('.')[1]));
            //console.log(payload)
            const id = payload["http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier"] || payload.sub;
            const name = payload["http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name"] || payload.DisplayName;
            const email = payload["http://schemas.xmlsoap.org/ws/2005/05/identity/claims/emailaddress"] || payload.email;

            if (id)
                setUser({ id, name, email });

        } catch (error) {
            console.error("Token không hợp lệ", error);
            logout();
        }
    };

    // Vừa vào web là tự động check token ngay
    useEffect(() => {
        const token = localStorage.getItem('token');
        if (token) decodeToken(token);
    }, []);

    // Hàm gọi khi đăng nhập thành công
    const login = (token: string) => {
        localStorage.setItem('token', token);
        decodeToken(token);
    };

    // Hàm Đăng xuất
    const logout = () => {
        localStorage.removeItem('token');
        setUser(null);
        navigate('/login');
    };

    return (
        <AuthContext.Provider value={{ user, login, logout }}>
            {children}
        </AuthContext.Provider>
    );
}

// 4. Tạo một cái móc (Hook) để các trang khác xài cho lẹ
export const useAuth = () => {
    const context = useContext(AuthContext);
    if (context === undefined) {
        throw new Error('useAuth phải được dùng bên trong AuthProvider');
    }
    return context;
};