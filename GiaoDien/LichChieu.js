// Logic UI: render area list, cinemas and handle clicks (dùng window.LICH_DATA)
// Tích hợp lazy-load + fallback logo từ c.logoFallbacks (folder /PICTURE)
document.addEventListener("DOMContentLoaded", () => {
  const data = window.LICH_DATA || [];
  const areaList = document.querySelector(".area-list");
  const cinemaList = document.querySelector(".cinema-list");
  const theaterNameEl = document.querySelector(".theater-name");
  const theaterAddrEl = document.querySelector(".theater-address");
  const schedulePanel = document.querySelector(".schedule-panel");

  function escapeHtml(str = "") {
    return String(str).replace(
      /[&<>"']/g,
      (s) =>
        ({
          "&": "&amp;",
          "<": "&lt;",
          ">": "&gt;",
          '"': "&quot;",
          "'": "&#39;",
        }[s])
    );
  }

  // Cache for resolved logos per chain to reduce retry
  const LOGO_CACHE = {};

  // try to load image with fallback list (sequential onerror)
  function loadImageWithFallback(imgEl, fallbacks = []) {
    if (!imgEl || !fallbacks.length) return;
    // if any cached resolved src exists, use it immediately
    const cacheKey = imgEl.dataset.chain || "";
    if (cacheKey && LOGO_CACHE[cacheKey]) {
      imgEl.src = LOGO_CACHE[cacheKey];
      return;
    }

    let idx = 0;
    function tryNext() {
      if (idx >= fallbacks.length) return;
      const src = fallbacks[idx++];
      imgEl.onerror = tryNext;
      imgEl.onload = () => {
        imgEl.onerror = null;
        if (cacheKey) LOGO_CACHE[cacheKey] = src;
      };
      imgEl.src = src;
    }
    tryNext();
  }

  // IntersectionObserver for lazy loading logos
  const io =
    "IntersectionObserver" in window
      ? new IntersectionObserver(
          (entries) => {
            entries.forEach((entry) => {
              if (!entry.isIntersecting) return;
              const img = entry.target;
              const fallbacks = JSON.parse(
                img.getAttribute("data-fallbacks") || "[]"
              );
              loadImageWithFallback(img, fallbacks);
              io.unobserve(img);
            });
          },
          { rootMargin: "200px" }
        )
      : null;

  function observeImg(img) {
    if (!img) return;
    // prefer native lazy if available but still use observer fallback to set src
    img.loading = "lazy";
    if (io) io.observe(img);
    else {
      // immediate load if no observer
      loadImageWithFallback(
        img,
        JSON.parse(img.getAttribute("data-fallbacks") || "[]")
      );
    }
  }

  function renderAreas() {
    if (!areaList) return;
    areaList.innerHTML = "";
    data.forEach((p, idx) => {
      const li = document.createElement("li");
      li.className = "area-item" + (idx === 0 ? " active" : "");
      li.dataset.index = idx;
      li.innerHTML = `${escapeHtml(p.name)} <span class="count">${
        p.cinemas.length
      }</span>`;
      areaList.appendChild(li);
    });
  }

  function renderCinemas(areaIdx) {
    if (!cinemaList) return;
    const province = data[areaIdx] || { cinemas: [] };
    cinemaList.innerHTML = "";
    province.cinemas.forEach((c, i) => {
      const li = document.createElement("li");
      li.className = "cinema-item" + (i === 0 ? " active" : "");
      li.dataset.cid = c.id;
      li.dataset.index = i;

      // create elements to avoid injecting long HTML strings (safer)
      const left = document.createElement("div");
      left.className = "cinema-left";
      const img = document.createElement("img");
      img.alt = c.name || "logo";
      img.width = 44;
      img.height = 44;
      img.className = "cinema-logo";
      img.dataset.fallbacks = JSON.stringify(c.logoFallbacks || []);
      img.dataset.chain = (c.chain || "").toString().toLowerCase();
      img.src = "/PICTURE/logos/placeholder.webp"; // quick placeholder
      img.setAttribute(
        "onerror",
        "this.onerror=null;this.src='/PICTURE/logos/placeholder.webp'"
      );
      left.appendChild(img);

      const right = document.createElement("div");
      right.className = "cinema-right";
      const nm = document.createElement("div");
      nm.className = "cinema-name";
      nm.textContent = c.name;
      const sub = document.createElement("div");
      sub.className = "cinema-sub";
      sub.textContent = c.sub || "";
      right.appendChild(nm);
      right.appendChild(sub);

      li.appendChild(left);
      li.appendChild(right);
      cinemaList.appendChild(li);

      // lazy load logo
      observeImg(img);
    });

    setTheaterInfo(areaIdx, 0);
  }

  function setTheaterInfo(areaIdx, cinemaIdx) {
    const p = data[areaIdx];
    if (!p) return;
    const c = p.cinemas[cinemaIdx];
    if (!c) return;
    if (theaterNameEl) theaterNameEl.textContent = c.name;
    if (theaterAddrEl)
      theaterAddrEl.innerHTML = `${escapeHtml(
        c.addr
      )} - <a href="#">Bản đồ</a>`;
  }

  // init
  renderAreas();
  renderCinemas(0);

  // events
  if (areaList) {
    areaList.addEventListener("click", (e) => {
      const li = e.target.closest(".area-item");
      if (!li) return;
      const idx = parseInt(li.dataset.index, 10);
      areaList
        .querySelectorAll(".area-item")
        .forEach((i) => i.classList.remove("active"));
      li.classList.add("active");
      renderCinemas(idx);
    });
  }

  if (cinemaList) {
    cinemaList.addEventListener("click", (e) => {
      const li = e.target.closest(".cinema-item");
      if (!li) return;
      const cinemaIdx = parseInt(li.dataset.index, 10);
      const activeArea = document.querySelector(".area-item.active");
      const areaIdx = activeArea ? parseInt(activeArea.dataset.index, 10) : 0;
      cinemaList
        .querySelectorAll(".cinema-item")
        .forEach((i) => i.classList.remove("active"));
      li.classList.add("active");
      setTheaterInfo(areaIdx, cinemaIdx);
      if (schedulePanel)
        schedulePanel.scrollIntoView({ behavior: "smooth", block: "start" });
    });
  }

  document.querySelectorAll(".date-tab").forEach((tab) => {
    tab.addEventListener("click", () => {
      document
        .querySelectorAll(".date-tab")
        .forEach((t) => t.classList.remove("active"));
      tab.classList.add("active");
    });
  });
});
