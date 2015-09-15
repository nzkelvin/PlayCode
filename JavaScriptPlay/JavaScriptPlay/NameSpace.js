//if (typeof (KelvinS) == typeof (undefined))
//    KelvinS = {};

//if (typeof (KelvinS.Crm) == typeof (undefined))
//    KelvinS.Crm = {
//        getGuid: function() { alert("Hello Guid") }
//    };


var KelvinS = KelvinS || {};
KelvinS.Crm = KelvinS.Crm || {
    getGuid: function () { alert("Hello Guid") }
};

// Run code here
(function Main() {
    KelvinS.Crm.getGuid();
})();