const { createApp } = Vue;

function tokenAntiforgery() {
  return document.querySelector('input[name="__RequestVerificationToken"]')
    .value;
}

createApp({
  data() {
    return {
      peticiones: [],
      buscar: "",
      pagina: 1,
      totalPaginas: 0,
      temporizador: null,
    };
  },

  mounted() {
    this.cargarPeticiones();
  },

  methods: {
    async cargarPeticiones() {
      const url = `/Peticiones/Listar?buscar=${encodeURIComponent(this.buscar)}&pagina=${this.pagina}`;
      const respuesta = await fetch(url);
      const datos = await respuesta.json();
      this.peticiones = datos.items;
      this.totalPaginas = datos.totalPaginas;
      this.pagina = datos.pagina;

      if (this.peticiones.length === 0 && this.pagina > 1) {
        this.pagina--;
        await this.cargarPeticiones();
      }
    },

    buscarConPausa() {
      clearTimeout(this.temporizador);
      this.temporizador = setTimeout(() => {
        this.pagina = 1;
        this.cargarPeticiones();
      }, 300);
    },

    irAPagina(n) {
      if (n < 1 || n > this.totalPaginas) return;
      this.pagina = n;
      this.cargarPeticiones();
    },

    async aprobar(p) {
      if (
        !confirm(`¿Aprobar "${p.nombre}"? ${p.solicitante} pasará a ser Admin.`)
      )
        return;
      await this.resolver(`/Peticiones/Aprobar/${p.idNegocio}`);
    },

    async rechazar(p) {
      if (!confirm(`¿Rechazar la petición "${p.nombre}"?`)) return;
      await this.resolver(`/Peticiones/Rechazar/${p.idNegocio}`);
    },

    async resolver(url) {
      const respuesta = await this.post(url);
      if (respuesta.ok) {
        await this.cargarPeticiones();
      } else {
        alert((await this.leerErrores(respuesta)).join("\n"));
      }
    },

    async post(url) {
      return await fetch(url, {
        method: "POST",
        headers: {
          RequestVerificationToken: tokenAntiforgery(),
        },
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
