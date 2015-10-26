//Use instanceof for custom types:
var ClassFirst = function () {};
var ClassSecond = function () {};
var instance = new ClassFirst();
typeof instance; // object
typeof instance == 'ClassFirst'; //false
instance instanceof Object; //true
instance instanceof ClassFirst; //true
instance instanceof ClassSecond; //false 

//Use typeof for simple built in types:
'example string' instanceof String; // false
typeof 'example string' == 'string'; //true

'example string' instanceof Object; //false
typeof 'example string' == 'object'; //false

true instanceof Boolean; // false
typeof true == 'boolean'; //true

99.99 instanceof Number; // false
typeof 99.99 == 'number'; //true

(function foo() {}) instanceof Function; //true
typeof function() {} == 'function'; //true

//Use instanceof for complex built in types:
/regularexpression/ instanceof RegExp; // true
typeof /regularexpression/; //object

[] instanceof Array; // true
typeof []; //object

({}) instanceof Object; // true
typeof {}; //object

//And the last one is a little bit tricky:
typeof null; //object

////////////////////////////////////
var k$ = k$ || {
    UseTypeofForSimpleBuildInTypes: function () {
        console.log("\r\n======Use typeof for simple built in types=======");

        console.log('example string' instanceof String);
        console.log(typeof 'example string');
        console.log(typeof 'example string' == 'string');

        console.log('example string' instanceof Object);
        console.log(typeof 'example string' == 'object');

        console.log(true instanceof Boolean);
        console.log(typeof true == 'boolean');

        console.log(99.99 instanceof Number);
        console.log(typeof 99.99 == 'number');
    },
    UseInstanceofForComplexBuildInTypes: function () {
        console.log("\r\n======Use instanceof for complex built in types=======");

        console.log(/regularexpress/ instanceof RegExp);
        console.log(typeof /regularexpress/);

        console.log([] instanceof Array);
        console.log(typeof []);

        console.log({} instanceof Object);
        console.log(typeof {})
    },
    UseInstanceofForCustomTypes: function () {
        console.log("\r\n======Use instanceof for custom types======");

        function ClassFirst() { }
        function ClassSecond() { }
        var instance = new ClassFirst();

        console.log(typeof instance);
        console.log(typeof instance == 'ClassFirst');
        console.log(instance instanceof Object);
        console.log(instance instanceof ClassFirst);
        console.log(instance instanceof ClassSecond);
    },
    TestNull: function () {
        console.log("\r\n======Test Null=======");

        console.log(null instanceof Object); // false
        console.log(typeof null); // 'object'
    },
    TestUndefined: function () {
        console.log("\r\n======Test Undefined=======");

        var undefinedObj;
        console.log(undefinedObj instanceof Object); // false
        console.log(typeof undefinedObj); // 'undefined'

    }
};

(function Main() {
    k$.UseTypeofForSimpleBuildInTypes();
    k$.UseInstanceofForComplexBuildInTypes();
    k$.UseInstanceofForCustomTypes();
    k$.TestNull();
    k$.TestUndefined();
})();