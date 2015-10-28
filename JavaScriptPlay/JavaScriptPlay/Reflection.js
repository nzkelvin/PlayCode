var k$ = k$ || {
    Customer: function (n, e) {
        this.name = n;
        this.email = e;
    }
};

(function Main() {
    var customer = new k$.Customer("Jeffery", "jeffery.white@test.com");

    // get perperty values of an object
    for (var prop in customer) {
        console.log(customer[prop]);
    }

    // get property names of an object
    for (var prop in customer) {
        console.log(prop);
    }

    // get property names of an object
    var keys = Object.keys(customer);
    for (var i = 0; i < keys.length; i++) {
        console.log(keys[i]);
    }
})();