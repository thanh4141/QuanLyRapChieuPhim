let currentShowtime = null;
let currentSeats = [];
let selectedSeatIds = [];
let bookingPreview = null;

// Load booking page
async function loadBookingPage(showtimeId) {
    try {
        // Reset state
        selectedSeatIds = [];
        bookingPreview = null;
        document.getElementById('book-button').disabled = true;
        document.getElementById('book-button').textContent = 'Đặt vé';

        // Load showtime details
        const showtimeResponse = await get(`/showtimes/${showtimeId}`, false); // Showtimes are public
        if (!showtimeResponse.success || !showtimeResponse.data) {
            showAlert('Không tìm thấy suất chiếu', 'error');
            return;
        }

        currentShowtime = showtimeResponse.data;

        // Check if showtime has started
        const startTime = new Date(currentShowtime.startTime);
        if (startTime <= new Date()) {
            showAlert('Suất chiếu đã bắt đầu. Không thể đặt vé.', 'error');
            return;
        }

        // Load seats
        const seatsResponse = await get(`/auditoriums/${currentShowtime.auditoriumId}/seats?showtimeId=${showtimeId}`, false); // Seats are public
        if (!seatsResponse.success || !seatsResponse.data) {
            showAlert('Không tìm thấy sơ đồ ghế', 'error');
            return;
        }

        currentSeats = seatsResponse.data;
        renderSeatGrid();
        renderBookingInfo();
    } catch (error) {
        console.error('Load booking page error:', error);
        showAlert('Lỗi khi tải trang đặt vé. Vui lòng thử lại.', 'error');
    }
}

// Render seat grid
function renderSeatGrid() {
    const container = document.getElementById('seat-grid-container');
    container.innerHTML = '';

    // Group seats by row
    const seatsByRow = {};
    currentSeats.forEach(seat => {
        if (!seatsByRow[seat.rowLabel]) {
            seatsByRow[seat.rowLabel] = [];
        }
        seatsByRow[seat.rowLabel].push(seat);
    });

    // Sort rows
    const sortedRows = Object.keys(seatsByRow).sort();

    sortedRows.forEach(rowLabel => {
        const rowDiv = document.createElement('div');
        rowDiv.style.marginBottom = 'var(--spacing-md)';

        const rowLabelDiv = document.createElement('div');
        rowLabelDiv.textContent = `Hàng ${rowLabel}`;
        rowLabelDiv.style.marginBottom = 'var(--spacing-sm)';
        rowLabelDiv.style.fontWeight = '600';
        rowDiv.appendChild(rowLabelDiv);

        const seatsDiv = document.createElement('div');
        seatsDiv.className = 'seat-grid';
        seatsDiv.style.gridTemplateColumns = `repeat(${seatsByRow[rowLabel].length}, 1fr)`;

        seatsByRow[rowLabel].forEach(seat => {
            const seatButton = document.createElement('button');
            seatButton.className = 'seat-grid__seat';
            seatButton.textContent = seat.seatNumber;

            if (seat.isBooked) {
                seatButton.classList.add('seat-grid__seat--booked');
                seatButton.disabled = true;
            } else if (selectedSeatIds.includes(seat.seatId)) {
                seatButton.classList.add('seat-grid__seat--selected');
            } else {
                seatButton.classList.add('seat-grid__seat--available');
                if (seat.seatTypeName && seat.seatTypeName.toLowerCase().includes('vip')) {
                    seatButton.classList.add('seat-grid__seat--vip');
                }
            }

            seatButton.onclick = () => toggleSeat(seat.seatId);
            seatsDiv.appendChild(seatButton);
        });

        rowDiv.appendChild(seatsDiv);
        container.appendChild(rowDiv);
    });
}

