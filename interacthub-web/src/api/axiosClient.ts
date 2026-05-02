import axios from 'axios';

//const API_BASE_URL = 'https://localhost:7275/api';
const API_BASE_URL = 'https://interacthub-web-asescae8abb8bje4.southeastasia-01.azurewebsites.net/api';
//const API_BASE_URL = 'https://congdanh2703-001-site1.stempurl.com/api';


const axiosClient = axios.create({
    baseURL: API_BASE_URL,
    headers: {
        'Content-Type': 'application/json',
    },
});

// Interceptor: Trước khi gửi request đi, tự động nhét Token vào Header
axiosClient.interceptors.request.use(
    (config) => {
        const token = localStorage.getItem('token');
        //console.log("DEBUG JWT gửi lên:", token);
        if (token && config.headers) {
            config.headers.Authorization = `Bearer ${token}`;
        }
        return config;
    },
    (error) => {
        return Promise.reject(error);
    }
);

export default axiosClient;