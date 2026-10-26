const { createApp } = Vue;

function formVacio() {
  return {
    idUsuario: 0,
    idRol: 0,
    nombre: "",
    apellido: "",
    email: "",
    telefono: "",
    password: "",
  };
}

function tokenAntiforgery() {
  return document.querySelector('input[name="__RequestVerificationToken"]')
    .value;
}

createApp({
  data() {
    return {
      usuarios: [],
      roles: [],
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
    this.cargarRoles();
    this.cargarUsuarios();
  },

  methods: {
    async cargarRoles() {
      const respuesta = await fetch("/Usuarios/ListarRoles");
      this.roles = await respuesta.json();
    },

    async cargarUsuarios() {
      const url = `/Usuarios/Listar?buscar=${encodeURIComponent(this.buscar)}&pagina=${this.pagina}`;
      const respuesta = await fetch(url);
      const datos = await respuesta.json();
      this.usuarios = datos.items;
      this.totalPaginas = datos.totalPaginas;
      this.pagina = datos.pagina;
    },

    buscarConPausa() {
      clearTimeout(this.temporizador);
      this.temporizador = setTimeout(() => {
        this.pagina = 1;
        this.cargarUsuarios();
      }, 300);
    },

    irAPagina(n) {
      if (n < 1 || n > this.totalPaginas) return;
      this.pagina = n;
      this.cargarUsuarios();
    },

    nuevo() {
      this.form = formVacio();
      this.errores = [];
      this.mostrarForm = true;
    },

    editar(u) {
      this.form = {
        idUsuario: u.idUsuario,
        idRol: u.idRol,
        nombre: u.nombre,
        apellido: u.apellido,
        email: u.email,
        telefono: u.telefono ?? "",
        password: "",
      };
      this.errores = [];
      this.mostrarForm = true;
    },

    cancelar() {
      this.mostrarForm = false;
    },

    async guardar() {
      const respuesta = await this.post("/Usuarios/Guardar", this.form);
      if (respuesta.ok) {
        this.mostrarForm = false;
        await this.cargarUsuarios();
        return;
      }
      this.errores = await this.leerErrores(respuesta);
    },

    async baja(u) {
      if (!confirm(`¿Dar de baja a ${u.nombre} ${u.apellido}?`)) return;
      const respuesta = await this.post(`/Usuarios/Baja/${u.idUsuario}`);
      if (respuesta.ok) {
        await this.cargarUsuarios();
      } else {
        alert((await this.leerErrores(respuesta)).join("\n"));
      }
    },

    async reactivar(u) {
      const respuesta = await this.post(`/Usuarios/Reactivar/${u.idUsuario}`);
      if (respuesta.ok) {
        await this.cargarUsuarios();
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
