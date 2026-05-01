import { useNavigate } from 'react-router-dom';
import { LogOut, Home, Users, Search, Bell, Check, Clock } from 'lucide-react';
import { useAuth } from '../context/AuthContext';
import { useState, useEffect, useRef } from 'react';
import { useDebounce } from '../hooks/useDebounce';
import axiosClient from '../api/axiosClient';
import * as signalR from '@microsoft/signalr';

export interface NotificationType {
    id: number;
    message: string;
    type: string;
    isRead: boolean;
    createdAt: string;
}

export interface SearchUserType {
    id: string;
    displayName: string;
    email?: string;
}

export default function Navbar() {
    const navigate = useNavigate();
    const { logout } = useAuth();

    // Các state tìm kiếm
    const [searchTerm, setSearchTerm] = useState('');
    const [searchResults, setSearchResults] = useState<SearchUserType[]>([]);
    const [isSearching, setIsSearching] = useState(false);
    const [showDropdown, setShowDropdown] = useState(false);
    const debouncedSearchTerm = useDebounce(searchTerm, 500);

    // Các state thông báo
    const [notifications, setNotifications] = useState<NotificationType[]>([]);
    const [showNotifDropdown, setShowNotifDropdown] = useState(false);
    const [unreadCount, setUnreadCount] = useState(0);

    // Xử lý ref để click ra ngoài thì đóng menu thông báo
    const notifRef = useRef<HTMLDivElement>(null);

    useEffect(() => {
        const handleClickOutside = (event: MouseEvent) => {
            if (notifRef.current && !notifRef.current.contains(event.target as Node)) {
                setShowNotifDropdown(false);
            }
        };
        document.addEventListener("mousedown", handleClickOutside);
        return () => document.removeEventListener("mousedown", handleClickOutside);
    }, []);

    // 1. TẢI THÔNG BÁO TỪ BACKEND
    const fetchNotifications = async () => {
        try {
            const res = await axiosClient.get('/Notifications');
            const data: NotificationType[] = Array.isArray(res.data) ? res.data : (res.data?.$values || []);
            setNotifications(data);
            setUnreadCount(data.filter(n => !n.isRead).length);
        } catch (error) {
            console.error("Lỗi khi tải thông báo", error);
        }
    };

    // 2. LẮNG NGHE THÔNG BÁO TỪ SIGNALR VÀ XÓA TOAST
    useEffect(() => {
        fetchNotifications();

        const token = localStorage.getItem('token');
        if (!token) return;

        const connection = new signalR.HubConnectionBuilder()
            .withUrl("https://localhost:7061/notificationHub", { accessTokenFactory: () => token })
            .withAutomaticReconnect()
            .build();

        connection.start().catch(err => console.log("Loi ket noi signalR", err));

        // Nhận thông báo mới từ server và cập nhật vào danh sách Notification Area
        connection.on("ReceiveNotification", (notification: NotificationType) => {
            setNotifications(prev => [notification, ...prev]);
            setUnreadCount(prev => prev + 1);
            // LƯU Ý: KHÔNG DÙNG toast.success NỮA!
        });

        return () => { connection.stop(); };
    }, []);

    // Đánh dấu đã đọc
    const handleMarkAsRead = async (id: number) => {
        try {
            await axiosClient.put(`/Notifications/${id}/read`);
            setNotifications(prev => prev.map(n => n.id === id ? { ...n, isRead: true } : n));
            setUnreadCount(prev => Math.max(0, prev - 1));
        } catch (error) {
            console.error("Lỗi đánh dấu đã đọc");
        }
    };

    // Tìm kiếm
    useEffect(() => {
        const fetchSearchResults = async () => {
            if (debouncedSearchTerm) {
                setIsSearching(true);
                setShowDropdown(true);
                try {
                    const response = await axiosClient.get('/User/search', { params: { q: debouncedSearchTerm } });
                    setSearchResults(Array.isArray(response.data) ? response.data : response.data.items || []);
                } catch (error) {
                    setSearchResults([]);
                } finally {
                    setIsSearching(false);
                }
            } else {
                setSearchResults([]);
                setShowDropdown(false);
            }
        };
        fetchSearchResults();
    }, [debouncedSearchTerm]);

    return (
        <nav className="bg-white shadow-md sticky top-0 z-50">
            <div className="max-w-6xl mx-auto px-4">
                <div className="flex justify-between items-center h-16">
                    <div className="text-2xl font-bold text-blue-600 cursor-pointer" onClick={() => navigate('/')}>
                        InteractHub
                    </div>

                    {/* Ô TÌM KIẾM */}
                    <div className="hidden md:flex flex-1 max-w-md mx-8 relative">
                        <div className="relative w-full">
                            <input
                                type="text"
                                placeholder="Tìm kiếm bạn bè..."
                                className="w-full bg-gray-100 text-gray-800 px-4 py-2 pl-10 rounded-full focus:outline-none focus:ring-2 focus:ring-blue-300 transition"
                                value={searchTerm}
                                onChange={(e) => setSearchTerm(e.target.value)}
                                onFocus={() => { if (searchTerm) setShowDropdown(true); }}
                                onBlur={() => setTimeout(() => setShowDropdown(false), 200)}
                            />
                            <Search className="absolute left-3 top-2.5 text-gray-400" size={18} />

                            {showDropdown && (
                                <div className="absolute top-full left-0 right-0 mt-2 bg-white rounded-xl shadow-lg border border-gray-100 max-h-96 overflow-y-auto z-50">
                                    {isSearching ? (
                                        <div className="p-4 text-center text-gray-500 text-sm">Đang tìm kiếm...</div>
                                    ) : searchResults.length > 0 ? (
                                        <ul>
                                            {searchResults.map((u: SearchUserType) => (
                                                <li key={u.id} className="px-4 py-3 hover:bg-gray-50 cursor-pointer flex items-center border-b border-gray-50" onClick={() => { navigate(`/profile/${u.id}`); setShowDropdown(false); setSearchTerm(''); }}>
                                                    <div className="w-10 h-10 bg-blue-100 text-blue-600 rounded-full flex items-center justify-center font-bold mr-3">{u.displayName?.charAt(0).toUpperCase()}</div>
                                                    <div><div className="font-semibold">{u.displayName}</div></div>
                                                </li>
                                            ))}
                                        </ul>
                                    ) : debouncedSearchTerm ? (
                                        <div className="p-4 text-center text-gray-500 text-sm">Không tìm thấy</div>
                                    ) : null}
                                </div>
                            )}
                        </div>
                    </div>

                    {/* ICON MENU */}
                    <div className="flex space-x-4 items-center">
                        <button onClick={() => navigate('/')} className="p-2 text-gray-600 hover:bg-gray-100 rounded-full" title="Trang chủ">
                            <Home size={22} />
                        </button>

                        <button onClick={() => navigate('/friends')} className="p-2 text-gray-600 hover:bg-gray-100 rounded-full" title="Bạn bè">
                            <Users size={22} />
                        </button>

                        {/* KHU VỰC THÔNG BÁO (NOTIFICATION AREA) */}
                        <div className="relative" ref={notifRef}>
                            <button
                                onClick={() => setShowNotifDropdown(!showNotifDropdown)}
                                className="p-2 text-gray-600 hover:bg-gray-100 rounded-full relative"
                                title="Thông báo"
                            >
                                <Bell size={22} />
                                {unreadCount > 0 && (
                                    <span className="absolute top-1 right-1 bg-red-500 text-white text-[10px] font-bold w-4 h-4 rounded-full flex items-center justify-center">
                                        {unreadCount > 9 ? '9+' : unreadCount}
                                    </span>
                                )}
                            </button>

                            {/* DROPDOWN THÔNG BÁO */}
                            {showNotifDropdown && (
                                <div className="absolute right-0 mt-2 w-80 bg-white rounded-xl shadow-2xl border border-gray-100 max-h-[400px] overflow-y-auto z-50">
                                    <div className="p-4 border-b border-gray-100 flex justify-between items-center bg-gray-50/50 sticky top-0 z-10">
                                        <h3 className="font-bold text-gray-800 text-lg">Thông báo</h3>
                                    </div>
                                    <div className="flex flex-col">
                                        {notifications.length === 0 ? (
                                            <div className="p-8 text-center text-gray-500 flex flex-col items-center">
                                                <Bell className="mb-2 text-gray-300" size={32} />
                                                Bạn chưa có thông báo nào.
                                            </div>
                                        ) : (
                                            notifications.map(notif => (
                                                <div
                                                    key={notif.id}
                                                    onClick={() => handleMarkAsRead(notif.id)}
                                                    className={`p-4 border-b border-gray-50 cursor-pointer flex items-start gap-3 transition ${notif.isRead ? 'bg-white opacity-60' : 'bg-blue-50/30 hover:bg-blue-50/50'}`}
                                                >
                                                    <div className={`w-10 h-10 rounded-full flex items-center justify-center flex-shrink-0 ${notif.isRead ? 'bg-gray-200 text-gray-500' : 'bg-blue-100 text-blue-600'}`}>
                                                        <Bell size={18} />
                                                    </div>
                                                    <div className="flex-1">
                                                        <p className={`text-sm ${notif.isRead ? 'text-gray-600' : 'text-gray-800 font-medium'}`}>
                                                            {notif.message}
                                                        </p>
                                                        <div className="flex items-center text-xs text-gray-400 mt-1">
                                                            <Clock size={12} className="mr-1" />
                                                            {new Date(notif.createdAt).toLocaleString('vi-VN', { hour: '2-digit', minute: '2-digit', day: '2-digit', month: '2-digit' })}
                                                        </div>
                                                    </div>
                                                    {!notif.isRead && (
                                                        <div className="w-2 h-2 bg-blue-600 rounded-full mt-2 flex-shrink-0"></div>
                                                    )}
                                                </div>
                                            ))
                                        )}
                                    </div>
                                </div>
                            )}
                        </div>

                        <button onClick={logout} className="p-2 text-red-500 hover:bg-red-50 rounded-full ml-2" title="Đăng xuất">
                            <LogOut size={22} />
                        </button>
                    </div>
                </div>
            </div>
        </nav>
    );
}