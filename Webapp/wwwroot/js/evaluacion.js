class ExamenTimer {

	constructor() {
		this.inicio = null;
		this.fin = null;
		this.tiempo = null;
		this.cache = {};
		this.resetUsage()
	}

	resetUsage() {
		this.usage = {
			clickLinks: {},
			hoverLinks: {},
			clickFiles: {},
			hoverFiles: {}
		};
		this.cache = {}
		return this
	}

	resetTime() {
		this.inicio = null;
		this.fin = null;
		this.tiempo = null;
		return this
	}

	resetAll() {
		return this.resetTime().resetUsage();
	}

	instrumentar(selector) {
		const self = this;
		let usage = this.usage;

		// capturar interacciones con los vinculos del mensaje
		// mas abajo estan las interacciones con los archivos
		function linkTxt(e) {
			let txt = $(e.target).text() || ''
			return txt.replace(/\s+/g, ' ').trim().substring(0, 20);
		}

		$(selector).on("mouseenter", "a", function (e) {
			e.preventDefault();
			let txt = linkTxt(e)
			self.cache[txt] = new Date();
		});

		$(selector).on("mouseleave", "a", function (e) {
			//e.preventDefault();
			let txt = linkTxt(e)
			let inicio = self.cache[txt] || null;
			if (inicio) {
				usage.hoverLinks[txt] = (new Date() - inicio) / 1000;
				delete self.cache[txt]
			}
		});

		$(selector).on("click", "a", function (e) {
			e.preventDefault();
			let txt = linkTxt(e)
			if (!usage.clickLinks[txt])
				usage.clickLinks[txt] = 0;
			usage.clickLinks[txt]++;
		});
	}

	clickFile(file, ev) {
		ev.preventDefault();
		this.cache['f:' + file] = new Date()
		if (!this.usage.clickFiles[file])
			this.usage.clickFiles[file] = 0;
		this.usage.clickFiles[file]++
	}

	enterFile(file, ev) {
		let key = 'f:' + file
		this.cache[key] = new Date();
		console.log(file)
	}

	exitFile(file, ev) {
		let key = 'f:' + file
		let item = this.cache[key] || null;
		if (item) {
			this.usage.hoverFiles[file] = (new Date() - item) / 1000;
			delete this.cache[key]
		}
	}

	start() {
		this.inicio = new Date();
	}

	stop() {
		if (!this.inicio)
			return null;
		this.fin = new Date();
		this.tiempo = (this.fin - this.inicio) / 1000;
		return this.tiempo
	}

}

const Cuestionario = {
	props: ['datos'],
	template: '#tplCuestionario',
	data() {
		return {
			error: false
		};
	},
	created() {
	},
	emits: ["respCuestionario"],
	methods: {
		continuar() {
			let valido = true;
			this.datos.preguntas.forEach(x => {
				x.error = x.respuesta ? null : 1;
				if (x.error)
					valido = false;
			});
			if (!valido) {
				this.error = true;
				return;
			}
			this.$emit('respCuestionario', this.datos.preguntas);
		},
	},
	mounted() {
		let i = 1;
		this.datos.preguntas.forEach(x => {
			x.nombreRadio = `preg${i}`;
			x.error = null;
			i++;
		});
		const self = this;
		$('#formCuest').validate({
			submitHandler() {
				self.continuar()
				return false
			}
		});

	},
}

