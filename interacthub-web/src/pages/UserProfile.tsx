import { useEffect, useState } from 'react';
import { useParams, useNavigate } from 'react-router-dom';
import Navbar from '../components/Navbar';
import { Mail, MapPin, Calendar, UserPlus, ArrowLeft, UserMinus, Clock, Check } from 'lucide-react';
import axiosClient from '../api/axiosClient';
import PostItem, { type PostType } from '../components/PostItem';
import { useAuth } from '../context/AuthContext';
import { toast } from 'react-toastify';
import * as React from 'react';

export interface UserProfileInfo {
    id: string;
    displayName: string;
    email: string;
    totalPosts?: number;
}

export default function UserProfile() {
    const { id } = useParams<{ id: string }>(); // ID của người dùng từ URL
    const navigate = useNavigate();
    const { user: currentUser } = useAuth();

    const [userInfo, setUserInfo] = useState<UserProfileInfo | null>(null);
    const [userPosts, setUserPosts] = useState<PostType[]>([]);

    const [isLoading, setIsLoading] = useState(true);
    const [isLoadingPosts, setIsLoadingPosts] = useState(false);

    // Các state liên quan đến kết bạn
    const [friendshipStatus, setFriendshipStatus] = useState<string>('None');
    const [requesterId, setRequesterId] = useState<string>('');

    const isMyProfile = currentUser?.id === id;

    // Lấy thông tin User
    const fetchUserInfo = async () => {
        if (!id) return;
        setIsLoading(true);
        try {
            const response = await axiosClient.get(`/User/${id}`);
            setUserInfo(response.data);
        } catch (error) {
            console.error("Lỗi khi tải thông tin người dùng: ", error);
            setUserInfo(null);
        } finally {
            setIsLoading(false);
        }
    };

    // Lấy bài viết của User
    const fetchUserPosts = async () => {
        if (!id) return;
        setIsLoadingPosts(true);
        try {
            const response = await axiosClient.get(`/Posts/user/${id}`);
            let fetchPosts: PostType[] = [];

            if (Array.isArray(response.data)) {
                fetchPosts = response.data;
            } else if (response.data && Array.isArray(response.data.data)) {
                fetchPosts = response.data.data;
            } else if (response.data && Array.isArray(response.data.items)) {
                fetchPosts = response.data.items;
            } else if (response.data && response.data.items && Array.isArray(response.data.items.result)) {
                // THÊM DÒNG NÀY: Dành riêng cho trường hợp Task chưa được await ở backend
                fetchPosts = response.data.items.result;
            } else if (response.data && Array.isArray(response.data.$values)) {
                fetchPosts = response.data.$values;
            }
            setUserPosts(fetchPosts);
        } catch (error) {
            console.error("Lỗi khi tải bài viết của người dùng: ", error);
        } finally {
            setIsLoadingPosts(false);
        }
    };

    // --- API: KIỂM TRA & XỬ LÝ KẾT BẠN ---
    const fetchFriendshipStatus = async () => {
        if (!id || isMyProfile) return;
        try {
            const response = await axiosClient.get(`/Friends/status/${id}`);
            setFriendshipStatus(response.data.status); // "None", "Pending", "Accepted"
            setRequesterId(response.data.requesterId || '');
        } catch (error) {
            console.error("Lỗi khi tải trạng thái bạn bè: ", error);
        }
    };

    const handleSendRequest = async () => {
        try {
            await axiosClient.post('/Friends/request', { receiverId: id });
            toast.success("Đã gửi lời mời kết bạn!");
            fetchFriendshipStatus();
        } catch (error) {
            toast.error("Không thể gửi lời mời.");
        }
    };

    const handleAcceptRequest = async () => {
        try {
            await axiosClient.put('/Friends/respond', { requesterId: id, status: 'Accepted' });
            toast.success("Đã trở thành bạn bè!");
            fetchFriendshipStatus();
        } catch (error) {
            toast.error("Có lỗi xảy ra khi xác nhận.");
        }
    };

    const handleUnfriendOrCancel = async (actionName: string) => {
        try {
            await axiosClient.delete(`/Friends/${id}`);
            toast.success(`Đã ${actionName} thành công.`);
            fetchFriendshipStatus();
        } catch (error) {
            toast.error("Có lỗi xảy ra.");
        }
    };

    useEffect(() => {
        fetchUserInfo();
        fetchUserPosts();
        fetchFriendshipStatus();
    }, [id]);

    if (isLoading) {
        return (
            <div className="min-h-screen bg-gray-100 pb-10">
                <Navbar />
                <div className="text-center py-20 text-gray-500">Đang tải thông tin...</div>
            </div>
        );
    }

    if (!userInfo) {
        return (
            <div className="min-h-screen bg-gray-100 pb-10">
                <Navbar />
                <div className="max-w-4xl mx-auto mt-10 bg-white p-10 text-center text-gray-500 shadow-sm rounded-xl">
                    Không tìm thấy người dùng này.
                    <button onClick={() => navigate('/')} className="block mx-auto mt-4 text-blue-600 hover:underline">
                        Quay về trang chủ
                    </button>
                </div>
            </div>
        );
    }

    return (
        <div className="min-h-screen bg-gray-100 pb-10">
            <Navbar />

            {/* Nút quay lại */}
            <div className="max-w-4xl mx-auto mt-4 px-4 sm:px-0">
                <button
                    onClick={() => navigate(-1)}
                    className="flex items-center text-gray-500 hover:text-blue-600 transition font-medium mb-2"
                >
                    <ArrowLeft size={18} className="mr-1" /> Quay lại
                </button>
            </div>

            {/* KHUNG CHỨA TOÀN BỘ HEADER */}
            <div className="max-w-4xl mx-auto bg-white shadow-md rounded-b-xl rounded-t-xl overflow-hidden">
                <div className="h-64 bg-gradient-to-r from-blue-500 to-purple-600 w-full relative">
                    {isMyProfile && (
                        <button className="absolute bottom-4 right-4 bg-white/20 hover:bg-white/40 text-white px-4 py-2 rounded-lg backdrop-blur-sm transition font-medium">
                            Đổi ảnh bìa
                        </button>
                    )}
                </div>

                <div className="px-8 pb-8 relative">
                    <div className="absolute -top-16 left-8 w-32 h-32 bg-white rounded-full p-1 shadow-lg">
                        <div className="w-full h-full bg-blue-600 rounded-full flex items-center justify-center text-5xl font-bold text-white border-4 border-white">
                            {userInfo?.displayName?.charAt(0).toUpperCase() || 'U'}
                        </div>
                    </div>

                    <div className="flex justify-end pt-4">
                        {isMyProfile ? (
                            <button className="bg-gray-200 hover:bg-gray-300 text-gray-800 px-4 py-2 rounded-lg font-medium transition">
                                Chỉnh sửa trang cá nhân
                            </button>
                        ) : friendshipStatus === 'Accepted' ? (
                            <button onClick={() => handleUnfriendOrCancel('hủy kết bạn')} className="bg-gray-200 hover:bg-red-500 hover:text-white text-gray-800 px-4 py-2 rounded-lg font-medium transition flex items-center">
                                <UserMinus size={18} className="mr-2" /> Bạn bè (Hủy)
                            </button>
                        ) : friendshipStatus === 'Pending' ? (
                            requesterId === currentUser?.id ? (
                                <button onClick={() => handleUnfriendOrCancel('hủy lời mời')} className="bg-gray-200 hover:bg-gray-300 text-gray-800 px-4 py-2 rounded-lg font-medium transition flex items-center">
                                    <Clock size={18} className="mr-2" /> Đã gửi lời mời (Hủy)
                                </button>
                            ) : (
                                <button onClick={handleAcceptRequest} className="bg-green-600 hover:bg-green-700 text-white px-4 py-2 rounded-lg font-medium transition flex items-center">
                                    <Check size={18} className="mr-2" /> Xác nhận kết bạn
                                </button>
                            )
                        ) : (
                            <button onClick={handleSendRequest} className="bg-blue-600 hover:bg-blue-700 text-white px-4 py-2 rounded-lg font-medium transition flex items-center">
                                <UserPlus size={18} className="mr-2" /> Thêm bạn bè
                            </button>
                        )}
                    </div>

                    <div className="mt-2">
                        <h1 className="text-3xl font-bold text-gray-900">{userInfo?.displayName}</h1>
                        <p className="text-gray-500 font-medium mt-1">
                            {isMyProfile ? 'Lập trình viên' : 'Thành viên InteractHub'}
                        </p>
                    </div>

                    <div className="mt-6 pt-6 border-t flex flex-wrap gap-6 text-gray-600">
                        <div className="flex items-center">
                            <Mail className="w-5 h-5 mr-2 text-gray-400" />
                            {userInfo?.email || 'Chưa cập nhật'}
                        </div>
                        <div className="flex items-center">
                            <MapPin className="w-5 h-5 mr-2 text-gray-400" />
                            Hà Nội, Việt Nam
                        </div>
                        <div className="flex items-center">
                            <Calendar className="w-5 h-5 mr-2 text-gray-400" />
                            Đã tham gia tháng 4/2026
                        </div>
                    </div>
                </div>
            </div>

            {/* KHU VỰC BÀI VIẾT CỦA USER */}
            <div className="max-w-2xl mx-auto mt-6">
                <h2 className="text-xl font-bold text-gray-800 mb-4 px-2">Bài viết của {userInfo?.displayName}</h2>

                {isLoadingPosts ? (
                    <div className="text-center text-gray-500 bg-white p-10 rounded-xl shadow-sm">Đang tải bài viết...</div>
                ) : userPosts.length === 0 ? (
                    <div className="text-center text-gray-500 bg-white p-10 rounded-xl shadow-sm">
                        {isMyProfile ? 'Bạn chưa có bài viết nào.' : 'Người dùng này chưa có bài viết nào.'}
                    </div>
                ) : (
                    userPosts.map((post) => (
                        <PostItem
                            key={post.id}
                            post={post}
                            onPostUpdated={fetchUserPosts}
                        />
                    ))
                )}
            </div>
        </div>
    );
}