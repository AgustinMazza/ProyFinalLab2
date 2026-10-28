const { createApp } = Vue;

createApp({
  data() {
    return {
      negocios: [],
      categorias: [],
      buscar: "",
      idCategoria: 0,
      pagina: 1,
      totalPaginas: 0,
      total: 0,
      cargando: false,
      error: "",
      temporizador: null,
      pedido: 0,
    };
  },

  mounted() {
    this.cargarCategorias();
    this.cargarNegocios();
  },

  methods: {
    async cargarCategorias() {
      const respuesta = await fetch("/Catalogo/ListarCategorias");
      this.categorias = await respuesta.json();
    },

    async cargarNegocios() {
      const miPedido = ++this.pedido;
      this.cargando = true;
      this.error = "";

      try {
        const url =
          `/Catalogo/Listar?buscar=${encodeURIComponent(this.buscar)}` +
          `&idCategoria=${this.idCategoria}&pagina=${this.pagina}`;
        const respuesta = await fetch(url);
        if (!respuesta.ok) throw new Error();
        const datos = await respuesta.json();

        if (miPedido !== this.pedido) return;

        this.negocios = datos.items;
        this.total = datos.total;
        this.totalPaginas = datos.totalPaginas;
        this.pagina = datos.pagina;
      } catch {
        if (miPedido !== this.pedido) return;
        this.error = "No se pudo cargar el listado. Probá de nuevo.";
      }

      this.cargando = false;
    },

    buscarConPausa() {
      clearTimeout(this.temporizador);
      this.temporizador = setTimeout(() => this.filtrar(), 300);
    },

    filtrar() {
      this.pagina = 1;
      this.cargarNegocios();
    },

    irAPagina(n) {
      if (n < 1 || n > this.totalPaginas) return;
      this.pagina = n;
      this.cargarNegocios();
      window.scrollTo({ top: 0, behavior: "smooth" });
    },
  },
}).mount("#app");