// Toggle seat selection
async function toggleSeat(seatId) {
    // Check if seat is booked
    const seat = currentSeats.find(s => s.seatId === seatId);
    if (seat && seat.isBooked) {
        showAlert('Ghế này đã được đặt', 'error');
        return;
    }

    if (selectedSeatIds.includes(seatId)) {
        selectedSeatIds = selectedSeatIds.filter(id => id !== seatId);
    } else {
        // Check max seats limit
        const MAX_SEATS = 8;
        if (selectedSeatIds.length >= MAX_SEATS) {
            showAlert(`Bạn chỉ có thể chọn tối đa ${MAX_SEATS} ghế`, 'error');
            return;
        }
        selectedSeatIds.push(seatId);
    }

    renderSeatGrid();
    await updateBookingPreview();
    renderBookingInfo();
}

// Update booking preview
async function updateBookingPreview() {
    if (selectedSeatIds.length === 0) {
        bookingPreview = null;
        document.getElementById('book-button').disabled = true;
        renderBookingInfo();
        return;
    }

    // Validate max seats (e.g., max 8 seats per booking)
    const MAX_SEATS = 8;
    if (selectedSeatIds.length > MAX_SEATS) {
        showAlert(`Bạn chỉ có thể chọn tối đa ${MAX_SEATS} ghế`, 'error');
        selectedSeatIds = selectedSeatIds.slice(0, MAX_SEATS);
        renderSeatGrid();
        return;
    }

    try {
        const response = await post('/bookings/preview', {
            showtimeId: currentShowtime.showtimeId,
            seatIds: selectedSeatIds
        }, true); // Require authentication

        if (response.success && response.data) {
            bookingPreview = response.data;
            document.getElementById('book-button').disabled = false;
        } else {
            showAlert(response.message || 'Không thể tính giá vé. Vui lòng thử lại.', 'error');
            bookingPreview = null;
            document.getElementById('book-button').disabled = true;
        }
    } catch (error) {
        console.error('Update booking preview error:', error);
        showAlert(error.message || 'Lỗi khi tính giá vé. Vui lòng thử lại.', 'error');
        bookingPreview = null;
        document.getElementById('book-button').disabled = true;
    }
}

// Render booking info
function renderBookingInfo() {
    if (!currentShowtime) return;

    const movieTitle = document.getElementById('movie-title');
    movieTitle.textContent = currentShowtime.movieTitle || 'Thông tin đặt vé';

    const showtimeInfo = document.getElementById('showtime-info');
    const startTime = new Date(currentShowtime.startTime);
    const endTime = currentShowtime.endTime ? new Date(currentShowtime.endTime) : null;
    
    let showtimeInfoHtml = `
        <p><strong>Phim:</strong> ${currentShowtime.movieTitle || 'N/A'}</p>
        <p><strong>Phòng:</strong> ${currentShowtime.auditoriumName || 'N/A'}</p>
        <p><strong>Thời gian:</strong> ${startTime.toLocaleString('vi-VN', { 
            weekday: 'long', 
            day: '2-digit', 
            month: '2-digit', 
            year: 'numeric',
            hour: '2-digit', 
            minute: '2-digit' 
        })}`;
    
    if (endTime) {
        showtimeInfoHtml += ` - ${endTime.toLocaleTimeString('vi-VN', { hour: '2-digit', minute: '2-digit' })}`;
    }
    showtimeInfoHtml += `</p>`;
    
    if (currentShowtime.format) {
        showtimeInfoHtml += `<p><strong>Định dạng:</strong> ${currentShowtime.format}</p>`;
    }
    if (currentShowtime.language) {
        showtimeInfoHtml += `<p><strong>Ngôn ngữ:</strong> ${currentShowtime.language}</p>`;
    }
    
    showtimeInfo.innerHTML = showtimeInfoHtml;

    const selectedSeatsDiv = document.getElementById('selected-seats');
    if (selectedSeatIds.length === 0) {
        selectedSeatsDiv.innerHTML = '<p class="text-secondary">Chưa chọn ghế</p>';
    } else {
        const selectedSeats = currentSeats.filter(s => selectedSeatIds.includes(s.seatId));
        let seatsHtml = '<p><strong>Ghế đã chọn (${selectedSeatIds.length}):</strong></p><ul style="margin: var(--spacing-sm) 0; padding-left: var(--spacing-lg);">';
        
        if (bookingPreview && bookingPreview.seats) {
            bookingPreview.seats.forEach(seat => {
                seatsHtml += `<li>${seat.rowLabel}${seat.seatNumber} - ${seat.seatTypeName || 'Thường'} (${formatCurrency(seat.price)})</li>`;
            });
        } else {
            selectedSeats.forEach(seat => {
                seatsHtml += `<li>${seat.rowLabel}${seat.seatNumber}</li>`;
            });
        }
        seatsHtml += '</ul>';
        selectedSeatsDiv.innerHTML = seatsHtml;
    }

    const totalAmountDiv = document.getElementById('total-amount');
    if (bookingPreview && bookingPreview.totalAmount) {
        let totalHtml = `<p><strong>Tổng tiền:</strong> ${formatCurrency(bookingPreview.totalAmount)}</p>`;
        if (bookingPreview.subTotal) {
            totalHtml += `<p style="font-size: 0.875rem; color: var(--text-secondary); margin-top: var(--spacing-xs);">`;
            totalHtml += `Tạm tính: ${formatCurrency(bookingPreview.subTotal)}`;
            if (bookingPreview.taxAmount) {
                totalHtml += ` | Thuế (10%): ${formatCurrency(bookingPreview.taxAmount)}`;
            }
            totalHtml += `</p>`;
        }
        totalAmountDiv.innerHTML = totalHtml;
    } else {
        totalAmountDiv.innerHTML = '';
    }
}

