const Cuestionario = {
	props: ['datos'],
	template: '#tplCuestionario',
	data() {
		return {
			error: false
		};
	},
	created() { },
	emits: ["respCuestionario"],
	methods: {
		continuar() {
			var valido = true;
			this.datos.preguntas.forEach(x => {
				x.error = x.respuesta ? null : 1;
				if (x.error)
					valido = false;
			});
			if (!valido) {
				this.error = true;
				return;
			}
			this.$emit('respCuestionario');
		},
	},
	mounted() {
		var i = 1;
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

const AppEvaluacion = {
	data() {
		return {
			model: null,
			baseUrl: null,
			urlImagen: '',
			verCuest: false,
			mensaje: {},
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
			cuest: null,
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
			return this.model.respuestas === 0;
		},
		badgeAvance() {
			var m = this.model;
			return `${m.respuestas} / ${m.total}`;
		}
	},
	created() {
		this.usage = {
			clickLinks: {},
			hoverLinks: {},
			clickFiles: {},
			hoverFiles: {}
		};
		this.cache = {}
	},
	methods: {
		resetUsage() {
			this.usage = {
				clickLinks: {},
				hoverLinks: {},
				clickFiles: {},
				hoverFiles: {}
			};
			this.cache = {}
		},
		claseResp(check) {
			return this.resp.respuesta === check ? 'btn-secondary' : 'btn-outline-secondary';
		},
		toggleRespuesta(valor) {
			const r = this.resp;
			if (r.respuesta === valor)
				return r.respuesta = null;
			r.respuesta = valor;
			delete r.errores.respuesta
		},
		loadCuestionario() {
			const self = this;
			$.post(this.baseUrl + '/cuestionario').then(r => {
				self.cuest = r;
				$('#preloader').hide();
			})
		},
		cuestionarioOK() {
			var preg = this.cuest.preguntas.map(x => {
				return { orden: x.orden, respuesta: x.respuesta, texto: x.texto }
			});
			const send = {
				data: JSON.stringify(preg)
			};
			const self = this;
			$.post(this.baseUrl + '/responderCuestionario', send).then(r => {
				if (r.error)
					return alert(r.error);
				self.cuest = null;
				self.avanzar();
				//window.location.reload();
			});
			return false;
		},
		verInstrucciones() {
			this.modalHelp.show();
		},
		verAyuda() {
			$('#popupHelp').modal('show');
		},
		procesarHtml() {
			const self = this;
			var html = this.mensaje.html;
			html = html.replaceAll('{imgroot}', this.urlImagen);
			$('#contenido').html(html);

			var u = this.usage;

			function linkTxt(e) {
				var txt = $(e.target).text() || ''
				return txt.replace(/\s+/g, ' ').trim().substring(0, 20);
			}

			//$('#contenido').on("mouseover", "a", function (e) {
			//	e.preventDefault();
			//	self.usage.hoverLink = true;
			//});

			$('#contenido').on("mouseenter", "a", function (e) {
				e.preventDefault();
				var txt = linkTxt(e)
				self.cache[txt] = new Date();
			});

			$('#contenido').on("mouseleave", "a", function (e) {
				//e.preventDefault();
				var txt = linkTxt(e)
				var item = self.cache[txt] || null;
				if (item) {
					var f = new Date();
					let secs = (new Date() - item) / 1000;
					u.hoverLinks[txt] = secs;
					delete self.cache[txt]
				}
			});


			$('#contenido').on("click", "a", function (e) {
				e.preventDefault();
				var txt = linkTxt(e)
				if (!u.clickLinks[txt])
					u.clickLinks[txt] = 0;
				u.clickLinks[txt]++;
			});
		},
		avanzar() {
			var self = this;
			var resp = this.resp;
			return jsonPost(this.baseUrl + '/avance').then(r => {
				console.log(r)
				if (r.cuestionario) {
					self.loadCuestionario();
					return
				}

				//if (r.accion == "fin")
				if (r.fin) {
					window.location.href = self.baseUrl + '/resultados';
					return;
				}

				console.log(r);
				self.respuestas = r.respuestas;

				resp.token = r.token;
				resp.preguntaId = r.preguntaId;
				resp.fin = null
				resp.respuesta = null
				resp.comentario = null
				resp.interaccion = null
				resp.errores = {}

				self.resetUsage();
				self.showEval = false;
				self.mensaje = r.mensaje || {}
				self.procesarHtml();

				resp.inicio = new Date();
				$('#preloader').hide();
			});
		},
		clickFile(a, ev) {
			ev.preventDefault();
			this.cache['f:' + a.name] = new Date()
			if (!this.usage.clickFiles[a.name])
				this.usage.clickFiles[a.name] = 0;
			this.usage.clickFiles[a.name]++
		},
		enterFile(a, ev) {
			var key = 'f:' + a.name
			this.cache[key] = new Date();
			console.log(a.name)
		},
		exitFile(a, ev) {
			var key = 'f:' + a.name
			var item = this.cache[key] || null;
			if (item) {
				var f = new Date();
				let secs = (new Date() - item) / 1000;
				this.usage.hoverFiles[a.name] = secs;
				delete this.cache[key]
			}
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

			r.fin = new Date();
			r.interaccion = JSON.stringify(this.usage)

			$('#app button').prop('disabled', true);

			$('#preloader').show();
			jsonPost(this.baseUrl + '/respuesta', r).then(r => {
				console.log(r);
				if (r.accion == "fin") {
					window.location.href = self.baseUrl + '/resultados';
					return;
				}
				$('#app button').prop('disabled', false);
				if (r.accion == "cuestionario") {
					self.loadCuestionario();
					return;
				}
				// cleanup
				self.avanzar();
			});
		},
	},
	mounted() {
		const self = this;

		if (this.model.cuestionario) {
			this.loadCuestionario();
			return;
		}

		this.modalHelp = new bootstrap.Modal('#popupIns',
			{
				keyboard: false,
				backdrop: 'static'
			});
		$('#popupIns')[0].addEventListener('hide.bs.modal',
			ev => {
				console.log(ev);
			});
		//if(this.esInicio) self.verInstrucciones();
		this.avanzar();

		$('#formEval').validate({
			submitHandler: self.responder
		});
	}
};