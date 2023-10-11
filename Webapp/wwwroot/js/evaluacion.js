class ExamenTimer {

	constructor() {
		this.inicio = null;
		this.fin = null;
		this.tiempo = null;
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

	resetTime() {
		this.inicio = null;
		this.fin = null;
		this.tiempo = null;
		return this
	}

	restart() {
		this.resetTime();
		this.start();
		return this;
	}

}

class LinkObserver {

	constructor() {
		this.usage = null;
		this.cache = {};
		this.ensureData()
	}

	reset() {
		this.usage = null;
		this.cache = {};
	}

	linkTxt(e) {
		let txt = e.target.innerText || e.target.href
		return txt.replace(/\s+/g, ' ').trim().substring(0, 80);
	}

	ensureData() {
		if (!this.usage) {
			this.usage = {
				clickLinks: {},
				hoverLinks: {},
				clickFiles: {},
				hoverFiles: {}
			}
		}
	}

	instrumentar(selector) {
		let self = this;
		let elem = $(selector);
		this.cache = {};

		elem.on("mouseenter", "a", function (e) {
			e.preventDefault();
			let item = self.linkTxt(e);
			self.iniciar(item, 'l:')
			//console.log('ENTER', e.target.href);
		});

		elem.on("mouseleave", "a", function (e) {
			e.preventDefault();
			let item = self.linkTxt(e);
			let t = self.finalizar(item, 'l:')
			if (t !== null)
				self.hover('link', item, t)
			//console.log('LEAVE', e.target.href);
		});

		elem.on("click", "a", function (e) {
			e.preventDefault();
			let item = self.linkTxt(e);
			self.click('link', item)
			//console.log('CLICK', e.target.href);
		});
	}

	iniciar(link, prefijo = '') {
		let key = prefijo + link
		this.cache[key] = new Date();
	}

	finalizar(link, prefijo = '') {
		let key = prefijo + link
		if (!this.cache[key])
			return null;
		let inicio = this.cache[key]
		delete this.cache[key]
		return (new Date() - inicio) / 1000;
	}

	click(tipo, link) {
		this.ensureData()
		let nombre = tipo === 'link' ? 'clickLinks' : 'clickFiles';
		if (!this.usage[nombre][link])
			this.usage[nombre][link] = 0;
		this.usage[nombre][link]++;
	}

	hover(tipo, link, tiempo) {
		this.ensureData()
		let nombre = tipo === 'link' ? 'hoverLinks' : 'hoverFiles';
		this.usage[nombre][link] = tiempo;
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
			links: null,
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
		this.links = new LinkObserver();
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
			ev.preventDefault()
			this.links.click('file', a.name)
		},
		enterFile(a, ev) {
			ev.preventDefault()
			this.links.iniciar(a.name, 'f:')
		},
		exitFile(a, ev) {
			ev.preventDefault()
			let t = this.links.finalizar(a.name, 'f:')
			if (t !== null)
				this.links.hover('file', a.name, t)
		},
		// fin eventos archivos

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
					this.timer.restart();
				})
			}

			if (r.accion === "cuestionario") {
				this.datos.pregunta = {};
				this.datos.cuest = r.data
				this.modo = 'cuestionario'
				Vue.nextTick(() => {
					this.timer.restart();
				});
			}

			$('#app button').prop('disabled', false);
			$('#preloader').hide();
		},
		procesarHtml() {
			let html = this.pregunta.html;
			html = html.replaceAll('{imgroot}', this.urlImagen);
			$('#contenido').html(html);
			this.links.reset();
			this.links.instrumentar('#contenido');
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
			r.interaccion = JSON.stringify(this.links.usage)
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
				//console.log(ev);
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
