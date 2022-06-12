'use strict';

var app = angular.module('DashboardApp', ['highcharts-ng', 'ui.bootstrap']);

// Specify SignalR server URL here for supporting CORS
app.value('signalRServer', '');
//app.value('signalRServer', 'http://myserver.com:7778/');