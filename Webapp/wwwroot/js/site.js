﻿// Please see documentation at https://docs.microsoft.com/aspnet/core/client-side/bundling-and-minification
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

var _autonameVueCount = 0;
const vAutoname = {
	mounted(el, binding, vnode) {
		_autonameVueCount++;
		$(el).prop('name', '_auto' + _autonameVueCount);
	}
}

