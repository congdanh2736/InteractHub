import { useState, useEffect } from 'react';

// Custom Hook này sẽ nhận vào 1 giá trị và thời gian chờ (delay)
export function useDebounce<T>(value: T, delay: number): T {
    const [debouncedValue, setDebouncedValue] = useState<T>(value);

    useEffect(() => {
        // Cài đặt một bộ đếm giờ
        const timer = setTimeout(() => {
            setDebouncedValue(value);
        }, delay);

        // Nếu người dùng gõ phím mới trước khi hết giờ -> Hủy bộ đếm cũ, đếm lại từ đầu
        return () => {
            clearTimeout(timer);
        };
    }, [value, delay]);

    return debouncedValue;
}