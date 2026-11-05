const { createApp } = Vue;

const DIAS = [
  "Domingo",
  "Lunes",
  "Martes",
  "Miércoles",
  "Jueves",
  "Viernes",
  "Sábado",
];

function generarHoras() {
  const horas = [];
  for (let m = 0; m <= 24 * 60; m += 30) {
    const h = String(Math.floor(m / 60)).padStart(2, "0");
    const min = String(m % 60).padStart(2, "0");
    horas.push(`${h}:${min}`);
  }
  return horas;
}

function canchaVacia(idNegocio) {
  return {
    idRecurso: 0,
    idNegocio: idNegocio,
    nombre: "",
    descripcion: "",
    duracionMin: 60,
    precioManana: 8000,
    precioTarde: 10000,
    precioNoche: 15000,
  };
}

function tokenAntiforgery() {
  return document.querySelector('input[name="__RequestVerificationToken"]')
    .value;
}

createApp({
  data() {
    return {
      idNegocio: parseInt(document.getElementById("app").dataset.idNegocio),
      canchas: [],
      dias: DIAS,
      horas: generarHoras(),
      mostrarForm: false,
      form: canchaVacia(0),
      errores: [],
      formHorario: null,
      erroresHorario: [],
      formTarifa: null,
      erroresTarifa: [],
    };
  },

  computed: {
    horasDesde() {
      return this.horas.slice(0, this.horas.length - 1);
    },
  },

  mounted() {
    this.cargar();
  },

  methods: {
    async cargar() {
      const respuesta = await fetch(
        `/Canchas/Listar?idNegocio=${this.idNegocio}`,
      );
      if (!respuesta.ok) {
        alert((await this.leerErrores(respuesta)).join("\n"));
        return;
      }
      this.canchas = await respuesta.json();
    },

    formatearPrecio(valor) {
      return "$ " + Number(valor).toLocaleString("es-AR");
    },

    nuevaCancha() {
      this.form = canchaVacia(this.idNegocio);
      this.errores = [];
      this.mostrarForm = true;
    },

    editarCancha(c) {
      this.form = {
        idRecurso: c.idRecurso,
        idNegocio: this.idNegocio,
        nombre: c.nombre,
        descripcion: c.descripcion ?? "",
        duracionMin: c.duracionMin,
        precioManana: 0,
        precioTarde: 0,
        precioNoche: 0,
      };
      this.errores = [];
      this.mostrarForm = true;
    },

    cancelarCancha() {
      this.mostrarForm = false;
    },

    async guardarCancha() {
      const cuerpo = {
        idRecurso: this.form.idRecurso,
        idNegocio: this.idNegocio,
        nombre: this.form.nombre,
        descripcion: this.form.descripcion,
        duracionMin: Number(this.form.duracionMin) || 0,
        precioManana: Number(this.form.precioManana) || 0,
        precioTarde: Number(this.form.precioTarde) || 0,
        precioNoche: Number(this.form.precioNoche) || 0,
      };
      const respuesta = await this.post("/Canchas/GuardarCancha", cuerpo);
      if (respuesta.ok) {
        this.mostrarForm = false;
        await this.cargar();
        return;
      }
      this.errores = await this.leerErrores(respuesta);
    },

    async bajaCancha(c) {
      if (!confirm(`Dar de baja la cancha "${c.nombre}"?`)) return;
      await this.accion(`/Canchas/BajaCancha/${c.idRecurso}`);
    },

    async reactivarCancha(c) {
      await this.accion(`/Canchas/ReactivarCancha/${c.idRecurso}`);
    },

    nuevoHorario(c) {
      this.formTarifa = null;
      this.formHorario = {
        idHorarioAtencion: 0,
        idRecurso: c.idRecurso,
        dias: [],
        horaDesde: "08:00",
        horaHasta: "24:00",
      };
      this.erroresHorario = [];
    },

    editarHorario(c, h) {
      this.formTarifa = null;
      this.formHorario = {
        idHorarioAtencion: h.idHorarioAtencion,
        idRecurso: c.idRecurso,
        dias: [h.diaSemana],
        horaDesde: h.horaDesde,
        horaHasta: h.horaHasta,
      };
      this.erroresHorario = [];
    },

    cancelarHorario() {
      this.formHorario = null;
    },

    async guardarHorario() {
      const respuesta = await this.post(
        "/Canchas/GuardarHorario",
        this.formHorario,
      );
      if (respuesta.ok) {
        this.formHorario = null;
        await this.cargar();
        return;
      }
      this.erroresHorario = await this.leerErrores(respuesta);
    },

    async eliminarHorario(h) {
      if (!confirm("Eliminar este horario de atención?")) return;
      await this.accion(`/Canchas/EliminarHorario/${h.idHorarioAtencion}`);
    },

    nuevaTarifa(c) {
      this.formHorario = null;
      this.formTarifa = {
        idTarifa: 0,
        idRecurso: c.idRecurso,
        nombre: "",
        horaDesde: "08:00",
        horaHasta: "14:00",
        precioHora: 0,
      };
      this.erroresTarifa = [];
    },

    editarTarifa(c, t) {
      this.formHorario = null;
      this.formTarifa = {
        idTarifa: t.idTarifa,
        idRecurso: c.idRecurso,
        nombre: t.nombre,
        horaDesde: t.horaDesde,
        horaHasta: t.horaHasta,
        precioHora: t.precioHora,
      };
      this.erroresTarifa = [];
    },

    cancelarTarifa() {
      this.formTarifa = null;
    },

    async guardarTarifa() {
      const cuerpo = {
        ...this.formTarifa,
        precioHora: Number(this.formTarifa.precioHora) || 0,
      };
      const respuesta = await this.post("/Canchas/GuardarTarifa", cuerpo);
      if (respuesta.ok) {
        this.formTarifa = null;
        await this.cargar();
        return;
      }
      this.erroresTarifa = await this.leerErrores(respuesta);
    },

    async bajaTarifa(t) {
      if (!confirm(`Dar de baja la tarifa "${t.nombre}"?`)) return;
      await this.accion(`/Canchas/BajaTarifa/${t.idTarifa}`);
    },

    async reactivarTarifa(t) {
      await this.accion(`/Canchas/ReactivarTarifa/${t.idTarifa}`);
    },

    async accion(url) {
      const respuesta = await this.post(url);
      if (respuesta.ok) {
        await this.cargar();
      } else {
        alert((await this.leerErrores(respuesta)).join("\n"));
      }
    },

    async post(url, cuerpo) {
      return await fetch(url, {
        method: "POST",
        headers: {
          "Content-Type": "application/json",
          RequestVerificationToken: tokenAntiforgery(),
        },
        body: cuerpo ? JSON.stringify(cuerpo) : undefined,
      });
    },

    async leerErrores(respuesta) {
      try {
        const datos = await respuesta.json();
        return datos.errores;
      } catch {
        return [
          `No se pudo completar la operación (código ${respuesta.status})`,
        ];
      }
    },
  },
}).mount("#app");