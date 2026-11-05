const { createApp } = Vue;

function tokenAntiforgery() {
  return document.querySelector('input[name="__RequestVerificationToken"]')
    .value;
}

createApp({
  data() {
    return {
      reservas: [],
      buscar: "",
      estado: "0",
      pagina: 1,
      totalPaginas: 0,
      error: "",
      temporizador: null,
    };
  },

  mounted() {
    this.cargarReservas();
  },

  methods: {
    async cargarReservas() {
      try {
        const url = `/GestionReservas/Listar?buscar=${encodeURIComponent(this.buscar)}&estado=${this.estado}&pagina=${this.pagina}`;
        const respuesta = await fetch(url);

        if (respuesta.redirected) {
          this.error = "Tu sesión venció, volvé a iniciar sesión";
          return;
        }
        if (!respuesta.ok) {
          this.error = `No se pudieron cargar las reservas (código ${respuesta.status})`;
          return;
        }

        const datos = await respuesta.json();
        this.reservas = datos.items;
        this.totalPaginas = datos.totalPaginas;
        this.pagina = datos.pagina;
        this.error = "";

        if (this.reservas.length === 0 && this.pagina > 1) {
          this.pagina--;
          await this.cargarReservas();
        }
      } catch {
        this.error = "No se pudieron cargar las reservas";
      }
    },

    buscarConPausa() {
      clearTimeout(this.temporizador);
      this.temporizador = setTimeout(() => {
        this.pagina = 1;
        this.cargarReservas();
      }, 300);
    },

    cambiarFiltro() {
      this.pagina = 1;
      this.cargarReservas();
    },

    irAPagina(n) {
      if (n < 1 || n > this.totalPaginas) return;
      this.pagina = n;
      this.cargarReservas();
    },

    async confirmar(r) {
      if (
        !confirm(
          `¿Confirmar la reserva de ${r.cliente} (${this.formatearFecha(r.fecha)} ${r.horaInicio})?`,
        )
      )
        return;
      await this.accion(`/GestionReservas/Confirmar/${r.idReserva}`);
    },

    async cancelar(r) {
      const verbo = r.estado === "Pendiente" ? "Rechazar" : "Cancelar";
      if (
        !confirm(
          `¿${verbo} la reserva de ${r.cliente} (${this.formatearFecha(r.fecha)} ${r.horaInicio})? El turno quedará libre.`,
        )
      )
        return;
      await this.accion(`/GestionReservas/Cancelar/${r.idReserva}`);
    },

    async accion(url) {
      this.error = "";
      try {
        const respuesta = await fetch(url, {
          method: "POST",
          headers: { RequestVerificationToken: tokenAntiforgery() },
        });

        if (respuesta.redirected) {
          this.error = "Tu sesión venció, volvé a iniciar sesión";
          return;
        }
        if (!respuesta.ok) {
          let detalle = `No se pudo completar la operación (código ${respuesta.status})`;
          try {
            detalle = (await respuesta.json()).mensaje || detalle;
          } catch {}
          this.error = detalle;
        }
      } catch {
        this.error = "No se pudo completar la operación";
      }
      await this.cargarReservas();
    },

    claseEstado(estado) {
      if (estado === "Confirmada") return "bg-success";
      if (estado === "Pendiente") return "bg-warning text-dark";
      if (estado === "Cancelada") return "bg-danger";
      return "bg-secondary";
    },

    formatearFecha(f) {
      const [a, m, d] = f.split("-");
      return `${d}/${m}/${a}`;
    },

    formatearPrecio(p) {
      return Number(p).toLocaleString("es-AR", {
        style: "currency",
        currency: "ARS",
      });
    },
  },
}).mount("#app");
