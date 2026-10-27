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
            esSuperAdmin: document.getElementById("app").dataset.esSuperadmin === "true",
            negocios: [],
            categorias: [],
            duenios: [],
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
        if (this.esSuperAdmin) this.cargarDuenios();
        this.cargarNegocios();
    },

    methods: {
        async cargarCategorias() {
            const respuesta = await fetch("/Negocios/ListarCategorias");
            this.categorias = await respuesta.json();
        },

        async cargarDuenios() {
            const respuesta = await fetch("/Negocios/ListarDuenios");
            this.duenios = await respuesta.json();
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
            this.mostrarForm = true;
        },

        cancelar() {
            this.mostrarForm = false;
        },

        async guardar() {
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