// Book tickets
document.getElementById('book-button').addEventListener('click', async () => {
    if (!bookingPreview || selectedSeatIds.length === 0) {
        showAlert('Vui lòng chọn ít nhất một ghế', 'error');
        return;
    }

    if (!currentShowtime) {
        showAlert('Thông tin suất chiếu không hợp lệ', 'error');
        return;
    }

    // Check if showtime has started
    const startTime = new Date(currentShowtime.startTime);
    if (startTime <= new Date()) {
        showAlert('Không thể đặt vé cho suất chiếu đã bắt đầu', 'error');
        return;
    }

    const button = document.getElementById('book-button');
    button.disabled = true;
    button.textContent = 'Đang xử lý...';

    try {
        const response = await post('/bookings', {
            showtimeId: currentShowtime.showtimeId,
            seatIds: selectedSeatIds
        }, true);

        if (response.success && response.data) {
            showAlert('Đặt vé thành công! Bạn có 15 phút để thanh toán.', 'success');
            setTimeout(() => {
                window.location.href = `payment.html?reservationId=${response.data.reservationId}`;
            }, 1500);
        } else {
            showAlert(response.message || 'Đặt vé thất bại. Vui lòng thử lại.', 'error');
            button.disabled = false;
            button.textContent = 'Đặt vé';
            
            // Reload seats to refresh availability
            if (response.message && response.message.includes('đã được đặt')) {
                setTimeout(() => {
                    loadBookingPage(currentShowtime.showtimeId);
                }, 2000);
            }
        }
    } catch (error) {
        console.error('Book tickets error:', error);
        const errorMessage = error.message || 'Đặt vé thất bại. Vui lòng thử lại.';
        showAlert(errorMessage, 'error');
        button.disabled = false;
        button.textContent = 'Đặt vé';
        
        // Reload seats if error is about seats being booked
        if (errorMessage.includes('đã được đặt') || errorMessage.includes('không còn trống')) {
            setTimeout(() => {
                loadBookingPage(currentShowtime.showtimeId);
            }, 2000);
        }
    }
});

// Format currency
function formatCurrency(amount) {
    return new Intl.NumberFormat('vi-VN', {
        style: 'currency',
        currency: 'VND'
    }).format(amount);
}

// Show alert
function showAlert(message, type) {
    const container = document.getElementById('alert-container');
    container.innerHTML = `<div class="alert alert--${type}">${message}</div>`;
    setTimeout(() => {
        container.innerHTML = '';
    }, 5000);
}

