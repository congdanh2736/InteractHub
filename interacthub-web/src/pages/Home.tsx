import * as signalR from '@microsoft/signalr';
import { useEffect, useState, useRef } from 'react';
import axiosClient from '../api/axiosClient';
import Navbar from '../components/Navbar';
import { Image as ImageIcon, Send, X } from 'lucide-react';
import { toast } from 'react-toastify';
import PostItem from '../components/PostItem';
import StoryList from '../components/StoryList';
import TrendingHashtags from '../components/TrendingHashtags';
import { useAuth } from '../context/AuthContext';
import { useNavigate } from 'react-router-dom';
import * as React from 'react';

export interface Comment {
    id: number;
    postId: number;
    userId: string;
    content: string;
    createdAt: string;
}

export interface Post {
    id: number;
    content: string;
    imageUrl?: string;
    userId: string;
    userDisplayName?: string;
    createdAt: string;
    likesCount: number;
    commentsCount: number;
    isLiked?: boolean;
    comments?: Comment[];
}

export default function Home() {
    const { user } = useAuth();

    const [posts, setPosts] = useState<Post[]>([]);
    const [content, setContent] = useState('');
    const [isLoading, setIsLoading] = useState(true);

    // --- 1. THÊM STATE CHO PHÂN TRANG ---
    const [page, setPage] = useState(1);
    const [hasMore, setHasMore] = useState(true); // Cờ kiểm tra xem còn bài viết nào để tải không
    const [isLoadingMore, setIsLoadingMore] = useState(false);

    const [imageFile, setImageFile] = useState<File | null>(null);
    const [imagePreview, setImagePreview] = useState<string | null>(null);
    const [isPosting, setIsPosting] = useState(false);

    const fileInputRef = useRef<HTMLInputElement>(null);

    const navigate = useNavigate();

    // --- 2. SỬA LẠI HÀM FETCH POSTS ĐỂ NHẬN THAM SỐ PAGE ---
    const fetchPosts = async (pageNumber = 1) => {
        try {
            if (pageNumber === 1) setIsLoading(true);
            else setIsLoadingMore(true);

            // Gửi params ?page=1&pageSize=5 lên Backend
            const response = await axiosClient.get('/Posts', {
                params: { pageNumber: pageNumber, pageSize: 5 }
            });

            let fetchedPosts: Post[] = [];

            if (Array.isArray(response.data)) {
                fetchedPosts = response.data;
            } else if (response.data && Array.isArray(response.data.data)) {
                fetchedPosts = response.data.data;
            } else if (response.data && Array.isArray(response.data.items)) {
                fetchedPosts = response.data.items;
            } else if (response.data && Array.isArray(response.data.$values)) {
                fetchedPosts = response.data.$values;
            }

            // Nếu Backend trả về ít hơn 5 bài, nghĩa là đã hết bài viết
            if (fetchedPosts.length < 5) {
                setHasMore(false);
            } else {
                setHasMore(true);
            }

            // Nếu là trang 1 thì ghi đè, nếu trang 2, 3 thì nối tiếp vào danh sách cũ
            if (pageNumber === 1) {
                setPosts([...fetchedPosts]);
            } else {
                setPosts(prevPosts => [...prevPosts, ...fetchedPosts]);
            }

        } catch (error) {
            console.error('Lỗi khi tải bài viết:', error);
            toast.error('Không thể tải danh sách bài viết!');
        } finally {
            setIsLoading(false);
            setIsLoadingMore(false);
        }
    };

    // Khi component vừa load, tải trang 1
    useEffect(() => {
        fetchPosts(1);
    }, []);

    // --- 3. HÀM XỬ LÝ KHI BẤM NÚT "XEM THÊM" ---
    const handleLoadMore = () => {
        if (!isLoadingMore && hasMore) {
            const nextPage = page + 1;
            setPage(nextPage);
            fetchPosts(nextPage);
        }
    };

    // ... (Giữ nguyên đoạn code SignalR của bạn) ...
    //useEffect(() => {
    //    const token = localStorage.getItem('token');
    //    if (!token) return;

    //    const connection = new signalR.HubConnectionBuilder()
    //        .withUrl("https://localhost:7061/notificationHub", { accessTokenFactory: () => token })
    //        .withAutomaticReconnect()
    //        .build();

    //    connection.start().catch(err => console.log("Loi ket noi signalR", err));

    //    const fetchSinglePost = async (postId: number) => {
    //        try {
    //            const response = await axiosClient.get(`/Posts/${postId}`);
    //            const updatePost = response.data;
    //            setPosts(prevPosts => prevPosts.map(post => post.id === postId ? updatePost : post));
    //        } catch (error) { console.error("Lỗi", error); }
    //    }

    //    connection.on("ReceiveNewPost", () => {
    //        toast.info("Có người vừa đăng bài viết mới");
    //        // Khi có bài mới, tải lại từ đầu trang 1
    //        setPage(1);
    //        fetchPosts(1);
    //    });
    //    connection.on("ReceiveNewComment", fetchSinglePost);
    //    connection.on("ReceiveLikeUpdate", fetchSinglePost);
    //    connection.on("ReceiveNotification", (notification) => {
    //        toast.success(notification.message);
    //    });

    //    return () => { connection.stop(); };
    //}, []);

    const handleImageSelect = (e: React.ChangeEvent<HTMLInputElement>) => {
        const file = e.target.files?.[0];
        if (file) {
            setImageFile(file);
            setImagePreview(URL.createObjectURL(file));
        }
    };

    const removeImage = () => {
        setImageFile(null);
        setImagePreview(null);
        if (fileInputRef.current) fileInputRef.current.value = '';
    };

    const handleCreatePost = async () => {
        if (!content.trim() && !imageFile) return;
        setIsPosting(true);
        try {
            let imageUrl = '';
            if (imageFile) {
                const formData = new FormData();
                formData.append('file', imageFile);
                const uploadRes = await axiosClient.post('/Files/upload', formData, {
                    headers: { 'Content-Type': 'multipart/form-data' }
                });
                imageUrl = uploadRes.data.url;
            }

            await axiosClient.post('/Posts', { content, imageUrl });
            toast.success('Đăng bài thành công! 🎉');
            setContent('');
            removeImage();

            // Đăng xong tải lại trang 1
            setPage(1);
            fetchPosts(1);
        } catch (error) {
            toast.error('Đăng bài thất bại!');
        } finally {
            setIsPosting(false);
        }
    };

    const goToProfile = (userId: string) => {
        // Nếu là mình thì về trang cá nhân của mình, ngược lại sang xem người khác
        if (userId === user?.id) {
            navigate('/profile');
        } else {
            navigate(`/profile/${userId}`);
        }
    };

    return (
        <div className="min-h-screen bg-gray-100 pb-10">
            <Navbar />
            <div className="max-w-5xl mx-auto mt-6 px-4 flex flex-col md:flex-row gap-6">
                <div className="md:w-2/3 w-full">
                    <StoryList />

                    <div className="max-w-2xl mx-auto mt-6 px-4">
                        <div className="bg-white p-4 rounded-xl shadow-sm mb-6">
                            <div className="flex space-x-3">
                                <div
                                    onClick={() => goToProfile(user?.id)}   
                                    className="w-10 h-10 bg-blue-600 text-white rounded-full flex items-center justify-center font-bold flex-shrink-0 cursor-pointer hover:bg-blue-500"
                                >
                                    {user?.name?.charAt(0).toUpperCase() || 'U'}
                                </div>
                                <input
                                    type="text" placeholder="Bạn đang nghĩ gì thế?"
                                    className="bg-gray-100 w-full rounded-full px-4 outline-none focus:bg-gray-200 transition"
                                    value={content} onChange={(e) => setContent(e.target.value)}
                                    onKeyDown={(e) => e.key === 'Enter' && !isPosting && handleCreatePost()}
                                />
                            </div>

                            {imagePreview && (
                                <div className="relative mt-4 ml-13">
                                    <img src={imagePreview} alt="Preview" className="max-h-64 rounded-lg object-cover" />
                                    <button onClick={removeImage} className="absolute top-2 right-2 bg-gray-800 bg-opacity-70 text-white p-1 rounded-full hover:bg-red-500 transition"><X size={16} /></button>
                                </div>
                            )}

                            <div className="border-t mt-4 pt-3 flex justify-between items-center">
                                <input type="file" accept="image/*" className="hidden" ref={fileInputRef} onChange={handleImageSelect} />
                                <button onClick={() => fileInputRef.current?.click()} className="flex items-center text-gray-500 hover:bg-gray-100 px-3 py-2 rounded-lg transition">
                                    <ImageIcon size={20} className="text-green-500 mr-2" /> Ảnh
                                </button>
                                <button onClick={handleCreatePost} disabled={isPosting} className={`text-white px-4 py-2 rounded-lg font-medium flex items-center transition ${isPosting ? 'bg-blue-400 cursor-not-allowed' : 'bg-blue-600 hover:bg-blue-700'}`}>
                                    <Send size={16} className="mr-2" /> {isPosting ? 'Đang đăng...' : 'Đăng bài'}
                                </button>
                            </div>
                        </div>

                        {isLoading ? (
                            <div className="text-center py-10">
                                <div className="animate-spin inline-block w-8 h-8 border-[3px] border-current border-t-transparent text-blue-600 rounded-full" role="status"></div>
                                <p className="text-gray-500 mt-3">Đang tải bảng tin...</p>
                            </div>
                        ) : (!Array.isArray(posts) || posts.length === 0) ? (
                            <div className="text-center text-gray-500 mt-10 p-10 bg-white rounded-xl shadow-sm">Chưa có bài viết nào. Hãy là người đầu tiên đăng bài!</div>
                        ) : (
                            <>
                                {posts.map((post) => (
                                    <PostItem key={post.id} post={post} onPostUpdated={() => {
                                        // Khi like/comment, chỉ reload lại trang 1 (hoặc bạn có thể tối ưu hơn sau này)
                                        setPage(1);
                                        fetchPosts(1);
                                    }} />
                                ))}

                                {/* --- 4. NÚT XEM THÊM (LOAD MORE) --- */}
                                {hasMore && (
                                    <div className="text-center mt-6">
                                        <button
                                            onClick={handleLoadMore}
                                            disabled={isLoadingMore}
                                            className="px-6 py-2 bg-white border border-gray-300 text-gray-700 rounded-full shadow-sm hover:bg-gray-50 transition disabled:opacity-50"
                                        >
                                            {isLoadingMore ? 'Đang tải thêm...' : 'Xem thêm bài viết'}
                                        </button>
                                    </div>
                                )}

                                {!hasMore && posts.length > 0 && (
                                    <div className="text-center mt-6 text-gray-500 text-sm">
                                        Đã hiển thị hết bài viết.
                                    </div>
                                )}
                            </>
                        )}
                    </div>
                </div>

                <div className="md:w-1/3 w-full hidden md:block">
                    <TrendingHashtags />
                </div>
            </div>
        </div>
    );
}