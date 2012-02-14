define ->
  class MapView
    constructor: ->
      @long = ko.observable()
      @lat = ko.observable()
      @zoom = ko.observable()
      @points = ko.observableArray()
  MapView : MapView