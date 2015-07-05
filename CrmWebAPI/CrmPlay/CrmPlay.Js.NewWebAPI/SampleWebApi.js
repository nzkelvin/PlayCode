/// <reference path="Sdk.WebAPIPreview.js" />

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

    var contactA = {
        firstname: "Tom",
        lastname: "Test"
    };

    Sdk.WebAPIPreview.create("contacts", contactA, function (url) {
        console.log("a contact was created. url: " + url);
    }, sampleErrorHandler);
}