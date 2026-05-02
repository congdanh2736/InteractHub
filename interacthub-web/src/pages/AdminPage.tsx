import { useState, useEffect } from 'react';
import { Trash2, UserPlus, ShieldAlert, Users, AlertTriangle } from 'lucide-react';
import axiosClient from '../api/axiosClient';
import { toast } from 'react-toastify';
import { useAuth } from '../context/AuthContext';
import { useNavigate } from 'react-router-dom';

interface UserData {
    id: string;
    userName: string;
    displayName: string;
    email: string;
}

interface ReportData {
    id: number;
    postId: number;
    reason: string;
    createdAt: string;
    reporterId: string;
}

export default function AdminPage() {
    const { user } = useAuth();
    const navigate = useNavigate();
    const [activeTab, setActiveTab] = useState<'users' | 'reports'>('users');

    const [usersList, setUsersList] = useState<UserData[]>([]);
    const [reportsList, setReportsList] = useState<ReportData[]>([]);

    // Modal thêm user
    const [showAddUserModal, setShowAddUserModal] = useState(false);
    const [newUser, setNewUser] = useState({ email: '', password: '', displayName: '', userName: '' });

    // Kiểm tra role: Nếu backend hỗ trợ lưu Role trong Token và bạn lấy ra được, hãy bật cái này lên
    /* useEffect(() => {
        if (user && !user.roles?.includes('Admin')) {
            toast.error("Bạn không có quyền truy cập trang này!");
            navigate('/');
        }
    }, [user]); */

    useEffect(() => {
        if (activeTab === 'users') fetchUsers();
        if (activeTab === 'reports') fetchReports();
    }, [activeTab]);

    const fetchUsers = async () => {
        try {
            // Tạm thời gọi API tìm kiếm với keyword trắng vì backend chưa có API Get All Users
            const response = await axiosClient.get('/User'); 
            setUsersList(response.data);

            // Ghi chú: Để hoàn hảo, bạn cần viết thêm hàm [HttpGet] GetAllUsers() ở UserController backend
        } catch (error) {
            console.error(error);
            toast.error("Không thể tải danh sách người dùng. Backend cần hỗ trợ API GetAllUsers.");
        }
    };

    const fetchReports = async () => {
        try {
            // Gọi đúng endpoint lấy Reports theo PostReportController
            const response = await axiosClient.get('/PostReports');
            setReportsList(response.data);
        } catch (error) {
            console.error(error);
            toast.error("Không thể tải danh sách báo cáo. Đảm bảo tài khoản này là Admin.");
        }
    };

    const handleDeleteUser = async (userId: string) => {
        if (window.confirm("Bạn có chắc muốn xóa người dùng này?")) {
            try {
                // Backend hiện tại chưa có API xóa user ở UserController
                // Bạn cần thêm [HttpDelete("{id}")] DeleteUser() vào backend
                await axiosClient.delete(`/User/${userId}`);
                toast.success("Đã xóa người dùng thành công");
                fetchUsers();
            } catch (error) {
                toast.error("Lỗi khi xóa người dùng hoặc chưa có API bên backend.");
            }
        }
    };

    const handleAddUser = async () => {
        if (!newUser.email || !newUser.password || !newUser.displayName) {
            toast.warning("Vui lòng điền đủ thông tin (Email, Mật khẩu, Tên hiển thị)");
            return;
        }
        try {
            // Gọi đúng endpoint đăng ký theo AuthController
            await axiosClient.post('/Auth/register', newUser);
            toast.success("Tạo người dùng thành công");
            setShowAddUserModal(false);
            setNewUser({ email: '', password: '', displayName: '', userName: '' });
            fetchUsers();
        } catch (error: any) {
            console.error(error);
            toast.error("Lỗi khi tạo người dùng. Có thể email đã tồn tại.");
        }
    };

    return (
        <div className="min-h-screen bg-gray-50 p-6">
            <div className="max-w-6xl mx-auto">
                <h1 className="text-3xl font-bold text-gray-800 mb-6 flex items-center">
                    <ShieldAlert className="mr-3 text-blue-600" size={32} />
                    Bảng Điều Khiển Admin
                </h1>

                {/* Tabs */}
                <div className="flex space-x-4 mb-6 border-b pb-2">
                    <button
                        onClick={() => setActiveTab('users')}
                        className={`flex items-center px-4 py-2 font-medium rounded-t-lg transition ${activeTab === 'users' ? 'text-blue-600 border-b-2 border-blue-600' : 'text-gray-500 hover:text-gray-700'}`}
                    >
                        <Users className="mr-2" size={20} />
                        Quản lý Người Dùng
                    </button>
                    <button
                        onClick={() => setActiveTab('reports')}
                        className={`flex items-center px-4 py-2 font-medium rounded-t-lg transition ${activeTab === 'reports' ? 'text-blue-600 border-b-2 border-blue-600' : 'text-gray-500 hover:text-gray-700'}`}
                    >
                        <AlertTriangle className="mr-2" size={20} />
                        Quản lý Báo Cáo
                    </button>
                </div>

                {/* Nội dung Tab Quản lý Người dùng */}
                {activeTab === 'users' && (
                    <div className="bg-white rounded-xl shadow-sm p-6">
                        <div className="flex justify-between items-center mb-4">
                            <h2 className="text-xl font-semibold text-gray-800">Danh sách Người dùng</h2>
                            <button
                                onClick={() => setShowAddUserModal(true)}
                                className="flex items-center px-4 py-2 bg-blue-600 text-white rounded-lg hover:bg-blue-700 transition"
                            >
                                <UserPlus size={18} className="mr-2" /> Thêm User
                            </button>
                        </div>

                        <div className="overflow-x-auto">
                            <table className="w-full text-left border-collapse">
                                <thead>
                                    <tr className="bg-gray-100 text-gray-600 border-b">
                                        <th className="p-3">ID</th>
                                        <th className="p-3">Tên hiển thị</th>
                                        <th className="p-3">Email</th>
                                        <th className="p-3 text-center">Hành động</th>
                                    </tr>
                                </thead>
                                <tbody>
                                    {usersList.map((u) => (
                                        <tr key={u.id} className="border-b hover:bg-gray-50">
                                            <td className="p-3 text-sm text-gray-500 truncate max-w-[100px]">{u.id}</td>
                                            <td className="p-3 font-medium text-gray-800">{u.displayName || u.userName}</td>
                                            <td className="p-3 text-gray-600">{u.email}</td>
                                            <td className="p-3 text-center">
                                                <button
                                                    onClick={() => handleDeleteUser(u.id)}
                                                    className="text-red-500 hover:bg-red-100 p-2 rounded-lg transition"
                                                    title="Xóa người dùng"
                                                >
                                                    <Trash2 size={18} />
                                                </button>
                                            </td>
                                        </tr>
                                    ))}
                                    {usersList.length === 0 && (
                                        <tr>
                                            <td colSpan={4} className="p-4 text-center text-gray-500">Chưa có dữ liệu người dùng</td>
                                        </tr>
                                    )}
                                </tbody>
                            </table>
                        </div>
                    </div>
                )}

                {/* Nội dung Tab Quản lý Báo cáo */}
                {activeTab === 'reports' && (
                    <div className="bg-white rounded-xl shadow-sm p-6">
                        <h2 className="text-xl font-semibold text-gray-800 mb-4">Danh sách Báo cáo</h2>
                        <div className="space-y-4">
                            {reportsList.map((report) => (
                                <div key={report.id} className="border rounded-lg p-4 flex justify-between items-start bg-red-50/30">
                                    <div>
                                        <p className="font-medium text-gray-800">Bài viết ID: {report.postId}</p>
                                        <p className="text-gray-600 text-sm mt-1">
                                            <span className="font-semibold">Lý do:</span> {report.reason}
                                        </p>
                                        <p className="text-xs text-gray-500 mt-2">
                                            Người báo cáo: {report.reporterId} - Ngày: {new Date(report.createdAt).toLocaleString('vi-VN')}
                                        </p>
                                    </div>
                                    <button
                                        onClick={() => navigate(`/`)} // Có thể đổi lại link đi tới bài viết chi tiết
                                        className="text-sm px-3 py-1.5 bg-gray-200 text-gray-700 rounded hover:bg-gray-300 transition"
                                    >
                                        Xem bài viết
                                    </button>
                                </div>
                            ))}
                            {reportsList.length === 0 && (
                                <p className="text-center text-gray-500">Chưa có báo cáo nào</p>
                            )}
                        </div>
                    </div>
                )}
            </div>

            {/* Modal Thêm người dùng */}
            {showAddUserModal && (
                <div className="fixed inset-0 bg-black/50 flex items-center justify-center z-50 p-4">
                    <div className="bg-white rounded-xl p-6 w-full max-w-md shadow-2xl">
                        <h3 className="text-xl font-bold text-gray-900 mb-4">Thêm Người Dùng Mới</h3>

                        <div className="space-y-3 mb-6">
                            <input
                                type="text" placeholder="Tên hiển thị"
                                className="w-full border rounded-lg p-3 outline-none focus:ring-2 focus:ring-blue-200"
                                value={newUser.displayName}
                                onChange={(e) => setNewUser({ ...newUser, displayName: e.target.value })}
                            />
                            <input
                                type="email" placeholder="Email (dùng làm tài khoản luôn)"
                                className="w-full border rounded-lg p-3 outline-none focus:ring-2 focus:ring-blue-200"
                                value={newUser.email}
                                onChange={(e) => setNewUser({ ...newUser, email: e.target.value })}
                            />
                            <input
                                type="password" placeholder="Mật khẩu"
                                className="w-full border rounded-lg p-3 outline-none focus:ring-2 focus:ring-blue-200"
                                value={newUser.password}
                                onChange={(e) => setNewUser({ ...newUser, password: e.target.value })}
                            />
                        </div>

                        <div className="flex justify-end space-x-3">
                            <button
                                onClick={() => setShowAddUserModal(false)}
                                className="px-4 py-2 text-gray-600 hover:bg-gray-100 rounded-lg transition"
                            >
                                Hủy
                            </button>
                            <button
                                onClick={handleAddUser}
                                className="px-4 py-2 bg-blue-600 text-white rounded-lg hover:bg-blue-700 font-medium transition"
                            >
                                Tạo tài khoản
                            </button>
                        </div>
                    </div>
                </div>
            )}
        </div>
    );
}