import { useEffect, useState } from 'react';
import Navbar from '../components/Navbar';
import axiosClient from '../api/axiosClient';
import { useAuth } from '../context/AuthContext';
import { Users, UserPlus, Check, X } from 'lucide-react';
import { toast } from 'react-toastify';

export interface FriendType {
    userId: string;
    displayName: string;
    status: string;
    createdAt: string;
}

export default function Friends() {
    const { user } = useAuth();
    const [activeTab, setActiveTab] = useState<'friends' | 'requests'>('friends');

    const [friends, setFriends] = useState<FriendType[]>([]);
    const [requests, setRequests] = useState<FriendType[]>([]);
    const [isLoading, setIsLoading] = useState(false);

    // 1. Hàm lấy danh sách bạn bè
    const fetchFriends = async () => {
        try {
            const response = await axiosClient.get('/Friends');
            setFriends(response.data);
        } catch (error) {
            console.error("Lỗi lấy danh sách bạn bè", error);
        }
    };

    // 2. Hàm lấy danh sách lời mời chờ duyệt
    const fetchRequests = async () => {
        try {
            const response = await axiosClient.get('/Friends/request');
            setRequests(response.data);
        } catch (error) {
            console.error("Lỗi lấy lời mời kết bạn", error);
        }
    };

    // Gọi 2 hàm trên khi vừa vào trang
    useEffect(() => {
        if (user) {
            setIsLoading(true);
            Promise.all([fetchFriends(), fetchRequests()]).finally(() => setIsLoading(false));
        }
    }, [user]);

    // 3. Hàm xử lý Chấp nhận / Từ chối lời mời
    const handleRespond = async (requesterId: string, status: 'Accepted' | 'Declined') => {
        try {
            // Nhớ đổi axiosClient.post thành axiosClient.put nhé!
            await axiosClient.put('/Friends/respond', {
                requesterId: requesterId,
                status: status
            });

            toast.success(status === 'Accepted' ? 'Đã chấp nhận kết bạn!' : 'Đã từ chối lời mời!');

            fetchFriends();
            fetchRequests();
        } catch (error) {
            toast.error("Có lỗi xảy ra, vui lòng thử lại!");
        }
    };

    return (
        <div className="min-h-screen bg-gray-100 pb-10">
            <Navbar />

            <div className="max-w-4xl mx-auto mt-8 px-4">
                <div className="bg-white rounded-xl shadow-md overflow-hidden">

                    {/* KHU VỰC CHỌN TAB */}
                    <div className="flex border-b">
                        <button
                            onClick={() => setActiveTab('friends')}
                            className={`flex-1 py-4 font-bold flex items-center justify-center transition ${activeTab === 'friends' ? 'text-blue-600 border-b-4 border-blue-600' : 'text-gray-500 hover:bg-gray-50'}`}
                        >
                            <Users className="mr-2" size={20} />
                            Bạn bè ({friends.length})
                        </button>
                        <button
                            onClick={() => setActiveTab('requests')}
                            className={`flex-1 py-4 font-bold flex items-center justify-center transition relative ${activeTab === 'requests' ? 'text-blue-600 border-b-4 border-blue-600' : 'text-gray-500 hover:bg-gray-50'}`}
                        >
                            <UserPlus className="mr-2" size={20} />
                            Lời mời kết bạn
                            {requests.length > 0 && (
                                <span className="ml-2 bg-red-500 text-white text-xs px-2 py-1 rounded-full">{requests.length}</span>
                            )}
                        </button>
                    </div>

                    {/* KHU VỰC HIỂN THỊ NỘI DUNG TAB */}
                    <div className="p-6">
                        {isLoading ? (
                            <div className="text-center text-gray-500 py-10">Đang tải dữ liệu...</div>
                        ) : activeTab === 'friends' ? (

                            /* --- TAB 1: DANH SÁCH BẠN BÈ --- */
                            friends.length === 0 ? (
                                <div className="text-center text-gray-500 py-10">Bạn chưa có người bạn nào.</div>
                            ) : (
                                <div className="grid grid-cols-1 sm:grid-cols-2 gap-4">
                                    {friends.map((friend: FriendType, index: number) => (
                                        <div key={index} className="flex items-center p-4 border rounded-lg hover:shadow-sm transition bg-gray-50">
                                            <div className="w-12 h-12 bg-blue-100 text-blue-600 rounded-full flex items-center justify-center text-xl font-bold mr-4">
                                                {friend.displayName ? friend.displayName.charAt(0).toUpperCase() : 'U'}
                                            </div>
                                            <div>
                                                <h3 className="font-bold text-gray-800 text-lg">{friend.displayName}</h3>
                                                <p className="text-sm text-gray-500">Bạn bè</p>
                                            </div>
                                        </div>
                                    ))}
                                </div>
                            )

                        ) : (

                            /* --- TAB 2: LỜI MỜI KẾT BẠN --- */
                            requests.length === 0 ? (
                                <div className="text-center text-gray-500 py-10">Không có lời mời kết bạn nào.</div>
                            ) : (
                                <div className="grid grid-cols-1 sm:grid-cols-2 gap-4">
                                    {requests.map((req: FriendType, index: number) => (
                                        <div key={index} className="flex items-center justify-between p-4 border rounded-lg shadow-sm bg-white">
                                            <div className="flex items-center">
                                                <div className="w-12 h-12 bg-orange-100 text-orange-600 rounded-full flex items-center justify-center text-xl font-bold mr-4">
                                                    {req.displayName ? req.displayName.charAt(0).toUpperCase() : 'U'}
                                                </div>
                                                <div>
                                                    <h3 className="font-bold text-gray-800">{req.displayName}</h3>
                                                    <p className="text-xs text-gray-500">Vừa gửi lời mời</p>
                                                </div>
                                            </div>
                                            <div className="flex space-x-2">
                                                <button
                                                    onClick={() => handleRespond(req.userId, 'Accepted')}
                                                    className="p-2 bg-blue-600 text-white rounded-full hover:bg-blue-700 transition" title="Chấp nhận"
                                                >
                                                    <Check size={18} />
                                                </button>
                                                <button
                                                    onClick={() => handleRespond(req.userId, 'Declined')}
                                                    className="p-2 bg-gray-200 text-gray-600 rounded-full hover:bg-gray-300 transition" title="Từ chối"
                                                >
                                                    <X size={18} />
                                                </button>
                                            </div>
                                        </div>
                                    ))}
                                </div>
                            )
                        )}
                    </div>
                </div>
            </div>
        </div>
    );
}