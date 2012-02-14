(function() {

  define(function() {
    var currentHash, defaultRoute, hashCheck, onRouteChanged, refresh, startRouting;
    defaultRoute = "#/Stack/Index";
    currentHash = "";
    onRouteChanged = null;
    startRouting = function(OnRouteChanged) {
      onRouteChanged = OnRouteChanged;
      window.location.hash = window.location.hash || defaultRoute;
      return setInterval(hashCheck, 100);
    };
    hashCheck = function() {
      if (window.location.hash !== currentHash) {
        currentHash = window.location.hash;
        refresh();
        if (onRouteChanged) return onRouteChanged(currentHash);
      }
    };
    refresh = function() {
      var actionName, controllerName, index, match, regexp;
      regexp = new RegExp("#/(\\w+)/(\\w+)/?(\\w+)?");
      match = currentHash.match(regexp);
      controllerName = match[1];
      actionName = match[2];
      index = match[3];
      return require(["Controllers/" + controllerName + "Controller"], function(controller) {
        return eval("controller." + actionName + "(" + index + ")");
      });
    };
    return {
      startRouting: startRouting
    };
  });

}).call(this);
