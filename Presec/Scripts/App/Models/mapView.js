(function() {

  define(function() {
    var MapView;
    MapView = (function() {

      function MapView() {
        this.long = ko.observable();
        this.lat = ko.observable();
        this.zoom = ko.observable();
        this.points = ko.observableArray();
      }

      return MapView;

    })();
    return {
      MapView: MapView
    };
  });

}).call(this);
