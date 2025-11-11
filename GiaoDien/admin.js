document.addEventListener('DOMContentLoaded', ()=>{

  // Sidebar nav switch
  document.querySelectorAll('.nav-item').forEach(btn=>{
    btn.addEventListener('click', ()=>{
      document.querySelectorAll('.nav-item').forEach(n=>n.classList.remove('active'));
      btn.classList.add('active');
      const section = btn.dataset.section;
      document.querySelectorAll('.admin-section').forEach(s=> s.classList.remove('active'));
      const el = document.getElementById(section);
      if(el) el.classList.add('active');
    });
  });

  // Sample movies data and render
  const movies = [
    {id:1,title:'The Creator',genre:'Sci-fi',poster:'https://i.imgur.com/1vQk3vT.jpg',status:'Đang chiếu'},
    {id:2,title:'Blue Beetle',genre:'Action',poster:'https://i.imgur.com/8Km9tLL.jpg',status:'Sắp chiếu'},
    {id:3,title:'Gran Turismo',genre:'Sport',poster:'https://i.imgur.com/5h7X6vE.jpg',status:'Đang chiếu'}
  ];

  const tbody = document.getElementById('movies-tbody');
  function renderMovies(){
    tbody.innerHTML = movies.map(m=>`
      <tr data-id="${m.id}">
        <td><img src="${m.poster}" alt="${m.title}"></td>
        <td>${m.title}</td>
        <td>${m.genre}</td>
        <td>${m.status}</td>
        <td>
          <button class="btn edit" data-id="${m.id}">Sửa</button>
          <button class="btn" data-action="del" data-id="${m.id}">Xóa</button>
        </td>
      </tr>
    `).join('');
  }
  renderMovies();

  // Movie form
  const form = document.getElementById('movie-form');
  const formReset = document.getElementById('movie-form-reset');
  form.addEventListener('submit', e=>{
    e.preventDefault();
    const fd = new FormData(form);
    const data = {
      id: Date.now(),
      title: fd.get('title'),
      genre: fd.get('genre'),
      duration: fd.get('duration')||0,
      poster: fd.get('poster') || 'https://via.placeholder.com/120x80'
    };
    movies.unshift(data);
    renderMovies();
    form.reset();
    alert('Đã lưu phim (demo).');
  });
  formReset.addEventListener('click', ()=> form.reset());

  // Delegated table actions
  tbody.addEventListener('click', e=>{
    const tr = e.target.closest('tr');
    if(!tr) return;
    const id = parseInt(e.target.dataset.id || tr.dataset.id);
    if(e.target.dataset.action === 'del'){
      const idx = movies.findIndex(m=>m.id===id);
      if(idx>-1){ movies.splice(idx,1); renderMovies(); }
    } else if(e.target.classList.contains('edit')){
      const m = movies.find(x=>x.id===id);
      if(m){
        form.title.value = m.title;
        form.genre.value = m.genre;
        form.poster.value = m.poster;
        window.scrollTo({top:0,behavior:'smooth'});
      }
    }
  });

  // Simple widgets (buttons)
  document.getElementById('refresh-movies').addEventListener('click', ()=> renderMovies());
  document.getElementById('add-movie-btn').addEventListener('click', ()=> {
    window.location.hash = '#movies';
    document.querySelector('[data-section="movies"]').click();
    form.title.focus();
  });

  // Theme toggle + sidebar collapse
  document.getElementById('theme-toggle').addEventListener('click', ()=>{
    document.body.classList.toggle('admin-dark');
    document.body.classList.toggle('admin-light');
  });
  document.getElementById('sidebar-collapse').addEventListener('click', ()=>{
    const sb = document.querySelector('.admin-sidebar');
    if(sb.style.width && sb.style.width !== ''){ sb.style.width=''; document.querySelector('.admin-main').style.marginLeft = ''; }
    else{ sb.style.width = (sb.style.width === '64px' ? '260px' : '64px'); document.querySelector('.admin-main').style.marginLeft = sb.style.width; }
  });

});