console.log('Hello world');

var x = 10;
var y = 25;
console.log(x * y);

var msgs = require("./msgs.js");
var msg = new msgs();
console.log(msg.message1);
console.log(msg.firstObj);

var underscore = require("underscore");
console.log(underscore.contains([1, 2, 3] , 3));