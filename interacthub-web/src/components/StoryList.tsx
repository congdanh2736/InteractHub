import { Plus } from 'lucide-react';
import { useAuth } from '../context/AuthContext';
import { useState, useEffect } from 'react';
import { toast } from 'react-toastify';
import axiosClient from '../api/axiosClient';
import * as React from 'react';

export interface StoryType {
    id: number;
    imageUrl: string;
    userId: string;
    createdAt: string;
}

export default function StoryList() {
    const { user } = useAuth();
    const [stories, setStories] = useState<StoryType[]>([]);

    const fetchStories = async () => {
        try {
            const response = await axiosClient.get('/Stories');
            setStories(response.data);
        } catch (error) {
            console.log("Loi khi tai story: ", error);
        }
    };

    useEffect(() => {
        fetchStories();
    }, []);

    // ham tao story moi
    const handleCreateStory = async () => {
        // Tạm thời dùng hộp thoại để nhập URL ảnh cho nhanh
        const imageUrl = window.prompt("Nhập đường dẫn ảnh (URL) cho Story của bạn:\n(Ví dụ: https://picsum.photos/200/300)");

        if (!imageUrl) return; // Nếu bấm Hủy hoặc không nhập thì thôi

        try {
            await axiosClient.post('/Stories', {
                imageUrl: imageUrl
            });
            toast.success("Đăng Story thành công!");
            fetchStories(); // Tải lại danh sách Story ngay lập tức
        } catch (error) {
            console.error(error);
            toast.error("Đăng Story thất bại!");
        }
    };

    return (
        <div className="bg-white rounded-xl shadow-sm p-4 mb-6 overflow-hidden">
            {/* Dải cuộn ngang */}
            <div className="flex space-x-4 overflow-x-auto pb-2 scrollbar-hide">

                {/* NÚT TẠO TIN CỦA TÔI */}
                <div
                    onClick={handleCreateStory}
                    className="flex flex-col items-center cursor-pointer min-w-[80px] hover:scale-105 transition"
                >
                    <div className="relative w-16 h-16 rounded-full ring-2 ring-gray-200 p-1">
                        <div className="w-full h-full bg-gray-100 rounded-full flex items-center justify-center text-blue-600 font-bold text-xl">
                            {user?.name?.charAt(0).toUpperCase() || 'U'}
                        </div>
                        <div className="absolute bottom-0 right-0 bg-blue-600 text-white rounded-full p-1 ring-2 ring-white">
                            <Plus size={16} />
                        </div>
                    </div>
                    <span className="text-xs font-bold text-gray-800 mt-2 text-center">Tạo tin</span>
                </div>

                {/* DANH SÁCH TIN (DỮ LIỆU THẬT TỪ API) */}
                {stories.map((story) => (
                    <div key={story.id} className="flex flex-col items-center cursor-pointer min-w-[80px] hover:scale-105 transition">
                        <div className="w-16 h-16 rounded-full ring-2 ring-blue-500 p-1">
                            {story.imageUrl ? (
                                <img
                                    src={story.imageUrl}
                                    alt="Story"
                                    className="w-full h-full rounded-full object-cover"
                                    onError={(e) => {
                                        // Nếu link ảnh bị lỗi/chết, hiển thị màu xám dự phòng
                                        (e.target as HTMLImageElement).src = 'https://via.placeholder.com/100?text=Lỗi+Ảnh';
                                    }}
                                />
                            ) : (
                                <div className="w-full h-full bg-blue-100 text-blue-600 rounded-full flex items-center justify-center font-bold text-xl">
                                    {story.user?.displayName?.charAt(0).toUpperCase() || 'F'}
                                </div>
                            )}
                        </div>
                        <span className="text-xs font-medium text-gray-800 mt-2 truncate w-[70px] text-center">
                            {story.user?.displayName || 'Người dùng'}
                        </span>
                    </div>
                ))}

                {/* Nếu chưa có ai đăng Story thì hiện thông báo nhỏ */}
                {stories.length === 0 && (
                    <div className="flex items-center justify-center px-4 text-sm text-gray-400 italic">
                        Chưa có tin nào mới. Hãy là người đầu tiên!
                    </div>
                )}
            </div>
        </div>
    );
}