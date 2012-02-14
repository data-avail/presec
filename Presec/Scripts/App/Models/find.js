(function() {

  define(["addr", "mapView"], function(Addr, MapView) {
    var Find;
    return Find = (function() {

      function Find() {
        this.addr = new Addr.Addr();
        this.station = new Addr.Addr();
        this.uik = new Addr.Addr();
        this.near = ko.observableArray();
        this.mapView = new MapView.MapView();
      }

      return Find;

    })();
  });

}).call(this);
