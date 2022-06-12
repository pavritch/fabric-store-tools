'use strict';

function MainController($scope, signalRHubProxy, $http) {
    var clientPushHubProxy = signalRHubProxy(signalRHubProxy.defaultServer, 'monitorHub');
    clientPushHubProxy.on('pushRequests', function(data) {
        $scope.setTimeframeData(data, $scope.selectedInterval + "Timeline");
    });

    clientPushHubProxy.on('pushNotifications', function(requests) {
        $scope.recentRequests = requests;
    });

    clientPushHubProxy.on('pushServerInfo', function(serverInfo) {
        $scope.requestsPerMinute = serverInfo.RequestsPerMinute;
        $scope.lastLogin = moment(serverInfo.VendorSessionInfo.LastLogin).format('HH:mm:ss');
        $scope.authenticationFailures = serverInfo.AuthenticationFailures;
    });

    $scope.setTimeframeData = function(data, timeframe) {
        $scope.requestsChart.series[0].data = data.CacheHits[timeframe];
        $scope.requestsChart.series[1].data = data.VendorRequests[timeframe];

        $scope.byStatusChart.series[0].data = data.RequestsByStatus.Unavailable[timeframe];
        $scope.byStatusChart.series[1].data = data.RequestsByStatus.Discontinued[timeframe];
        $scope.byStatusChart.series[2].data = data.RequestsByStatus.OutOfStock[timeframe];
        $scope.byStatusChart.series[3].data = data.RequestsByStatus.PartialStock[timeframe];
        $scope.byStatusChart.series[4].data = data.RequestsByStatus.InStock[timeframe];

        $scope.cachePercentage = data.CachePercentage;
        $scope.totalRequests = data.TotalRequests;
    };

    $http.get('/api/1.0/InsideFabric/vendors/').success(function(vendors) {
        $scope.vendors = _.filter(vendors, function (vendor) { return vendor.StockCapabilities != 'None'; });
        $scope.vendors.unshift({DisplayName: "All"});
    });

    $scope.requestsPerMinute = 0;
    $scope.cachePercentage = 0;
    $scope.totalRequests = 0;
    $scope.selectedInterval = "Minutes";
    $scope.selectedVendor = 'All';

    $scope.selectVendor = function(vendor) {
        $scope.selectedVendor = vendor;
        clientPushHubProxy.invoke('SetSelectedVendor', $scope.selectedVendor);
    }

    $scope.getRowColor = function (request) {
        var status = request.StockCheckStatus;
        var color = $scope.colors[status];
        return {
            'background-color': color
        }
    }

    $scope.colors = {
        "InStock": '#90ed7d',
        "PartialStock": '#7cb5ec',
        "Unavailable": '#f1805c',
        "OutOfStock": '#f7a35c',
        "Discontinued": '#CFCFC4'
    };

    $scope.toFormattedTime = function (s) {
        if (s == null) return "";
        var date = new Date(Date.parse(s));
        return date.toHHMMSS();
    }

    $scope.toFormattedDate = function (s) {
        if (s == null) return "";
        var date = new Date(Date.parse(s));
        return date.toHHMMSS() + ' ' + (date.getMonth()+1) + "/" + date.getDate();
    }

    // chart configuration
    $scope.requestsChart = {
        options: {
            chart: {
                type: 'line'
            },
            plotOptions: {
                column: {
                    stacking: 'normal'
                }
            },
            legend: {
                verticalAlign: 'bottom'
            },
            xAxis: {
                labels: {
                    formatter: function() {
                        var width = $scope.requestsChart.series[0].data;
                        return Math.abs(width.length - 1 - this.value);
                    }
                }
            },
            yAxis: {
                allowDecimals: false,
                title: {
                    text: ''
                }
            }
        },
        series: [{
            name: 'Cache Hits',
        }, {
            name: 'Vendor Requests',
        }],
        title: {
            text: ''
        },
        loading: false
    }
    $scope.byStatusChart = {
        options: {
            chart: {
                type: 'column'
            },
            plotOptions: {
                column: {
                    stacking: 'normal'
                }
            },
            legend: {
                verticalAlign: 'top'
            },
            xAxis: {
                labels: {
                    formatter: function() {
                        var width = $scope.byStatusChart.series[0].data;
                        return Math.abs(width.length - 1 - this.value);
                    }
                }
            },
            yAxis: {
                allowDecimals: false,
                title: {
                    text: ''
                }
            }
        },
        //Unavailable, 
        series: [{
            name: 'Unavailable',
            color: $scope.colors['Unavailable']
        }, {
            name: 'Discontinued',
            color: $scope.colors['Discontinued']
        }, {
            name: 'Out of Stock',
            color: $scope.colors['OutOfStock']
        }, {
            name: 'Partial Stock',
            color: $scope.colors['PartialStock']
        }, {
            name: 'In Stock',
            color: $scope.colors['InStock']
        }],
        title: {
            text: ''
        },
        loading: false
    }
};

Date.prototype.toHHMMSS = function() {
    var hours = this.getHours();
    var minutes = this.getMinutes();
    var seconds = this.getSeconds();

    if (hours < 10) {
        hours = "0" + hours;
    }
    if (minutes < 10) {
        minutes = "0" + minutes;
    }
    if (seconds < 10) {
        seconds = "0" + seconds;
    }
    var time = hours + ':' + minutes + ':' + seconds;
    return time;
}