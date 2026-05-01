import { useState } from 'react';
import { ThumbsUp, MessageCircle, MoreHorizontal, Flag, Send } from 'lucide-react';
import { useAuth } from '../context/AuthContext';
import { toast } from 'react-toastify';
import axiosClient from '../api/axiosClient';
import { useNavigate } from 'react-router-dom'; // Thêm import chuyển hướng

export interface CommentType {
    id: number | string;
    userId: string;
    content: string;
    userDisplayName?: string; // Thêm trường tên người dùng
}

export interface PostType {
    id: number;
    userId: string;
    userDisplayName?: string;
    createdAt: string;
    content: string;
    imageUrl?: string;
    isLiked?: boolean;
    likesCount?: number;
    commentsCount?: number;
    comments?: CommentType[];
}

interface PostItemProps {
    post: PostType;
    onPostUpdated?: () => void;
}

export default function PostItem({ post: initialPost, onPostUpdated }: PostItemProps) {
    const { user } = useAuth();
    const navigate = useNavigate(); // Khởi tạo công cụ chuyển hướng

    const [post, setPost] = useState<PostType>(initialPost);

    const [showMenu, setShowMenu] = useState(false);
    const [showReportModal, setShowReportModal] = useState(false);
    const [reportReason, setReportReason] = useState('');

    const [isCommentOpen, setIsCommentOpen] = useState(false);
    const [commentText, setCommentText] = useState('');

    const fetchSinglePost = async () => {
        try {
            const response = await axiosClient.get(`/Posts/${post.id}`);
            setPost(response.data);
        } catch (error) {
            console.error("Không thể tải lại bài viết: ", error);
        }
    };

    const handleReportPost = async () => {
        if (!reportReason) {
            toast.warning("Vui lòng nhập lý do báo cáo");
            return;
        }

        try {
            await axiosClient.post('/PostReports', {
                postId: post.id,
                reason: reportReason
            });
            toast.success("Đã gửi báo cáo thành công. Cảm ơn bạn!");
            setShowReportModal(false);
            setShowMenu(false);
            setReportReason('');
        } catch (error) {
            toast.error("Lỗi khi gửi báo cáo!");
        }
    }

    const handleLike = async () => {
        const wasLiked = post.isLiked;
        setPost(prev => ({
            ...prev,
            isLiked: !wasLiked,
            likesCount: (prev.likesCount || 0) + (wasLiked ? -1 : 1)
        }));

        try {
            await axiosClient.post('/Likes/toggle', { postId: post.id });
        } catch (error) {
            setPost(prev => ({
                ...prev,
                isLiked: wasLiked,
                likesCount: (prev.likesCount || 0) + (wasLiked ? 1 : -1)
            }));
            toast.error('Lỗi khi thả tim!');
        }
    };

    const handleSendComment = async () => {
        if (!commentText.trim()) return;
        try {
            await axiosClient.post('/Comments', { postId: post.id, content: commentText });
            toast.success('Đã gửi bình luận!');
            setCommentText('');
            await fetchSinglePost();
        } catch (error) {
            toast.error('Lỗi khi bình luận!');
        }
    };

    // Hàm tiện ích: Khi click vào thông tin người dùng sẽ đưa tới trang của họ
    const goToProfile = (userId: string) => {
        // Nếu là mình thì về trang cá nhân của mình, ngược lại sang xem người khác
        if (userId === user?.id) {
            navigate('/profile');
        } else {
            navigate(`/profile/${userId}`);
        }
    };

    const renderContent = (text: string) => {
        if (!text) return null;
        // Tách chuỗi dựa theo dấu #
        const parts = text.split(/(#\w+)/g);
        return parts.map((part, index) => {
            if (part.startsWith('#')) {
                const tagWord = part.replace('#', '');
                return (
                    <span
                        key={index}
                        onClick={(e) => {
                            e.stopPropagation(); // Ngăn chặn sự kiện click lan ra ngoài
                            navigate(`/hashtag/${tagWord}`);
                        }}
                        className="text-blue-600 hover:underline cursor-pointer font-medium"
                    >
                        {part}
                    </span>
                );
            }
            return <span key={index}>{part}</span>; // Từ bình thường (Không phải hashtag)
        });
    };

    return (
        <div className="bg-white p-4 rounded-xl shadow-sm mb-4">
            {/* Header bài viết */}
            <div className="flex items-center mb-3 justify-between">
                <div className="flex items-center">
                    {/* Bấm vào avatar người đăng */}
                    <div
                        onClick={() => goToProfile(post.userId)}
                        className="w-10 h-10 bg-blue-100 text-blue-600 rounded-full flex items-center justify-center font-bold mr-3 flex-shrink-0 cursor-pointer hover:bg-blue-200 transition"
                    >
                        {post.userDisplayName ? post.userDisplayName.substring(0, 1).toUpperCase() : 'U'}
                    </div>
                    <div>
                        {/* Bấm vào tên người đăng */}
                        <h3
                            onClick={() => goToProfile(post.userId)}
                            className="font-bold text-gray-800 cursor-pointer hover:text-blue-600 hover:underline transition"
                        >
                            {post.userDisplayName || post.userId || 'Người dùng'}
                        </h3>
                        <p className="text-xs text-gray-500">
                            {post.createdAt ? new Date(post.createdAt).toLocaleString('vi-VN') : 'Vừa xong'}
                        </p>
                    </div>
                </div>

                {/* NÚT MENU (3 CHẤM) */}
                <div className="relative">
                    <button
                        onClick={() => setShowMenu(!showMenu)}
                        className="p-2 text-gray-500 hover:bg-gray-100 rounded-full transition"
                    >
                        <MoreHorizontal size={20} />
                    </button>

                    {showMenu && (
                        <div className="absolute right-0 mt-2 w-48 bg-white border rounded-lg shadow-lg z-10 overflow-hidden">
                            <button
                                onClick={() => {
                                    setShowReportModal(true);
                                    setShowMenu(false);
                                }}
                                className="w-full text-left px-4 py-3 text-red-600 hover:bg-red-50 flex items-center transition"
                            >
                                <Flag size={16} className="mr-2" />
                                Báo cáo
                            </button>
                        </div>
                    )}
                </div>
            </div>

            {/* Nội dung bài viết */}
            <p className="text-gray-800 mb-3 whitespace-pre-wrap">{renderContent(post.content)}</p>
            {post.imageUrl && (
                <img
                    src={post.imageUrl.startsWith('http') ? post.imageUrl : `https://congdanh2703-001-site1.stempurl.com${post.imageUrl}`}
                    alt="Post"
                    className="w-full rounded-lg mb-3 object-cover max-h-96"
                />
            )}

            {/* NÚT THÍCH VÀ BÌNH LUẬN */}
            <div className="border-t border-b py-1 my-3 flex text-gray-500 font-medium text-sm">
                <button
                    onClick={handleLike}
                    className={`flex-1 py-2 flex items-center justify-center hover:bg-gray-100 rounded-lg transition ${post.isLiked ? 'text-blue-600' : ''}`}
                >
                    <ThumbsUp size={18} className="mr-2" /> {post.likesCount || 0} Thích
                </button>
                <button
                    onClick={() => setIsCommentOpen(!isCommentOpen)}
                    className="flex-1 py-2 flex items-center justify-center hover:bg-gray-100 rounded-lg transition"
                >
                    <MessageCircle size={18} className="mr-2" /> {post.commentsCount || 0} Bình luận
                </button>
            </div>

            {/* KHU VỰC BÌNH LUẬN */}
            {isCommentOpen && (
                <div className="mt-3">
                    <div className="flex space-x-2 mb-4">
                        <div className="w-8 h-8 bg-blue-600 text-white rounded-full flex items-center justify-center font-bold flex-shrink-0">
                            {user?.name?.charAt(0).toUpperCase() || 'U'}
                        </div>
                        <input
                            type="text" placeholder="Viết bình luận..."
                            className="bg-gray-100 w-full rounded-full px-4 py-2 outline-none focus:bg-gray-200 text-sm"
                            value={commentText}
                            onChange={(e) => setCommentText(e.target.value)}
                            onKeyDown={(e) => e.key === 'Enter' && handleSendComment()}
                        />
                        <button
                            onClick={handleSendComment}
                            className="bg-blue-100 text-blue-600 p-2 rounded-full hover:bg-blue-200 transition"
                        >
                            <Send size={16} />
                        </button>
                    </div>

                    {post.comments && post.comments.length > 0 && (
                        <div className="space-y-3">
                            {post.comments.map((cmt: CommentType) => (
                                <div key={cmt.id} className="flex space-x-2">
                                    {/* Bấm vào avatar người bình luận */}
                                    <div
                                        onClick={() => goToProfile(cmt.userId)}
                                        className="w-8 h-8 bg-gray-200 text-gray-600 rounded-full flex items-center justify-center font-bold text-xs flex-shrink-0 cursor-pointer hover:bg-gray-300 transition"
                                    >
                                        {cmt.userDisplayName ? cmt.userDisplayName.substring(0, 1).toUpperCase() : 'U'}
                                    </div>
                                    <div className="bg-gray-100 px-3 py-2 rounded-2xl text-sm max-w-[85%]">
                                        {/* Bấm vào tên người bình luận */}
                                        <span
                                            onClick={() => goToProfile(cmt.userId)}
                                            className="font-bold block text-gray-800 break-words cursor-pointer hover:text-blue-600 hover:underline transition"
                                        >
                                            {cmt.userDisplayName || cmt.userId}
                                        </span>
                                        <span className="text-gray-700 break-words">{cmt.content}</span>
                                    </div>
                                </div>
                            ))}
                        </div>
                    )}
                </div>
            )}

            {showReportModal && (
                <div className="fixed inset-0 bg-black/50 flex items-center justify-center z-50 p-4">
                    {/* ... (Code của Modal Report giữ nguyên) ... */}
                    <div className="bg-white rounded-xl p-6 w-full max-w-md shadow-2xl">
                        <h3 className="text-xl font-bold text-gray-900 mb-2">Báo cáo bài viết</h3>
                        <p className="text-gray-600 text-sm mb-4">Bạn thấy bài viết này có vấn đề gì?</p>

                        <textarea
                            className="w-full border rounded-lg p-3 text-gray-700 outline-none focus:ring-2 focus:ring-red-200 focus:border-red-500 min-h-[100px] mb-4"
                            placeholder="Ví dụ: Nội dung phản cảm, Spam, Bạo lực..."
                            value={reportReason}
                            onChange={(e) => setReportReason(e.target.value)}
                        />

                        <div className="flex justify-end space-x-3">
                            <button
                                onClick={() => setShowReportModal(false)}
                                className="px-4 py-2 text-gray-600 hover:bg-gray-100 rounded-lg transition"
                            >
                                Hủy
                            </button>
                            <button
                                onClick={handleReportPost}
                                className="px-4 py-2 bg-red-600 text-white rounded-lg hover:bg-red-700 font-medium transition"
                            >
                                Gửi báo cáo
                            </button>
                        </div>
                    </div>
                </div>
            )}
        </div>
    );
}