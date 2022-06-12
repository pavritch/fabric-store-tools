'use strict';

function RequestTestController($scope, $http) {
    $scope.stockChecks = [{}];
    $scope.allResults = [];
    $scope.requestNum = 0;
    $scope.tableColors = ['#B0E57C', '#FFEC94', '#9BD1FA', '#FFFFFF'];
    $scope.isLoading = false;

    $scope.addCheck = function () {
        $scope.stockChecks.push({});
    }

    $scope.removeCheck = function () {
        $scope.stockChecks.pop();
    }

    $scope.toFormattedDate = function (s) {
        if (s == null) return "";
        var date = new Date(Date.parse(s));
        return date.toHHMMSS();
    }

    $scope.getColor = function (res) {
        var color = $scope.tableColors[res.RequestNum % $scope.tableColors.length];
        return {
            'background-color': color
        }
    }

    $scope.submitProductRequest = function () {
        var url = '/api/1.0/InsideFabric/stockcheck/';
        $scope.isLoading = true;
        $http.post(url, $scope.stockChecks).success(function (results) {
            $scope.requestNum++;
            angular.forEach(results, function (value) {
                value.RequestNum = $scope.requestNum;
            });
            $scope.allResults = results.concat($scope.allResults);
            $scope.isLoading = false;
        });
    }
};

Date.prototype.toHHMMSS = function () {
    var hours = this.getHours();
    var minutes = this.getMinutes();
    var seconds = this.getSeconds();

    if (hours < 10) { hours = "0" + hours; }
    if (minutes < 10) { minutes = "0" + minutes; }
    if (seconds < 10) { seconds = "0" + seconds; }
    var time = hours + ':' + minutes + ':' + seconds;
    return time;
}