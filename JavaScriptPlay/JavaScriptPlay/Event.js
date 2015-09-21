var JavaScriptPlay = JavaScriptPlay || {};

var Item = function (name, id) {
    this.name = name;
    this.id = id;
    this.onExecutionCompleteHandlers = [];

};

Item.prototype.addExecutionCompleteHandler = function (handler) {
    this.onExecutionCompleteHandlers.push(handler);
};

Item.prototype.Execution = function () {
    for (i = 0; i < this.onExecutionCompleteHandlers.length; i++) {
        this.onExecutionCompleteHandlers[i]();
    }
};

(function main() {
    var myItem = new Item("box", 1);
    myItem.addExecutionCompleteHandler(function () { alert(myItem.id) });

    myItem.Execution();
})();