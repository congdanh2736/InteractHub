import { useEffect, useState } from 'react';
import Navbar from '../components/Navbar';
import { Mail, MapPin, Calendar } from 'lucide-react';
import axiosClient from '../api/axiosClient';
import PostItem from '../components/PostItem';
import type { PostType } from '../components/PostItem';
import { useAuth } from '../context/AuthContext'; // 1. Bắt sóng Auth

export default function Profile() {
    const { user } = useAuth(); // 2. Lấy luôn thông tin User (đã giải mã sẵn ở Context)

    const [userPosts, setUserPosts] = useState<PostType[]>([]);
    const [isLoadingPosts, setIsLoadingPosts] = useState(false);

    const fetchUserPosts = async (userId: string) => {
        try {
            const response = await axiosClient.get(`/Posts/user/${userId}`);

            console.log("Dữ liệu Posts trả về từ API:", response);

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
            console.error("Lỗi khi tải bài viết của người dùng:", error);
        } finally {
            setIsLoadingPosts(false);
        }
    };

    // 3. Rất gọn gàng: Nếu có user.id thì đi lấy bài viết!
    useEffect(() => {
        if (user?.id) {
            fetchUserPosts(user?.id);
        }
    }, [user?.id]);


    return (
        <div className="min-h-screen bg-gray-100 pb-10">
            <Navbar />

            {/* KHUNG CHỨA TOÀN BỘ HEADER */}
            <div className="max-w-4xl mx-auto bg-white shadow-md rounded-b-xl overflow-hidden">
                <div className="h-64 bg-gradient-to-r from-blue-500 to-purple-600 w-full relative">
                    <button className="absolute bottom-4 right-4 bg-white/20 hover:bg-white/40 text-white px-4 py-2 rounded-lg backdrop-blur-sm transition font-medium">
                        Đổi ảnh bìa
                    </button>
                </div>

                <div className="px-8 pb-8 relative">
                    <div className="absolute -top-16 left-8 w-32 h-32 bg-white rounded-full p-1 shadow-lg">
                        <div className="w-full h-full bg-blue-600 rounded-full flex items-center justify-center text-5xl font-bold text-white border-4 border-white">
                            {/* Dùng user.name từ Context */}
                            {user?.name?.charAt(0).toUpperCase() || 'U'}
                        </div>
                    </div>

                    <div className="flex justify-end pt-4">
                        <button className="bg-gray-200 hover:bg-gray-300 text-gray-800 px-4 py-2 rounded-lg font-medium transition">
                            Chỉnh sửa trang cá nhân
                        </button>
                    </div>

                    <div className="mt-2">
                        <h1 className="text-3xl font-bold text-gray-900">{user?.name}</h1>
                        <p className="text-gray-500 font-medium mt-1">Lập trình viên</p>
                    </div>

                    <div className="mt-6 pt-6 border-t flex flex-wrap gap-6 text-gray-600">
                        <div className="flex items-center">
                            <Mail className="w-5 h-5 mr-2 text-gray-400" />
                            {user?.email || 'Chưa cập nhật'}
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

            {/* KHU VỰC BÀI VIẾT CỦA TÔI */}
            <div className="max-w-2xl mx-auto mt-6">
                <h2 className="text-xl font-bold text-gray-800 mb-4 px-2">Bài viết của {user?.name || 'Người dùng'}</h2>

                {isLoadingPosts ? (
                    <div className="text-center text-gray-500 bg-white p-10 rounded-xl shadow-sm">Đang tải bài viết...</div>
                ) : userPosts.length === 0 ? (
                    <div className="text-center text-gray-500 bg-white p-10 rounded-xl shadow-sm">Bạn chưa có bài viết nào.</div>
                ) : (
                    userPosts.map((post) => (
                        <PostItem
                            key={post.id}
                            post={post}
                            onPostUpdated={() => {
                                if (user?.id) fetchUserPosts(user.id);
                            }}
                        />
                    ))
                )}
            </div>
        </div>
    );
}