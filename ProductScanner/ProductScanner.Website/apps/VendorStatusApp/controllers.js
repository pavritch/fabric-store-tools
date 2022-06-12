'use strict';

function VendorStatusController($scope, signalRHubProxy, $http) {
    $scope.sortOrderAsc = false;
    $scope.sortField = "VendorName";
    $scope.refreshButtonText = "Refresh Vendor Status";

    var clientPushHubProxy = signalRHubProxy(signalRHubProxy.defaultServer, 'monitorHub');
    clientPushHubProxy.on('pushStatuses', function(data) {
        var sortedData = _.sortBy(data, function (x) { return x[$scope.sortField]; });
        if ($scope.sortOrderAsc) sortedData = sortedData.reverse();
        $scope.vendorStatuses = sortedData;
    });

    $scope.formatAvailable = function(available, capabilities) {
        if (capabilities == 'None') return '';
        return available ? "Yes" : "No";
    };

    $scope.setSortField = function(fieldName) {
        if ($scope.sortField == fieldName) $scope.sortOrderAsc = !$scope.sortOrderAsc;
        $scope.sortField = fieldName;
    }

    $scope.refreshVendorStatus = function() {
        var url = '/api/1.0/InsideFabric/refresh/';
        $scope.refreshButtonText = "Refreshing...";
        $http.get(url).success(function() {
            $scope.refreshButtonText = "Refresh Vendor Status";
        });
    }

    $scope.getRowColor = function(status) {
        if (!status.Available)
            return { 'background-color': '#f1805c' };
        return {};
    };

    $scope.toFormattedDate = function (s) {
        if (s == null) return "";
        var date = new Date(Date.parse(s));
        return date.toHHMMSS() + ' ' + (date.getMonth()+1) + "/" + date.getDate();
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