const { createApp } = Vue;

const app = document.getElementById("appReservas");

const idRecurso = app.dataset.recursoId;
const urlDisponibilidad = app.dataset.disponibilidadUrl;
const urlConfirmar = app.dataset.confirmarUrl;

createApp({
  data() {
    const ahora = new Date();

    const hoy = [
      ahora.getFullYear(),
      String(ahora.getMonth() + 1).padStart(2, "0"),
      String(ahora.getDate()).padStart(2, "0"),
    ].join("-");

    return {
      confirmando: false,
      mensaje: "",
      errorConfirmacion: "",
      idRecurso: idRecurso,
      hoy: hoy,
      fecha: hoy,
      turnos: [],
      turnoSeleccionado: null,
      cargando: false,
      error: "",
    };
  },

  mounted() {
    this.cargarTurnos();
  },

  watch: {
    fecha() {
      this.turnoSeleccionado = null;
      this.cargarTurnos();
    },
  },

  methods: {
    async cargarTurnos() {
      if (!this.fecha) return;

      this.cargando = true;
      this.error = "";
      this.turnos = [];

      try {
        const url =
          urlDisponibilidad +
          "?id=" +
          this.idRecurso +
          "&fecha=" +
          encodeURIComponent(this.fecha);

        const respuesta = await fetch(url);

        if (!respuesta.ok) {
          throw new Error("No se pudo consultar la disponibilidad.");
        }

        this.turnos = await respuesta.json();
      } catch (e) {
        this.error = e.message;
      } finally {
        this.cargando = false;
      }
    },

    seleccionarTurno(turno) {
      this.turnoSeleccionado = turno;
    },

    formatearPrecio(precio) {
      return new Intl.NumberFormat("es-AR", {
        style: "currency",
        currency: "ARS",
      }).format(precio);
    },
    async confirmarReserva() {
      if (!this.turnoSeleccionado) {
        this.errorConfirmacion = "Primero selecciona un turno";
        return;
      }

      this.confirmando = true;
      this.mensaje = "";
      this.errorConfirmacion = "";

      try {
        const token = app.querySelector(
          'input[name="__RequestVerificationToken"]',
        ).value;

        const datos = new URLSearchParams();

        datos.append("idRecurso", this.idRecurso);
        datos.append("fecha", this.fecha);
        datos.append("horaInicio", this.turnoSeleccionado.horaInicio + ":00");

        const respuesta = await fetch(urlConfirmar, {
          method: "POST",
          headers: {
            "Content-Type": "application/x-www-form-urlencoded",
            RequestVerificationToken: token,
          },
          body: datos,
        });

        if (respuesta.redirected) {
          throw new Error("Necesitás iniciar sesión para reservar");
        }
        const contenido = await respuesta.text();

        if (!respuesta.ok) {
          let detalle = "No se pudo realizar la reserva";

          try {
            const resultado = JSON.parse(contenido);
            detalle = resultado.mensaje || detalle;
          } catch {
            if (respuesta.status === 401 || respuesta.redirected) {
              detalle = "Necesitas iniciar sesion para reservar";
            }
          }

          throw new Error(detalle);
        }

        const resultado = JSON.parse(contenido);

        this.mensaje =
          resultado.mensaje + " Numero de reserva: " + resultado.idReserva;

        this.turnoSeleccionado = null;

        await this.cargarTurnos();
      } catch (e) {
        this.errorConfirmacion = e.message;
      } finally {
        this.confirmando = false;
      }
    },
  },
}).mount("#appReservas");
