ko.bindingHandlers.ymap =
  init:(element, valueAccessor, allBindingsAccessor) ->
    mapView = ko.utils.unwrapObservable valueAccessor
    map = new YMaps.Map $(element)
    map.addControl new YMaps.TypeControl()
    map.addControl new YMaps.Zoom()
    map.addControl new YMaps.ScaleLine()
    map.enableScrollZoom()
    pos = YMaps.location
    zoom = 10
    map.setCenter new YMaps.GeoPoint(pos.longitude, pos.latitude), zoom
    YMaps.Events.observe map, map.Events.BoundsChange, (object) -> @_refresh mapView
    @_refresh mapView
  _refresh: (mapView) -