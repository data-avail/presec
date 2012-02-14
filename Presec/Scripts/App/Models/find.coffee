define ["addr", "mapView"], (Addr, MapView) ->
  class Find
    constructor: ->
      @addr = new Addr.Addr()
      @station = new Addr.Addr()
      @uik = new Addr.Addr()
      @near = ko.observableArray()
      @boundary = ko.observableArray()
      @mapView = new MapView.MapView()