const CompExamen = {
	props: ['datos', 'baseUrl', 'simulacion', 'urlImagen', 'retroPath'],
	template: '#tplExamen',
	data() {
		return {
			resp: {
				inicio: null,
				fin: null,
				token: null,
				preguntaId: null,
				respuesta: null,
				comentario: null,
				interaccion: null,
				errores: {},
			},
			showEval: false,
			modo: '',
		}
	},
	watch: {
		'resp.comentario': function (val) {
			if (val)
				delete this.resp.errores.comentario;
		}
	},
	computed: {
		esInicio() {
			return this.datos.model.indice === 0 && !this.simulacion;
		},
		badgeAvance() {
			let m = this.model;
			return `${m.indice} / ${m.total}`;
		},
		cuest() { return this.datos.cuest },
		pregunta() { return this.datos.pregunta },
		model() { return this.datos.model },
	},
	created() {
		this.timer = new ExamenTimer();
	},
	methods: {
		resetRespuesta() {
			this.resp.inicio = null
			this.resp.fin = null
			this.resp.preguntaId = null
			this.resp.respuesta = null
			this.resp.comentario = null
			this.resp.interaccion = null
			this.resp.errores = {}
		},
		verInstrucciones() {
			this.modalHelp.show();
		},
		verAyuda() {
			$('#popupHelp').modal('show');
		},

		toggleRespuesta(valor) {
			const r = this.resp;
			if (r.respuesta === valor)
				return r.respuesta = null;
			r.respuesta = valor;
			delete r.errores.respuesta
		},
		// paso a control de archivos
		clickFile(a, ev) {
			this.timer.clickFile(a.name, ev);
		},
		enterFile(a, ev) {
			this.timer.enterFile(a.name, ev);
		},
		exitFile(a, ev) {
			this.timer.exitFile(a.name, ev);
		},

		claseResp(check) {
			return this.resp.respuesta === check ? 'btn-secondary' : 'btn-outline-secondary';
		},
		cuestionarioOK(respuestas) {
			const send = {
				respuestas: respuestas,
				estado: this.model.estado || null,
				tiempoCuest: this.timer.stop(),
			};
			const self = this;
			jsonPost(this.baseUrl + '/responderCuestionario', send).then(r => {
				self.procesarRespuesta(r)
			});
			return false;
		},
		procesarRespuesta(r) {
			console.log(r);
			if (r.error)
				return alert(r.error);
			if (r.estado)
				this.model.estado = r.estado
			if (r.indice)
				this.model.indice = r.indice
			this.resetRespuesta();

			if (r.accion === "fin") {
				if (this['verResultado'])
					this.verResultado(r.data)
				else
					alert("Fin del examen")
			}

			if (r.accion === "pregunta") {
				this.datos.cuest = null;
				this.datos.pregunta = r.data;
				this.modo = 'pregunta'
				this.showEval = false;
				Vue.nextTick(() => {
					this.procesarHtml()
					this.timer.resetAll().start();
				})
			}

			if (r.accion === "cuestionario") {
				this.datos.pregunta = {};
				this.datos.cuest = r.data
				this.modo = 'cuestionario'
				this.timer.resetAll().start();
			}

			$('#app button').prop('disabled', false);
			$('#preloader').hide();
		},
		procesarHtml() {
			let html = this.pregunta.html;
			html = html.replaceAll('{imgroot}', this.urlImagen);
			$('#contenido').html(html);
			this.timer.instrumentar('#contenido')
		},
		responder() {
			const self = this;
			const r = this.resp;
			let error = false;
			if (!r.respuesta) {
				r.errores['respuesta'] = true;
				error = true;
			}
			if (!r.comentario) {
				r.errores['comentario'] = true;
				error = true;
			}

			if (error)
				return false;

			const timer = this.timer;
			timer.stop();
			r.inicio = timer.inicio;
			r.fin = timer.fin;
			r.interaccion = JSON.stringify(timer.usage)
			r.preguntaId = this.pregunta.id
			r.token = this.model.token
			if (this.model.estado)
				r.estado = this.model.estado

			$('#app button').prop('disabled', true);

			$('#preloader').show();
			jsonPost(this.baseUrl + '/respuesta', r).then(r => {
				self.procesarRespuesta(r)
			});
			return false;
		},
	},
	mounted() {
		const self = this;

		this.modalHelp = new bootstrap.Modal('#popupIns',
			{
				keyboard: false,
				backdrop: 'static'
			});

		$('#popupIns')[0].addEventListener('hide.bs.modal',
			ev => {
				console.log(ev);
			});

		$('#formEval').validate({
			submitHandler: self.responder
		});

		if (this.esInicio && !this.cuest) {
			Vue.nextTick(() => {
				self.verInstrucciones();
			})
		}

		let init = {
			accion: '',
			data: null,
			indice: this.model.indice
		}

		if (this.cuest) {
			init.accion = 'cuestionario'
			init.data = this.cuest
		}

		if (this.pregunta && this.pregunta.id) {
			init.accion = 'pregunta'
			init.data = this.pregunta
		}

		if (init.accion)
			this.procesarRespuesta(init);

	}
}
