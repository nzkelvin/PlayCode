//Constructor
function Pager() {
    this.CurrentPage = 0;
}

Pager.maxRecordsPerPage = 4;

//This is for object
Pager.prototype.nextPage = function () {
    this.CurrentPage++;
    console.log("from prototype");
};

////This is static
//Pager.nextPage = function () {
//    console.log("from static object");
//};

// This function will execute first.
(function main() {
    var mypager = new Pager();
    mypager.nextPage();
    console.log(mypager.CurrentPage);
    console.log(Pager.maxRecordsPerPage);
})();