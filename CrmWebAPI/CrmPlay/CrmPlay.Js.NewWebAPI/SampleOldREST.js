/// <reference path="SDK.REST.js" />

document.onreadystatechange = function () {
    if (document.readyState == "complete") {
        startSample();
    }
}

//Simple error handler used by all samples just writes message to console
function sampleErrorHandler(error) {
    console.log(error.message);
}

function startSample() {
    addContact();
}

function addContact() {
    console.log("addContact function starting.");
    var contactAUri;

    var contact = {
        firstname: "Tom",
        lastname: "Test from Old REST API"
    };

    SDK.REST.createRecord(
        contact,
        "Contact",
        function (contact) {
            console.log("An contact is created. Id: " + contact.ContactId);
        }
    );
}