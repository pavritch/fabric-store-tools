
$(function () {

    detailsComponent.initialize();
});


var detailsComponent = (function () {


    // external functions

    return {
        initialize: function () {

            console.log("Details component initialized.")
        },


        // display product by productID in details pane
        setProduct: function (id) {

            console.log("set product details: " + id)
        },

        clear: function () {

            console.log("clear details pane");
        },

    };
}());


