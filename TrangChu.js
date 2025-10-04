// Đổi giao diện sáng/tối
const themeToggle = document.querySelector(".theme-toggle");
themeToggle.addEventListener("click", () => {
  document.body.classList.toggle("dark-theme");
});

// Lướt lên đầu trang
const scrollTopBtn = document.querySelector(".scroll-top-icon");
scrollTopBtn.addEventListener("click", () => {
  window.scrollTo({ top: 0, behavior: "smooth" });
});

// Ẩn/hiện nút lên đầu trang khi cuộn
window.addEventListener("scroll", () => {
  if (window.scrollY > 200) {
    scrollTopBtn.style.display = "flex";
  } else {
    scrollTopBtn.style.display = "none";
  }
});
// Ẩn ban đầu
scrollTopBtn.style.display = "none";
