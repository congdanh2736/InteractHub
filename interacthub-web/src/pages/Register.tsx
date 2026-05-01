import { useForm } from 'react-hook-form';
import { useNavigate, Link } from 'react-router-dom';
import { toast } from 'react-toastify';
import axiosClient from '../api/axiosClient';
import { AxiosError } from 'axios';

export interface RegisterForm {
    email: string;
    displayName: string;
    password: string;
}

export default function Register() {
    const navigate = useNavigate();

    // Khởi tạo công cụ React Hook Form
    const {
        register, // Dùng để gắn vào các ô input
        handleSubmit, // Dùng để bọc hàm gửi dữ liệu
        watch, // Dùng để theo dõi xem mật khẩu nhập là gì (để so sánh)
        formState: { errors, isSubmitting } // Chứa danh sách các lỗi và trạng thái đang load
    } = useForm();

    // Hàm này CHỈ CHẠY khi người dùng đã nhập đúng 100% luật lệ
    const onSubmit = async (data: RegisterForm) => {
        try {
            // Gọi API sang C# để tạo tài khoản
            await axiosClient.post('/Auth/register', {
                email: data.email,
                displayName: data.displayName,
                password: data.password
            });

            toast.success('Đăng ký thành công! Hãy đăng nhập.');
            navigate('/login'); // Đá sang trang đăng nhập
        } catch (err: unknown) {
            const error = err as AxiosError<{ message: string }>;
            console.log("🚨 LÝ DO C# TỪ CHỐI:", error.response?.data);
            toast.error(error.response?.data?.message || 'Đăng ký thất bại!');
        }
    };

    return (
        <div className="min-h-screen bg-gray-100 flex items-center justify-center">
            <div className="bg-white p-8 rounded-xl shadow-md w-full max-w-md">
                <h2 className="text-3xl font-bold text-center text-blue-600 mb-6">Đăng Ký InteractHub</h2>

                {/* handleSubmit sẽ kiểm tra lỗi trước, nếu OK mới gọi onSubmit */}
                <form onSubmit={handleSubmit(onSubmit)} className="space-y-4">

                    {/* 1. Ô EMAIL */}
                    <div>
                        <label className="block text-gray-700 font-medium mb-1">Email</label>
                        <input
                            type="text"
                            className={`w-full px-4 py-2 border rounded-lg outline-none transition ${errors.email ? 'border-red-500 focus:ring-red-200' : 'focus:ring-blue-200 focus:border-blue-500'}`}
                            placeholder="Nhập email của bạn"
                            {...register("email", {
                                required: "Email không được để trống",
                                pattern: {
                                    value: /^[A-Z0-9._%+-]+@[A-Z0-9.-]+\.[A-Z]{2,}$/i,
                                    message: "Email không đúng định dạng (VD: a@gmail.com)"
                                }
                            })}
                        />
                        {/* Dòng báo lỗi màu đỏ */}
                        {errors.email && <p className="text-red-500 text-sm mt-1">{errors.email.message as string}</p>}
                    </div>

                    {/* 2. Ô TÊN HIỂN THỊ */}
                    <div>
                        <label className="block text-gray-700 font-medium mb-1">Tên hiển thị</label>
                        <input
                            type="text"
                            className={`w-full px-4 py-2 border rounded-lg outline-none transition ${errors.displayName ? 'border-red-500' : 'focus:border-blue-500'}`}
                            placeholder="VD: Nguyễn Văn A"
                            {...register("displayName", {
                                required: "Tên hiển thị không được để trống",
                                minLength: { value: 3, message: "Tên phải có ít nhất 3 ký tự" }
                            })}
                        />
                        {errors.displayName && <p className="text-red-500 text-sm mt-1">{errors.displayName.message as string}</p>}
                    </div>

                    {/* 3. Ô MẬT KHẨU */}
                    <div>
                        <label className="block text-gray-700 font-medium mb-1">Mật khẩu</label>
                        <input
                            type="password"
                            className={`w-full px-4 py-2 border rounded-lg outline-none transition ${errors.password ? 'border-red-500' : 'focus:border-blue-500'}`}
                            placeholder="Ít nhất 6 ký tự"
                            {...register("password", {
                                required: "Mật khẩu không được để trống",
                                minLength: { value: 6, message: "Mật khẩu phải từ 6 ký tự trở lên, chữ Hoa, chữ thường, số, ký tự đặc biệt" }
                            })}
                        />
                        {errors.password && <p className="text-red-500 text-sm mt-1">{errors.password.message as string}</p>}
                    </div>

                    {/* 4. Ô XÁC NHẬN MẬT KHẨU */}
                    <div>
                        <label className="block text-gray-700 font-medium mb-1">Xác nhận mật khẩu</label>
                        <input
                            type="password"
                            className={`w-full px-4 py-2 border rounded-lg outline-none transition ${errors.confirmPassword ? 'border-red-500' : 'focus:border-blue-500'}`}
                            placeholder="Nhập lại mật khẩu"
                            {...register("confirmPassword", {
                                required: "Vui lòng xác nhận mật khẩu",
                                validate: (value) => value === watch("password") || "Mật khẩu nhập lại không khớp!"
                            })}
                        />
                        {errors.confirmPassword && <p className="text-red-500 text-sm mt-1">{errors.confirmPassword.message as string}</p>}
                    </div>

                    {/* NÚT ĐĂNG KÝ */}
                    <button
                        type="submit"
                        disabled={isSubmitting}
                        className={`w-full py-2 rounded-lg font-bold text-white transition ${isSubmitting ? 'bg-blue-400' : 'bg-blue-600 hover:bg-blue-700'}`}
                    >
                        {isSubmitting ? 'Đang xử lý...' : 'Đăng Ký Tài Khoản'}
                    </button>
                </form>

                <p className="text-center mt-4 text-gray-600">
                    Đã có tài khoản? <Link to="/login" className="text-blue-600 font-bold hover:underline">Đăng nhập ngay</Link>
                </p>
            </div>
        </div>
    );
}