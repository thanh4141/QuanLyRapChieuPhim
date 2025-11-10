        function switchView(viewName) {
            // Hide all views
            document.querySelectorAll('.view').forEach(view => {
                view.classList.remove('active');
            });

            // Show selected view
            const viewMap = {
                'login': 'loginView',
                'register': 'registerView',
                'forgot': 'forgotView'
            };

            document.getElementById(viewMap[viewName]).classList.add('active');

            // Hide success message when switching views
            document.getElementById('forgotSuccess').classList.remove('show');
        }

        function togglePassword(inputId) {
            const input = document.getElementById(inputId);
            if (input.type === 'password') {
                input.type = 'text';
            } else {
                input.type = 'password';
            }
        }

        function handleLogin() {
            const email = document.getElementById('loginEmail').value;
            const password = document.getElementById('loginPassword').value;

            if (!email || !password) {
                alert('Vui lòng nhập đầy đủ thông tin!');
                return;
            }

            // Validate email format
            const emailRegex = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
            if (!emailRegex.test(email)) {
                alert('Email không hợp lệ!');
                return;
            }

            alert(`Đăng nhập thành công!\nEmail: ${email}`);
            // Here you would typically send the data to your backend
        }

        function handleRegister() {
            const name = document.getElementById('registerName').value;
            const email = document.getElementById('registerEmail').value;
            const phone = document.getElementById('registerPhone').value;
            const password = document.getElementById('registerPassword').value;
            const confirmPassword = document.getElementById('registerConfirmPassword').value;

            if (!name || !email || !phone || !password || !confirmPassword) {
                alert('Vui lòng nhập đầy đủ thông tin!');
                return;
            }

            // Validate email
            const emailRegex = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
            if (!emailRegex.test(email)) {
                alert('Email không hợp lệ!');
                return;
            }

            // Validate phone
            const phoneRegex = /^[0-9]{10,11}$/;
            if (!phoneRegex.test(phone)) {
                alert('Số điện thoại không hợp lệ!');
                return;
            }

            // Check password match
            if (password !== confirmPassword) {
                alert('Mật khẩu xác nhận không khớp!');
                return;
            }

            // Check password strength
            if (password.length < 6) {
                alert('Mật khẩu phải có ít nhất 6 ký tự!');
                return;
            }

            alert(`Đăng ký thành công!\nTên: ${name}\nEmail: ${email}`);
            switchView('login');
        }

        function handleForgotPassword() {
            const email = document.getElementById('forgotEmail').value;

            if (!email) {
                alert('Vui lòng nhập email!');
                return;
            }

            // Validate email
            const emailRegex = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
            if (!emailRegex.test(email)) {
                alert('Email không hợp lệ!');
                return;
            }

            // Show success message
            document.getElementById('forgotSuccess').classList.add('show');
            document.getElementById('forgotEmail').value = '';

            // Hide success message after 5 seconds
            setTimeout(() => {
                document.getElementById('forgotSuccess').classList.remove('show');
            }, 5000);
        }