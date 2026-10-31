const { createApp } = Vue;

function formVacio() {
  return {
    idNegocio: 0,
    idCategoria: 0,
    idUsuario: 0,
    nombre: "",
    descripcion: "",
    direccion: "",
    ciudad: "",
    telefono: "",
  };
}

function tokenAntiforgery() {
  return document.querySelector('input[name="__RequestVerificationToken"]')
    .value;
}

createApp({
  data() {
    return {
      esSuperAdmin:
        document.getElementById("app").dataset.esSuperadmin === "true",
      negocios: [],
      categorias: [],
      duenioBuscar: "",
      duenioResultados: [],
      temporizadorDuenio: null,
      duenioNombre: "",
      cambiandoDuenio: false,
      buscar: "",
      pagina: 1,
      totalPaginas: 0,
      mostrarForm: false,
      form: formVacio(),
      errores: [],
      temporizador: null,
    };
  },

  mounted() {
    this.cargarCategorias();
    this.cargarNegocios();
  },

  methods: {
    async cargarCategorias() {
      const respuesta = await fetch("/Negocios/ListarCategorias");
      this.categorias = await respuesta.json();
    },

    buscarDueniosConPausa() {
      clearTimeout(this.temporizadorDuenio);
      this.temporizadorDuenio = setTimeout(() => this.buscarDuenios(), 300);
    },

    async buscarDuenios() {
      const url = `/Negocios/BuscarDuenios?buscar=${encodeURIComponent(this.duenioBuscar)}`;
      const respuesta = await fetch(url);
      this.duenioResultados = await respuesta.json();
    },

    elegirDuenio(d) {
      this.form.idUsuario = d.idUsuario;
      this.duenioNombre = d.nombreCompleto;
      this.cambiandoDuenio = false;
      this.duenioBuscar = "";
      this.duenioResultados = [];
    },

    cambiarDuenio() {
      this.cambiandoDuenio = true;
      this.duenioBuscar = "";
      this.buscarDuenios();
    },

    async cargarNegocios() {
      const url = `/Negocios/Listar?buscar=${encodeURIComponent(this.buscar)}&pagina=${this.pagina}`;
      const respuesta = await fetch(url);
      const datos = await respuesta.json();
      this.negocios = datos.items;
      this.totalPaginas = datos.totalPaginas;
      this.pagina = datos.pagina;
    },

    buscarConPausa() {
      clearTimeout(this.temporizador);
      this.temporizador = setTimeout(() => {
        this.pagina = 1;
        this.cargarNegocios();
      }, 300);
    },

    irAPagina(n) {
      if (n < 1 || n > this.totalPaginas) return;
      this.pagina = n;
      this.cargarNegocios();
    },

    nuevo() {
      this.form = formVacio();
      this.errores = [];
      this.duenioNombre = "";
      this.cambiandoDuenio = false;
      this.duenioBuscar = "";
      this.duenioResultados = [];
      this.mostrarForm = true;
    },

    editar(n) {
      this.form = {
        idNegocio: n.idNegocio,
        idCategoria: n.idCategoria,
        idUsuario: n.idUsuario,
        nombre: n.nombre,
        descripcion: n.descripcion ?? "",
        direccion: n.direccion ?? "",
        ciudad: n.ciudad ?? "",
        telefono: n.telefono ?? "",
      };
      this.errores = [];
      this.duenioNombre = n.duenio ?? "";
      this.cambiandoDuenio = false;
      this.duenioBuscar = "";
      this.duenioResultados = [];
      this.mostrarForm = true;
    },

    cancelar() {
      this.mostrarForm = false;
    },

    async guardar() {
      if (
        this.esSuperAdmin &&
        (this.cambiandoDuenio || this.form.idUsuario === 0)
      ) {
        this.errores = ["Elegí un dueño de la lista."];
        return;
      }
      const respuesta = await this.post("/Negocios/Guardar", this.form);
      if (respuesta.ok) {
        this.mostrarForm = false;
        await this.cargarNegocios();
        return;
      }
      this.errores = await this.leerErrores(respuesta);
    },

    async baja(n) {
      if (!confirm(`Dar de baja el negocio? "${n.nombre}"?`)) return;
      const respuesta = await this.post(`/Negocios/Baja/${n.idNegocio}`);
      if (respuesta.ok) {
        await this.cargarNegocios();
      } else {
        alert((await this.leerErrores(respuesta)).join("\n"));
      }
    },

    async reactivar(n) {
      const respuesta = await this.post(`/Negocios/Reactivar/${n.idNegocio}`);
      if (respuesta.ok) {
        await this.cargarNegocios();
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
