const { createApp } = Vue;

createApp({
  data() {
    return {
      reservas: [],
      pagina: 1,
      totalPaginas: 0,
      error: "",
      info: null,
    };
  },

  mounted() {
    this.cargarReservas();
  },

  methods: {
    async cargarReservas() {
      this.error = "";
      try {
        const respuesta = await fetch(
          `/Reservas/ListarMias?pagina=${this.pagina}`,
        );
        if (!respuesta.ok) {
          this.error = `No se pudieron cargar las reservas (código ${respuesta.status})`;
          return;
        }
        const datos = await respuesta.json();
        this.reservas = datos.items;
        this.totalPaginas = datos.totalPaginas;
        this.pagina = datos.pagina;
      } catch {
        this.error = "No se pudieron cargar las reservas";
      }
    },

    irAPagina(n) {
      if (n < 1 || n > this.totalPaginas) return;
      this.pagina = n;
      this.cargarReservas();
    },

    async verInfo(r) {
      this.error = "";
      try {
        const respuesta = await fetch(`/Reservas/InfoMia/${r.idReserva}`);
        if (!respuesta.ok) {
          this.error = `No se pudo cargar la información (código ${respuesta.status})`;
          return;
        }
        this.info = await respuesta.json();
      } catch {
        this.error = "No se pudo cargar la información";
      }
    },

    cerrarInfo() {
      this.info = null;
    },

    async cancelar(r) {
      if (
        !confirm(
          `¿Cancelar la reserva en ${r.lugar} del ${this.formatearFecha(r.fecha)} a las ${r.horaInicio}?`,
        )
      )
        return;

      this.error = "";
      try {
        const token = document.querySelector(
          'input[name="__RequestVerificationToken"]',
        ).value;
        const respuesta = await fetch(`/Reservas/Cancelar/${r.idReserva}`, {
          method: "POST",
          headers: { RequestVerificationToken: token },
        });

        if (respuesta.redirected) {
          this.error = "Tu sesión venció, volvé a iniciar sesión";
          return;
        }

        if (!respuesta.ok) {
          let detalle = `No se pudo cancelar (código ${respuesta.status})`;
          try {
            detalle = (await respuesta.json()).mensaje || detalle;
          } catch {}
          this.error = detalle;
        }
      } catch {
        this.error = "No se pudo cancelar la reserva";
      }
      await this.cargarReservas();
    },

    formatearFecha(texto) {
      const [anio, mes, dia] = texto.split("-");
      return `${dia}/${mes}/${anio}`;
    },

    claseEstado(estado) {
      if (estado === "Confirmada") return "bg-success";
      if (estado === "Pendiente") return "bg-warning text-dark";
      if (estado === "Cancelada") return "bg-danger";
      return "bg-secondary";
    },
  },
}).mount("#app");
