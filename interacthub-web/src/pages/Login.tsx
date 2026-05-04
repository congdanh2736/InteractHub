import { useNavigate, Link } from 'react-router-dom';
import axios from 'axios';
import { useForm } from 'react-hook-form'; // Thêm thư viện React Hook Form
import axiosClient from '../api/axiosClient';
import { toast } from 'react-toastify';
import { useAuth } from '../context/AuthContext'; // Import Context
import * as React from 'react';

// Khai báo kiểu dữ liệu cho Form
interface LoginFormInputs {
    email: string;
    password: string;
}

export default function Login() {
    const navigate = useNavigate();
    const { login } = useAuth(); // Lấy hàm login từ AuthContext

    // Khởi tạo React Hook Form
    const {
        register,
        handleSubmit,
        formState: { errors, isSubmitting }
    } = useForm<LoginFormInputs>();

    // Xử lý khi submit form
    const onSubmit = async (data: LoginFormInputs) => {
        try {
            // Gọi API sang C#
            const response = await axiosClient.post('/Auth/login', {
                email: data.email,
                password: data.password
            });

            login(response.data.token);

            toast.success('Đăng nhập thành công! 🎉');
            navigate('/');
        } catch (error) {
            if (axios.isAxiosError(error)) {
                toast.error(error.response?.data?.message || 'Sai tài khoản hoặc mật khẩu!');
            } else {
                toast.error('Đã xảy ra lỗi hệ thống!');
            }
        }
    };

    return (
        <div className="flex h-screen bg-gray-100 items-center justify-center">
            <div className="flex flex-col md:flex-row w-full max-w-4xl bg-white shadow-xl rounded-lg overflow-hidden">

                {/* Cột trái: Lời chào */}
                <div className="w-full md:w-1/2 bg-blue-600 text-white p-10 flex flex-col justify-center">
                    <h1 className="text-4xl font-bold mb-4">InteractHub</h1>
                    <p className="text-lg text-blue-100">
                        Kết nối với bạn bè và thế giới xung quanh bạn trên InteractHub.
                    </p>
                </div>

                {/* Cột phải: Form đăng nhập */}
                <div className="w-full md:w-1/2 p-10">
                    <h2 className="text-2xl font-bold text-gray-800 mb-6">Đăng nhập</h2>

                    {/* Bọc handleSubmit của React Hook Form */}
                    <form onSubmit={handleSubmit(onSubmit)} className="space-y-4">
                        <div>
                            <input
                                type="email"
                                placeholder="Email của bạn"
                                className={`w-full px-4 py-3 border rounded-lg focus:outline-none focus:ring-1 ${errors.email ? 'border-red-500 focus:ring-red-500' : 'border-gray-300 focus:border-blue-500 focus:ring-blue-500'
                                    }`}
                                {...register('email', {
                                    required: 'Vui lòng nhập email',
                                    pattern: {
                                        value: /^[A-Z0-9._%+-]+@[A-Z0-9.-]+\.[A-Z]{2,}$/i,
                                        message: 'Email không đúng định dạng'
                                    }
                                })}
                            />
                            {/* Hiển thị lỗi nếu có */}
                            {errors.email && <p className="text-red-500 text-sm mt-1">{errors.email.message}</p>}
                        </div>

                        <div>
                            <input
                                type="password"
                                placeholder="Mật khẩu"
                                className={`w-full px-4 py-3 border rounded-lg focus:outline-none focus:ring-1 ${errors.password ? 'border-red-500 focus:ring-red-500' : 'border-gray-300 focus:border-blue-500 focus:ring-blue-500'
                                    }`}
                                {...register('password', {
                                    required: 'Vui lòng nhập mật khẩu',
                                    minLength: { value: 6, message: 'Mật khẩu phải từ 6 ký tự' }
                                })}
                            />
                            {errors.password && <p className="text-red-500 text-sm mt-1">{errors.password.message}</p>}
                        </div>

                        <button
                            type="submit"
                            disabled={isSubmitting} // Disable nút khi đang gọi API
                            className={`w-full text-white font-bold py-3 rounded-lg transition duration-200 ${isSubmitting ? 'bg-blue-400 cursor-not-allowed' : 'bg-blue-600 hover:bg-blue-700'
                                }`}
                        >
                            {isSubmitting ? 'Đang đăng nhập...' : 'Đăng nhập'}
                        </button>
                    </form>

                    <div className="mt-6 text-center border-t pt-4">
                        <Link to="/register">
                            <button className="bg-green-500 text-white font-bold py-3 px-6 rounded-lg hover:bg-green-600 transition duration-200">
                                Tạo tài khoản mới
                            </button>
                        </Link>
                    </div>
                </div>
            </div>
        </div>
    );
}