// Staff functions for check-in and ticket management

let scanningInterval = null;

// Check ticket by QR code
async function checkTicketByQrCode(qrCodeData) {
    try {
        const response = await get(`/tickets/qr/${encodeURIComponent(qrCodeData)}`, true);
        
        if (response.success && response.data) {
            currentTicket = response.data;
            displayTicketInfo(currentTicket);
            document.getElementById('qr-code-input').value = '';
        } else {
            showAlert(response.message || 'Không tìm thấy vé với mã QR code này', 'error');
            hideTicketInfo();
        }
    } catch (error) {
        console.error('Check ticket error:', error);
        showAlert(error.message || 'Lỗi khi kiểm tra vé', 'error');
        hideTicketInfo();
    }
}

// Display ticket info
function displayTicketInfo(ticket) {
    const container = document.getElementById('ticket-info-container');
    const infoDiv = document.getElementById('ticket-info');
    const checkinBtn = document.getElementById('checkin-btn');

    const startTime = ticket.showtimeStartTime ? new Date(ticket.showtimeStartTime) : new Date();
    const statusClass = ticket.status === 'Paid' ? 'ticket-status--paid' : 
                       ticket.status === 'CheckedIn' ? 'ticket-status--checkedin' : 
                       'ticket-status--booked';
    
    const statusText = ticket.status === 'Paid' ? 'Đã thanh toán' :
                      ticket.status === 'CheckedIn' ? 'Đã check-in' :
                      ticket.status === 'Booked' ? 'Đã đặt' : ticket.status;

    infoDiv.innerHTML = `
        <p><strong>Mã đặt vé:</strong> <code style="background-color: var(--bg-secondary); padding: 2px 6px; border-radius: var(--border-radius);">${ticket.reservationCode || 'N/A'}</code></p>
        <p><strong>Phim:</strong> ${ticket.movieTitle || 'N/A'}</p>
        <p><strong>Phòng:</strong> ${ticket.auditoriumName || 'N/A'}</p>
        <p><strong>Ghế:</strong> ${ticket.rowLabel}${ticket.seatNumber}</p>
        <p><strong>Loại ghế:</strong> ${ticket.seatTypeName || 'N/A'}</p>
        <p><strong>Thời gian:</strong> ${startTime.toLocaleString('vi-VN', { 
            weekday: 'long', 
            day: '2-digit', 
            month: '2-digit', 
            year: 'numeric',
            hour: '2-digit', 
            minute: '2-digit' 
        })}</p>
        <p><strong>Trạng thái:</strong> <span class="ticket-status ${statusClass}">${statusText}</span></p>
        ${ticket.checkedInAt ? `<p><strong>Đã check-in lúc:</strong> ${new Date(ticket.checkedInAt).toLocaleString('vi-VN')}</p>` : ''}
        <p><strong>Giá vé:</strong> ${formatCurrency(ticket.seatPrice || 0)}</p>
    `;

    // Enable/disable check-in button
    if (ticket.status === 'Paid' && !ticket.checkedInAt) {
        checkinBtn.disabled = false;
        checkinBtn.textContent = 'Check-in vé';
    } else if (ticket.status === 'CheckedIn') {
        checkinBtn.disabled = true;
        checkinBtn.textContent = 'Đã check-in';
        checkinBtn.classList.add('btn--secondary');
    } else {
        checkinBtn.disabled = true;
        checkinBtn.textContent = 'Vé chưa thanh toán';
    }

    container.style.display = 'block';
    container.scrollIntoView({ behavior: 'smooth', block: 'nearest' });
}

// Hide ticket info
function hideTicketInfo() {
    document.getElementById('ticket-info-container').style.display = 'none';
    currentTicket = null;
}

// Check-in ticket
async function checkInTicket(qrCodeData) {
    const button = document.getElementById('checkin-btn');
    button.disabled = true;
    button.textContent = 'Đang xử lý...';

    try {
        const response = await post('/tickets/checkin', {
            qrCodeData: qrCodeData
        }, true);

        if (response.success) {
            showAlert('Check-in vé thành công!', 'success');
            // Reload ticket info
            await checkTicketByQrCode(qrCodeData);
        } else {
            showAlert(response.message || 'Check-in vé thất bại', 'error');
        }
    } catch (error) {
        console.error('Check-in error:', error);
        showAlert(error.message || 'Lỗi khi check-in vé', 'error');
    } finally {
        // Button state will be updated by displayTicketInfo
    }
}

// Start camera for QR scanning
async function startCamera() {
    try {
        const video = document.getElementById('qr-video');
        const startBtn = document.getElementById('start-camera-btn');
        const stopBtn = document.getElementById('stop-camera-btn');

        stream = await navigator.mediaDevices.getUserMedia({ 
            video: { 
                facingMode: 'environment' // Use back camera on mobile
            } 
        });

        video.srcObject = stream;
        video.style.display = 'block';
        video.play();

        startBtn.style.display = 'none';
        stopBtn.style.display = 'block';

        // Start scanning
        scanningInterval = setInterval(() => {
            scanQRCode(video);
        }, 500);
    } catch (error) {
        console.error('Camera error:', error);
        showAlert('Không thể truy cập camera. Vui lòng nhập mã QR thủ công.', 'error');
    }
}

// Stop camera
function stopCamera() {
    const video = document.getElementById('qr-video');
    const startBtn = document.getElementById('start-camera-btn');
    const stopBtn = document.getElementById('stop-camera-btn');

    if (stream) {
        stream.getTracks().forEach(track => track.stop());
        stream = null;
    }

    video.style.display = 'none';
    startBtn.style.display = 'block';
    stopBtn.style.display = 'none';

    if (scanningInterval) {
        clearInterval(scanningInterval);
        scanningInterval = null;
    }
}

// Scan QR code from video
function scanQRCode(video) {
    const canvas = document.getElementById('qr-canvas');
    const context = canvas.getContext('2d');

    if (video.readyState === video.HAVE_ENOUGH_DATA) {
        canvas.width = video.videoWidth;
        canvas.height = video.videoHeight;
        context.drawImage(video, 0, 0, canvas.width, canvas.height);

        const imageData = context.getImageData(0, 0, canvas.width, canvas.height);
        
        // Simple QR code detection - in production, use a library like jsQR
        // For now, we'll use manual input or a QR library
        // This is a placeholder - you should integrate a QR code library like jsQR
    }
}

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

