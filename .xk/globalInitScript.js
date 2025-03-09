var json = host.lib('Newtonsoft.Json');
var jsonLinq = json.Newtonsoft.Json.Linq;
var jsonConvert = json.JsonConvert;
var jObject = jsonLinq.JObject;

var xferLang = host.lib('ParksComputing.Xfer.Lang');

var startWorkspace = xk.store.get("startWorkspace");

if (startWorkspace != null) {
    xk.setActiveWorkspace(startWorkspace);
}
