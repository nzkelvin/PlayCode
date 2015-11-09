"use strict"

// Of course it will work with inheritance.
var k$ = k$ || {
    Fruit: function (name) {
        var _name = name;

        Object.defineProperty(this, "name", {
            get: function () { return _name; }
        })
    }
};

(function Main() {
    var fruit = new k$.Fruit("apple");
    console.log(fruit.name);
})();