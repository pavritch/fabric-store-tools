
$(function () {

    filterComponent.initialize();
});


var filterComponent = (function () {

    var componentContainer; // element for root container
    var activeTabName; // Manufacturers,Categories,Search

    // all selected vars are filled in on init to reflect the
    // current active state of their respective tabs

    var selectedManufacturerID;
    var selectedManufacturerName;

    var selectedCategoryID;
    var selectedCategoryName;

    var selectedQuery;


    function populateProductsByManufacturer()
    {
        // uses the current state, call out to other component
        //console.log("populate manufacturer: " + selectedManufacturerName)
        productsComponent.populateProductsByManufacturer(selectedManufacturerID, selectedManufacturerName);
    }

    function populateProductsByCategory()
    {
        // uses the current state, call out to other component
        //console.log("populate category: " + selectedCategoryName)
        productsComponent.populateProductsByCategory(selectedCategoryID, selectedCategoryName);
    }

    function populateProductsBySearch()
    {
        // uses the current state, call out to other component
        //console.log("populate search: " + selectedQuery)
        productsComponent.populateProductsBySearch(selectedQuery);

    }

    function selectManufacturer(id, name)
    {
        selectedManufacturerID = id;
        selectedManufacturerName = name;
        populateProductsByManufacturer();
    }

    function selectCategory(id, name)
    {
        selectedCategoryID = id;
        selectedCategoryName = name;
        populateProductsByCategory();
    }

    function search(query)
    {
        selectedQuery = query;
        populateProductsBySearch();
    }

    function activeTabChanged(el)
    {
        // a tag that was clicked; but also called on init for Manufacaturers
        activeTabName = el.text();
        console.log("tab changed: " + activeTabName);

        // having just switched to this tab, bring in products for whichever
        // element is active. (might already be cached)

        switch (activeTabName) {
            case "Manufacturers":
                populateProductsByManufacturer();
                break;

            case "Categories":
                populateProductsByCategory();
                break;

            case "Search":
                populateProductsBySearch();
                break;
        }
    }

    function hookManufacturers() {
        componentContainer.find("#tabManufacturers a[data-manufacturerid]").click(function (event) {
            event.preventDefault();

            var id = parseInt($(this).attr("data-manufacturerid"))
            var name = $(this).text();

            componentContainer.find("#tabManufacturers a.active").removeClass('active');
            $(this).addClass('active');

            selectManufacturer(id, name);
        });

        var el = componentContainer.find("#tabManufacturers a.active");

        selectedManufacturerID = el.attr("data-manufacturerid");
        selectedManufacturerName = el.text();
    }

    function hookCategories() {
        componentContainer.find("#tabCategories a[data-categoryid]").click(function (event) {
            event.preventDefault();

            var id = parseInt($(this).attr("data-categoryid"))
            var name = $(this).text();

            componentContainer.find("#tabCategories a.active").removeClass('active');
            $(this).addClass('active');

            selectCategory(id, name);
        });

        var el = componentContainer.find("#tabCategories a.active");
        selectedCategoryID = el.attr("data-categoryid");
        selectedCategoryName = el.text();
    }

    function hookSearch() {
        componentContainer.find("#tabSearch #btnSearchProducts").click(function (event) {
            event.preventDefault();

            selectedQuery = "";
            var query = componentContainer.find("#tabSearch #searchQuery").val();
            if (query != "")
                search(query);
        });

        componentContainer.find("#tabSearch #btnSearchUnclassified").click(function (event) {
            event.preventDefault();

            selectedQuery = "";
            search("Unclassified");
        });

        componentContainer.find("#tabSearch #btnSearchUnpublished").click(function (event) {
            event.preventDefault();

            selectedQuery = "";
            search("Unpublished");
        });

        selectedQuery = "";
    }


    // external functions

    return {
        initialize: function () {

            componentContainer = $("#filterContainer");

            // initial active tab is Manufacturers, set by MVC page
            componentContainer.find('a[data-toggle="tab"]').on('shown.bs.tab', function (e) {
                activeTabChanged($(e.target));
            })

            hookManufacturers();
            hookCategories();
            hookSearch();

            // both manufacturers and categories will have their first node active coming
            // in from mvc

            // make sure other panels have initialized, then fire off notification which will trigger the
            // initial population of products
            setTimeout(function () { activeTabChanged(componentContainer.find("#tabLinkManufacturers")) }, 200);
        },

    };
}());


