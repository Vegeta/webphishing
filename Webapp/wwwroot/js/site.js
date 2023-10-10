// Please see documentation at https://docs.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.

function jsonPost(url, data) {
	return $.ajax({
		type: "POST",
		data: JSON.stringify(data),
		url: url,
		contentType: "application/json charset=utf-8",
	})
}

function num(x) {
	var f = parseFloat(x);
	return isNaN(f) ? 0 : f;
}

var _autonameVueCount = 0;
const vAutoname = {
	mounted(el, binding, vnode) {
		_autonameVueCount++;
		$(el).prop('name', '_auto' + _autonameVueCount);
	}
}

const formatoFechaFull = 'dddd, D [de] MMMM, YYYY h:mm A';

const globalDirectives = {
	install(app, options) {
		app.directive('autoname', vAutoname);

		app.mixin({
			data() {
				return {}
			},
			created() {
				// no reactive
				this.formatoFechaFull = formatoFechaFull
			},
			methods: {
				numero: function (val, decimal = 2) {
					const res = parseFloat(val)
					if (isNaN(res))
						return null
					return res.toFixed(decimal)
				},
				fecha: function (val, formato) {
					if (!val) return val;

					if (formato === 'fechaHora')
						formato = 'YYYY-MM-DD h:mm A'

					const d = window.dayjs(val);
					if (!d)
						return '<ERROR>';
					return d.locale('es').format(formato || 'YYYY-MM-DD');
				},
				fechaHora(val) {
					return this.fecha(val, 'YYYY-MM-DD hh:mm:ss')
				},
				porcentaje: function (val, decimales = 0) {
					if (!val || val === '' || val === null || isNaN(val))
						return '';
					var n = num(val);
					var txt = '';
					if (decimales > 0)
						txt = n.toFixed(decimales);
					else
						txt = Math.round(n);
					txt += '%';
					return txt;
				},
				fileSize: function (val) {
					if (!val || val === '') return '';
					return fileSize(val);
				}
			}
		});
	}
}