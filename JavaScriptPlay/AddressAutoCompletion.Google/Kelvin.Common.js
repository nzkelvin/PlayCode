var Kelvin = Kelvin || {}

Kelvin.Common = Kelvin.Common || {
    parseQueryStringToJson: function () {
        var search = window.location.search.substring(1);
        return search
            ? JSON.parse('{"' + search.replace(/&/g, '","').replace(/=/g, '":"') + '"}', function (key, value) { return key === "" ? value : decodeURIComponent(value) })
            : {};
    }
};

Kelvin.Common.Crm = Kelvin.Common.Crm || {
    parseWebResourceCustomParameterToJson: function () {
        var qsJson = Kelvin.Common.parseQueryStringToJson();

        if (qsJson != null && qsJson.hasOwnProperty("data"))
        {
            return JSON.parse(qsJson["data"]);
        }
    }
}