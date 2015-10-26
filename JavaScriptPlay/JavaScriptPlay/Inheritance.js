"use strict";

var k$ = k$ || {
    Animal: function () {

    }
};

(function Main() {
    var animal = new k$.Animal();
    console.log(animal instanceof k$.Animal);
}());