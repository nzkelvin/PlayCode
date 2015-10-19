/// <reference path="XrmPageTemplate.js" />
/// <reference path="Kelvin.Common.js" />

var placeSearch, autocomplete;
var componentForm = {
    street_number: 'short_name',
    route: 'long_name',
    locality: 'long_name',
    administrative_area_level_1: 'short_name',
    country: 'long_name',
    postal_code: 'short_name'
};
// todo: comment about the map
// todo: video
var crmDataMap = {
    street_number: 'address1_line1',
    route: 'address1_line2',
    sublocality_level_1: 'address1_line3',
    locality: 'address1_city',
    administrative_area_level_1: 'address1_stateorprovince',
    country: 'address1_country',
    postal_code: 'address1_postalcode'
};
//var googleKey = "";

function initAutocomplete() {
    //var googleKey = "";
    var paramData = Kelvin.Common.Crm.parseWebResourceCustomParameterToJson();
    if (paramData != null && paramData.map != null)
        crmDataMap = paramData.map;

    //if (paramData != null && paramData.googlekey != null)
    //    googleKey = paramData.googlekey;
    
    initAddressFields();
    //importGoogleJs(googleKey);

    // Create the autocomplete object, restricting the search to geographical
    // location types.
    autocomplete = new google.maps.places.Autocomplete(
        /** @type {!HTMLInputElement} */(document.getElementById('autocomplete')),
        { types: ['geocode'] });

    // When the user selects an address from the dropdown, populate the address
    // fields in the form.
    autocomplete.addListener('place_changed', fillInAddress);
}

function initAddressFields() {
    var keys = Object.keys(crmDataMap);
    var crmPage = window.parent.Xrm.Page;
    if (crmPage == null)
        return;

    for (var i = 0; i < keys.length; i++) {
        var key = keys[i];
        var element = document.getElementById(key);
        
        if (element != null) {
            var crmFieldName = crmDataMap[key];
            var crmAttr = crmPage.getAttribute(crmFieldName);
            
            if (crmAttr != null) {
                element.value = crmAttr.getValue(); // load data from crm fields

                // change crm fields' value when values change.
                (function () {
                    var attr = crmAttr;
                    var elem = element;
                    element.addEventListener("change", function () {
                        attr.setValue(elem.value);
                    })
                }());
            }
        }
    }
}

// [START region_fillform]
function fillInAddress() {
    // Get the place details from the autocomplete object.
    var place = autocomplete.getPlace();

    for (var component in componentForm) {
        document.getElementById(component).value = '';
        document.getElementById(component).disabled = false;
    }

    // CRM Page
    var crmPage = window.parent.Xrm.Page;
    if (crmPage == null)
        return;

    // Get each component of the address from the place details
    // and fill the corresponding field on the form.
    for (var i = 0; i < place.address_components.length; i++) {
        var addressType = place.address_components[i].types[0];
        if (componentForm[addressType]) {
            var val = place.address_components[i][componentForm[addressType]];
            document.getElementById(addressType).value = val;

            // CRM integration
            var crmField = crmPage.getAttribute(crmDataMap[addressType]);
            if (crmField != null)
                crmField.setValue(val);
        }
    }

    document.getElementById("autocomplete").value = "";
}
// [END region_fillform]

// [START region_geolocation]
// Bias the autocomplete object to the user's geographical location,
// as supplied by the browser's 'navigator.geolocation' object.
function geolocate() {
    if (navigator.geolocation) {
        navigator.geolocation.getCurrentPosition(function (position) {
            var geolocation = {
                lat: position.coords.latitude,
                lng: position.coords.longitude
            };
            var circle = new google.maps.Circle({
                center: geolocation,
                radius: position.coords.accuracy
            });
            autocomplete.setBounds(circle.getBounds());
        });
    }
}
// [END region_geolocation]

(function importGoogleJs(googleKey) {
    var googleKey = "";
    var paramData = Kelvin.Common.Crm.parseWebResourceCustomParameterToJson();
    if (paramData != null && paramData.googlekey != null)
        googleKey = paramData.googlekey;

    var imported = document.createElement('script');
    var googleLibSrc = "https://maps.googleapis.com/maps/api/js?libraries=places&callback=initAutocomplete";
    if (googleKey && googleKey.trim())
        googleLibSrc = googleLibSrc + "&key=" + googleKey;
    imported.src = googleLibSrc;
    imported.defer = "defer";
    imported.async = "async";
    document.body.appendChild(imported);
}());
