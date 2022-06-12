
$(function () {

    productsComponent.initialize();
});



var smallTilesProvider = (function () {

    var productHtmlContainer; // where we inject product listing results
    var gridster = null;

    function changePage(targetPage) {
        productsComponent(targetPage);
    }


    // external functions

    return {
        initialize: function(container)
        {
            productHtmlContainer = container;
        },

        loadHtml: function (html) {

            // good to go, we have an html fragment
            productsHtmlContainer.html(html);
            
            gridster = productHtmlContainer.find("#productsTilesWrapper ul").gridster({
                widget_selector: "li",
                widget_margins: [5, 5],
                widget_base_dimensions: [150, 150],
                extra_rows: 0,
                extra_cols: 0,
                max_cols: null,
                min_cols: 1,
                min_rows: 1,
                max_size_x: 1,
                autogrow_cols: true,
                autogenerate_stylesheet: true,
                avoid_overlapped_widgets: true,

            }).data('gridster');
            // no drag and drop
            gridster.disable();

            // hook in menu events

            // hook up pager

            productsHtmlContainer.find("#productsPagerWrapper a.pgrButton").click(function (event) {
                    event.preventDefault();

                    var targetPage = parseInt($(this).attr("data-targetpage"));
                    changePage(targetPage)
                });

        },
    };
}());


var largeTilesProvider = (function () {

    var productHtmlContainer; // where we inject product listing results

    function internalFunction() {
    }


    // external functions

    return {
        initialize: function (container) {
            productHtmlContainer = container;
        },

        loadHtml: function (html) {
            // good to go, we have an html fragment
            productsHtmlContainer.html(html);

        },
    };
}());


var cardTilesProvider = (function () {

    var productHtmlContainer; // where we inject product listing results

    function internalFunction() {
    }


    // external functions

    return {
        initialize: function (container) {
            productHtmlContainer = container;
        },

        loadHtml: function (html) {

            // good to go, we have an html fragment
            productsHtmlContainer.html(html);
        },
    };
}());




var productsComponent = (function () {


    var componentContainer; // element for root container
    var productHtmlContainer; // where we inject product listing results

    var pageNumber = 1; // init to 1 on each new entry from filter
    var filterOn = "Manufacturer"; // Manufacturer,Category,Query
    // defaults; mirrors UX

    var pageSize = 40;
    var displayFormat = "SmallTiles"; // SmallTiles,LargeTiles, CardTiles
    var displayFormatProvider = null; // one of the classes
    // current state, only one will be set
    var manufacturerID;
    var categoryID;
    var searchQuery;
 


    function setContainerTitle(title) {

        componentContainer.find("#productsContainerTitle").text(title)
    }

    
    function getRandomArbitrary(min, max) {
        return Math.random() * (max - min) + min;
    }

    function getUrlUniqifier(prefix) {

        var dt = new Date();
        var ms = dt.getTime();

        var str = prefix + "rand=";

        str += ms;
        str += getRandomArbitrary(1000000, 9999999)
        return str.replace(".", "");
    }



    function onDisplayFormatChanged(format)
    {
        displayFormat = format;

        switch (format) {
            case "SmallTiles":
                displayFormatProvider = smallTilesProvider;
                pageSize = 40;
                pageNumber = 1;
                break;

            case "LargeTiles":
                displayFormatProvider = largeTilesProvider;
                pageSize = 40;
                pageNumber = 1;
                break;

            case "CardTiles":
                displayFormatProvider = cardTilesProvider;
                pageSize = 40;
                pageNumber = 1;
                break;
        }
    }


    function fetchProducts()
    {
        var url = "/Products/"

        switch(filterOn)
        {
            case "Manufacturer":
                url += "GetProductsByManufacturer/" + manufacturerID + "/" + displayFormat + "/" + pageSize + "/" + pageNumber + getUrlUniqifier("?");
                break;

            case "Category":
                url += "GetProductsByCategory/" + categoryID + "/" + displayFormat + "/" + pageSize + "/" + pageNumber + getUrlUniqifier("?");
                break;

            case "Query":
                url += "GetProductsByQuery/" + displayFormat + "/" + pageSize + "/" + pageNumber + "?query=" + encodeURIComponent(searchQuery) + getUrlUniqifier("&");
                break;

        }

        $.get(url)
          .done(function (htmlFragment) {
              displayFormatProvider.loadHtml(htmlFragment);
          });

    }

    // external functions

    return {
        initialize: function () {

            componentContainer = $("#productsContainer");
            productsHtmlContainer = $("#productsHtmlContainer");

            smallTilesProvider.initialize(productsHtmlContainer);
            largeTilesProvider.initialize(productsHtmlContainer);
            cardTilesProvider.initialize(productsHtmlContainer);

            onDisplayFormatChanged("SmallTiles");

            console.log("Products component initialized.")
        },

        navigateToPage: function(targetPage)
        {
            // navigate to page with all other settings remaining the same
            // called from the tile providers
            pageNumber = targetPage;
            fetchProducts();
        },

        populateProductsByManufacturer: function (id, name) {
            filterOn = "Manufacturer";
            manufacturerID = id;
            pageNumber = 1;
            setContainerTitle(name);
            fetchProducts();
        },

        populateProductsByCategory: function (id, name) {
            filterOn = "Category";
            categoryID = id;
            pageNumber = 1;
            setContainerTitle(name);
            fetchProducts();
        },

        populateProductsBySearch: function (query) {
            pageNumber = 1;
            filterOn = "Query";
            searchQuery = query;
            if (query === "")
                 setContainerTitle("No Results");
            else
                setContainerTitle("Find: " + query);

            fetchProducts();
        },

    };
}());


