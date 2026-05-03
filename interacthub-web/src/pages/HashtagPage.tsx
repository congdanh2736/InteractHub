import { useEffect, useState } from 'react';
import { useParams, useNavigate } from 'react-router-dom';
import axiosClient from '../api/axiosClient';
import Navbar from '../components/Navbar';
import PostItem from '../components/PostItem';
import TrendingHashtags from '../components/TrendingHashtags';
import { Hash, ArrowLeft } from 'lucide-react';
import * as React from 'react';

export default function HashtagPage() {
    const { tag } = useParams<{ tag: string }>(); // Ví dụ: lấy ra chữ "React" từ URL /hashtag/React
    const navigate = useNavigate();

    const [posts, setPosts] = useState<any[]>([]);
    const [isLoading, setIsLoading] = useState(true);

    const fetchPostsByHashtag = async () => {
        setIsLoading(true);
        try {
            // Khi gọi API, chúng ta cần Encode dấu # thành %23 để backend không nhầm với Hash URL
            const response = await axiosClient.get('/Posts', {
                params: {
                    keyword: `#${tag}`,
                    pageNumber: 1,
                    pageSize: 50
                }
            });

            let fetchedPosts = [];
            if (Array.isArray(response.data)) {
                fetchedPosts = response.data;
            } else if (response.data && Array.isArray(response.data.items)) {
                fetchedPosts = response.data.items;
            } else if (response.data && Array.isArray(response.data.$values)) {
                fetchedPosts = response.data.$values;
            }

            setPosts(fetchedPosts);
        } catch (error) {
            console.error('Lỗi khi tải bài viết:', error);
        } finally {
            setIsLoading(false);
        }
    };

    // Tự động tải lại khi "tag" trên URL thay đổi
    useEffect(() => {
        fetchPostsByHashtag();
    }, [tag]);

    return (
        <div className="min-h-screen bg-gray-100 pb-10">
            <Navbar />
            <div className="max-w-5xl mx-auto mt-6 px-4 flex flex-col md:flex-row gap-6">
                <div className="md:w-2/3 w-full">

                    {/* Nút quay lại */}
                    <button
                        onClick={() => navigate(-1)}
                        className="flex items-center text-gray-500 hover:text-blue-600 transition font-medium mb-4"
                    >
                        <ArrowLeft size={18} className="mr-1" /> Quay lại
                    </button>

                    {/* Header chủ đề Hashtag */}
                    <div className="bg-white rounded-xl shadow-sm p-6 mb-6 flex items-center bg-gradient-to-r from-blue-50 to-indigo-50 border border-blue-100">
                        <div className="w-16 h-16 bg-blue-600 text-white rounded-full flex items-center justify-center shadow-lg mr-4 flex-shrink-0">
                            <Hash size={32} />
                        </div>
                        <div>
                            <h1 className="text-2xl font-bold text-gray-800">#{tag}</h1>
                            <p className="text-gray-500 mt-1">Các bài viết có gắn thẻ này</p>
                        </div>
                    </div>

                    <div className="max-w-2xl mx-auto mt-6">
                        {isLoading ? (
                            <div className="text-center py-10">
                                <div className="animate-spin inline-block w-8 h-8 border-[3px] border-current border-t-transparent text-blue-600 rounded-full"></div>
                                <p className="text-gray-500 mt-3">Đang tìm kiếm bài viết...</p>
                            </div>
                        ) : posts.length === 0 ? (
                            <div className="text-center text-gray-500 mt-2 p-10 bg-white rounded-xl shadow-sm border border-gray-100">
                                <Hash size={40} className="mx-auto text-gray-300 mb-3" />
                                Chưa có bài viết nào chứa thẻ <strong className="text-gray-700">#{tag}</strong>
                            </div>
                        ) : (
                            posts.map((post) => (
                                <PostItem key={post.id} post={post} onPostUpdated={fetchPostsByHashtag} />
                            ))
                        )}
                    </div>
                </div>

                {/* Giữ lại cột Xu hướng bên phải */}
                <div className="md:w-1/3 w-full hidden md:block">
                    <TrendingHashtags />
                </div>
            </div>
        </div>
    );
}