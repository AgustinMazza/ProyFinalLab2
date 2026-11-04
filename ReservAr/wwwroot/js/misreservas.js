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
                const respuesta = await fetch(`/Reservas/ListarMias?pagina=${this.pagina}`);
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

        formatearFecha(texto) {
            const [anio, mes, dia] = texto.split("-");
            return `${dia}/${mes}/${anio}`;
        },

        claseEstado(estado) {
            if (estado === "Próximamente") return "bg-success";
            return "bg-secondary";
        },
    },
}).mount("#app");