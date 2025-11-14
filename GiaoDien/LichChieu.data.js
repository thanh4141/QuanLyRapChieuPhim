// Tạo data và tự gán logo từ folder /PICTURE/logos/
// Đặt các file logo nhỏ ở: /PICTURE/logos/{chain}.webp (vd: beta.webp, cinestar.webp, lotte.webp, cgv.webp, mega.webp, galaxy.webp, placeholder.webp)
(function () {
  const DEFAULT_PLACEHOLDER = "/PICTURE/logos/placeholder.webp";
  const EXT_ORDER = ["webp", "png", "svg"];

  const RAW = [
    {
      key: "tp_hcm",
      name: "Tp. Hồ Chí Minh",
      cinemas: [
        {
          id: "beta_quang_trung",
          name: "Beta Quang Trung",
          sub: "Quận Gò Vấp",
          addr: "645 Quang Trung, P.11, Q.Gò Vấp",
          chain: "beta",
        },
        {
          id: "beta_tran_quang_khai",
          name: "Beta Trần Quang Khải",
          sub: "Quận 1",
          addr: "Trần Quang Khải, Q.1",
          chain: "beta",
        },
        {
          id: "cinestar_hai_ba_trung",
          name: "Cinestar Hai Bà Trưng",
          sub: "Quận 3",
          addr: "Hai Bà Trưng, Q.3",
          chain: "cinestar",
        },
        {
          id: "dcine_ben_thanh",
          name: "DCINE Bến Thành",
          sub: "Quận 1",
          addr: "Bến Thành, Q.1",
          chain: "dcine",
        },
        {
          id: "satra_q6",
          name: "Cinestar Satra Quận 6",
          sub: "Quận 6",
          addr: "Quận 6, TP HCM",
          chain: "cinestar",
        },
        {
          id: "cinestar_quoc_thanh",
          name: "Cinestar Quốc Thanh",
          sub: "Quận 5",
          addr: "Quốc Thanh, Q.5",
          chain: "cinestar",
        },
      ],
    },
    {
      key: "ha_noi",
      name: "Hà Nội",
      cinemas: [
        {
          id: "lotte_hanoi",
          name: "Lotte Cinema Hà Nội",
          sub: "Đống Đa",
          addr: "Đống Đa, Hà Nội",
          chain: "lotte",
        },
        {
          id: "cg_vincom_ba_trieu",
          name: "CGV Vincom Bà Triệu",
          sub: "Hai Bà Trưng",
          addr: "Bà Triệu, Hà Nội",
          chain: "cgv",
        },
        {
          id: "bic_cinemas",
          name: "BIC Cinema",
          sub: "Cầu Giấy",
          addr: "Cầu Giấy, Hà Nội",
          chain: "placeholder",
        },
      ],
    },
    {
      key: "da_nang",
      name: "Đà Nẵng",
      cinemas: [
        {
          id: "lotte_dn",
          name: "Lotte Cinema Đà Nẵng",
          sub: "Hải Châu",
          addr: "Hải Châu, Đà Nẵng",
          chain: "lotte",
        },
        {
          id: "galaxy_dn",
          name: "Galaxy Đà Nẵng",
          sub: "Sơn Trà",
          addr: "Sơn Trà, Đà Nẵng",
          chain: "galaxy",
        },
      ],
    },
    {
      key: "dong_nai",
      name: "Đồng Nai",
      cinemas: [
        {
          id: "cinestar_bien_hoa",
          name: "Cinestar Biên Hòa",
          sub: "Biên Hòa",
          addr: "Biên Hòa, Đồng Nai",
          chain: "cinestar",
        },
        {
          id: "mega_bienhoa",
          name: "Mega GS Biên Hòa",
          sub: "Biên Hòa",
          addr: "Biên Hòa, Đồng Nai",
          chain: "mega",
        },
      ],
    },
    {
      key: "can_tho",
      name: "Cần Thơ",
      cinemas: [
        {
          id: "cinestar_can_tho",
          name: "Cinestar Cần Thơ",
          sub: "Ninh Kiều",
          addr: "Ninh Kiều, Cần Thơ",
          chain: "cinestar",
        },
      ],
    },
    {
      key: "binh_duong",
      name: "Bình Dương",
      cinemas: [
        {
          id: "lotte_binh_duong",
          name: "Lotte Cinema Bình Dương",
          sub: "Thủ Dầu Một",
          addr: "Thủ Dầu Một, Bình Dương",
          chain: "lotte",
        },
        {
          id: "beta_binh_duong",
          name: "Beta Bình Dương",
          sub: "Thuận An",
          addr: "Thuận An, Bình Dương",
          chain: "beta",
        },
      ],
    },
    {
      key: "binh_phuoc",
      name: "Bình Phước",
      cinemas: [
        {
          id: "cinema_bp",
          name: "Cinema Bình Phước",
          sub: "Đồng Xoài",
          addr: "Đồng Xoài, Bình Phước",
          chain: "placeholder",
        },
      ],
    },
    {
      key: "binh_thuan",
      name: "Bình Thuận",
      cinemas: [
        {
          id: "cinema_bt",
          name: "Cinema Bình Thuận",
          sub: "Phan Thiết",
          addr: "Phan Thiết, Bình Thuận",
          chain: "placeholder",
        },
      ],
    },
    {
      key: "ba_ria",
      name: "Bà Rịa - Vũng Tàu",
      cinemas: [
        {
          id: "megags_vungtau",
          name: "Mega GS Vũng Tàu",
          sub: "Vũng Tàu",
          addr: "Vũng Tàu, Bà Rịa - Vũng Tàu",
          chain: "mega",
        },
      ],
    },
    {
      key: "an_giang",
      name: "An Giang",
      cinemas: [
        {
          id: "cinema_ag",
          name: "Cinema An Giang",
          sub: "Long Xuyên",
          addr: "Long Xuyên, An Giang",
          chain: "placeholder",
        },
      ],
    },
    {
      key: "ben_tre",
      name: "Bến Tre",
      cinemas: [
        {
          id: "cinema_bt2",
          name: "Cinema Bến Tre",
          sub: "Bến Tre",
          addr: "Bến Tre, Bến Tre",
          chain: "placeholder",
        },
      ],
    },
    {
      key: "hai_phong",
      name: "Hải Phòng",
      cinemas: [
        {
          id: "lotte_hp",
          name: "Lotte Cinema Hải Phòng",
          sub: "Ngô Quyền",
          addr: "Ngô Quyền, Hải Phòng",
          chain: "lotte",
        },
      ],
    },
    {
      key: "kien_giang",
      name: "Kiên Giang",
      cinemas: [
        {
          id: "cinema_kg",
          name: "Cinema Kiên Giang",
          sub: "Rạch Giá",
          addr: "Rạch Giá, Kiên Giang",
          chain: "placeholder",
        },
      ],
    },
    {
      key: "hai_duong",
      name: "Hải Dương",
      cinemas: [
        {
          id: "cinema_hd",
          name: "Cinema Hải Dương",
          sub: "Hải Dương",
          addr: "Hải Dương, Hải Dương",
          chain: "placeholder",
        },
      ],
    },
    {
      key: "tra_vinh",
      name: "Trà Vinh",
      cinemas: [
        {
          id: "cinema_tv",
          name: "Cinema Trà Vinh",
          sub: "Trà Vinh",
          addr: "Trà Vinh, Trà Vinh",
          chain: "placeholder",
        },
      ],
    },
  ];

  RAW.forEach((prov) => {
    prov.cinemas.forEach((c) => {
      const chain = (c.chain || "placeholder").toString().toLowerCase();
      c.logoFallbacks = EXT_ORDER.map(
        (ext) => `/PICTURE/logos/${chain}.${ext}`
      );
      c.logoFallbacks.push(DEFAULT_PLACEHOLDER);
      // primary guess
      c.logo = c.logoFallbacks[0];
    });
  });

  window.LICH_DATA = RAW;
})();
