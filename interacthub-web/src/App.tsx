import { BrowserRouter, Routes, Route, Navigate } from 'react-router-dom';
import { ToastContainer } from 'react-toastify';
import 'react-toastify/dist/ReactToastify.css';
import Register from './pages/Register';
import Login from './pages/Login';
import Home from './pages/Home'; // Import trang Home vào
import Profile from './pages/Profile';
import ProtectedRoute from './components/ProtectedRoute';
import { AuthProvider } from './context/AuthContext';
import Friend from './pages/Friend';
import UserProfile from './pages/UserProfile';
import HashtagPage from './pages/HashtagPage';
import AdminPage from './pages/AdminPage';

function App() {
    return (
        <BrowserRouter>
            <ToastContainer position="top-right" autoClose={3000} />
            <AuthProvider>
                <Routes>
                    {/* nhung trang ma ai cung co the vao duoc */}
                    <Route path="/login" element={<Login />} />
                    <Route path="/register" element={<Register />} />

                    {/* nhung trang bat buoc phai dang nhap moi vao dc */}
                    <Route
                        path="/friends"
                        element={
                            <ProtectedRoute>
                                <Friend />
                            </ProtectedRoute>
                        }
                    />

                    <Route
                        path="/"
                        element={
                            <ProtectedRoute>
                                <Home />
                            </ProtectedRoute>
                        }
                    />

                    <Route
                        path="/profile"
                        element={
                            <ProtectedRoute>
                                <Profile />
                            </ProtectedRoute>
                        }
                    />

                    <Route
                        path="/profile/:id"
                        element={
                            <ProtectedRoute>
                                <UserProfile />
                            </ProtectedRoute>
                        }
                    />

                    <Route
                        path="/hashtag/:tag"
                        element={
                            <ProtectedRoute>
                                <HashtagPage />
                            </ProtectedRoute>
                        }
                    />

                    {/* neu go duong link khac thi se cho ve trang login*/}
                    <Route path="*" element={<Navigate to="/login" replace />} />

                    <Route
                        path="/admin"
                        element={
                            <ProtectedRoute>
                                <AdminPage />
                            </ProtectedRoute>
                        }
                    />
                </Routes>
            </AuthProvider>
        </BrowserRouter>
    );
}

export default App;