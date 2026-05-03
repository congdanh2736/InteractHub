import { useEffect, useState } from 'react';
import axiosClient from '../api/axiosClient';
import { TrendingUp } from 'lucide-react';
import { useNavigate } from 'react-router-dom';
import * as React from 'react';

export interface HashtagType {
    id: number;
    name: string;
    usageCount: number;
}


export default function TrendingHashtags() {
    const [hashtags, setHashtags] = useState<HashtagType[]>([]);
    const navigate = useNavigate();

    useEffect(() => {
        const fetchTrending = async () => {
            try {
                const res = await axiosClient.get('/Hashtags/trending');
                setHashtags(res.data);
            } catch (error) {
                console.error("Lỗi lấy hashtag", error);
            }
        };
        fetchTrending();
    }, []);

    if (hashtags.length === 0) return null;

    return (
        <div className="bg-white rounded-xl shadow-sm p-4 sticky top-20">
            <div className="flex items-center mb-4 border-b pb-2">
                <TrendingUp className="text-blue-600 mr-2" size={20} />
                <h3 className="font-bold text-gray-800 text-lg">Xu hướng thịnh hành</h3>
            </div>

            <div className="space-y-1">
                {hashtags.map((tag, index) => (
                    <div
                        key={tag.id}
                        // Cắt bỏ dấu "#" để truyền lên thanh địa chỉ cho đẹp
                        onClick={() => navigate(`/hashtag/${tag.name.replace('#', '')}`)}
                        className="flex justify-between items-center cursor-pointer hover:bg-gray-50 p-3 rounded-lg transition"
                    >
                        <div>
                            <p className="font-bold text-gray-800">{tag.name}</p>
                            <p className="text-xs text-gray-500">{tag.usageCount} bài viết</p>
                        </div>
                        <span className="text-gray-300 font-bold text-sm">#{index + 1}</span>
                    </div>
                ))}
            </div>
        </div>
    );
}