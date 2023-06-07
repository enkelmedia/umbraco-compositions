(function () {
    "use strict";

    console.log('Ctrl registered');

    function controller($scope, $http, navigationService, contentTypeResource) {

        var currentNode = $scope.currentNode;

        console.log($scope.currentNode);
        

        var vm = this;
        vm.compositionContentTypes = [];
        vm.compositionId = '';
        
        $http.get('/umbraco/backoffice/DocTypeToolbox/DocTypeToolbox/GetCompositions/' + $scope.currentNode.id).then(function (res) {
            console.log(res);
            vm.compositionContentTypes = res.data;
        });
        
        //contentTypeResource.getAll().then(function (data) {

        //    console.log('res from getall', data);
        //    vm.compositionContentTypes = data.filter(x => x.isElement);

        //});

        vm.text = 'Ctls was here';

        vm.append = function () {

            var model = {
                contentTypeId : currentNode.id,
                compositionId : vm.compositionId
            };

            $http.post('/umbraco/backoffice/DocTypeToolbox/DocTypeToolbox/ApplyComposition',model).then(function (res) {
                console.log(res);
                vm.success = true;
                vm.successMessage = 'Changes has been applied!';
            });
        }

        vm.close = function () {
            navigationService.hideDialog();
        }

        return vm;
    }

    angular.module("umbraco").controller("Umbraco.Community.DocTypeToolbox", ['$scope','$http', 'navigationService', 'contentTypeResource', controller]);
})();