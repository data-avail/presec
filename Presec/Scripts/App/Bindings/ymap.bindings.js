(function() {

  ko.bindingHandlers.ymap = {
    init: function(element, valueAccessor, allBindingsAccessor) {
      var map, mapView, pos, zoom;
      mapView = ko.utils.unwrapObservable(valueAccessor);
      map = new YMaps.Map($(element));
      map.addControl(new YMaps.TypeControl());
      map.addControl(new YMaps.Zoom());
      map.addControl(new YMaps.ScaleLine());
      map.enableScrollZoom();
      pos = YMaps.location;
      zoom = 10;
      map.setCenter(new YMaps.GeoPoint(pos.longitude, pos.latitude), zoom);
      YMaps.Events.observe(map, map.Events.BoundsChange, function(object) {
        return this._refresh(mapView);
      });
      return this._refresh(mapView);
    },
    _refresh: function(mapView) {}
  };

}).call(this);
