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

const globalDirectives = {
	install(app, options) {
		app.directive('autoname', vAutoname);

		app.mixin({
			methods: {
				formatFecha: function (val, formato = '') {
					if (!val) return val;
					var m = new Date(val);
					if (isNaN(m))
						return val;
					if (formato === 'hora')
						return moment(m).format('YYYY-MM-DD HH:mm:ss');
					return moment(m).format('YYYY-MM-DD');
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