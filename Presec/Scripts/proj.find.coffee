$ ->
  map = null
  gCollection = new YMaps.GeoObjectCollection()
  fCollection = new YMaps.GeoObjectCollection()

  loadMap = ->
    bounds = map.getBounds()
    zoom = "street"
    if map.getZoom() <= 10
      zoom = "city"
    else if map.getZoom() <= 13
      zoom = "district"
    id = "#{bounds._left};#{bounds._bottom};#{bounds._right};#{bounds._top};#{zoom}"
    OData.read "/Service/PresecService.svc/MapRegions('#{id}')?$expand=coords", (result) ->
        gCollection.removeAll()
        $(result.coords).each ->
          placemark = new YMaps.Placemark new YMaps.GeoPoint(@lat, @lon), {draggable: false, style : "default#storehouseIcon"}
          txt = @descr
          if @count > 1 then txt = "#{txt} (#{@count})"
          placemark.id = placemark.name = placemark.description = txt
          if fCollection.filter((x) -> x._point.equals placemark._point).length == 0
            gCollection.add placemark

  createMap = ->
    map = new YMaps.Map $("#map")[0]
    map.setCenter new YMaps.GeoPoint(37.64, 55.76), 10
    map.enableScrollZoom()
    map.addOverlay gCollection
    map.addOverlay fCollection
    YMaps.Events.observe map, map.Events.BoundsChange, (object) -> loadMap()

  createSelector = ->
      $("#search_field").autocomplete
        minLength : 3,
        autoFocus : true,
        source: (req, res) ->
          OData.read "/Service/PresecService.svc/GeoSuggestions('россия, москва, #{req.term}')?$expand=suggestions", (data) ->
              res data.suggestions.map( (x)-> label : x.descr, value : x.term, gref : x.refer )

      $("#search_field").keypress (e) ->
        if(e.keyCode == 13)
          e.preventDefault()
          $("#search_button").click()

    $(".toggle_layout").hide()
    #events
    $("#search_button").click ->
        fCollection.removeAll()
        search = $("#search_field").val()
        OData.read "/Service/PresecService.svc/Stations('#{search}')?$expand=near,boundary/matches,similar/lines/matches", (data) ->
          ko.mapping.fromJS data, {}, viewModel
          geo = viewModel.station().geo
          if geo then map.setCenter new YMaps.GeoPoint(geo.lat(), geo.lon()), 15
          placemark = new YMaps.Placemark map.getCenter(), {draggable: false, style : "default#attentionIcon"}
          placemark.id = placemark.name = placemark.description = viewModel.id()
          fCollection.add placemark

    class LineModel
      constructor:(@id, @lines) ->

    class ViewModel
        constructor: ->
            @id = ko.observable()
            @key = ko.observable()
            @station = ko.observable()
            @uik = ko.observable()
            @similar = ko.observableArray()
            @near = ko.observableArray()
            @boundary = ko.observableArray()
            ###
            @results = ko.observableArray()
            @first = ko.computed => @results()[0]
            @similars = ko.computed =>
              for r in @results().filter((x) => x != @results()[0])
                new LineModel r.id(), r.lines().filter((x) => new RegExp(".*" + (@search()) + ".*", "i").test(x.addr())).map((x)=>x.addr())
            @near = ko.computed =>
              if @first()
                for n in @first().near()
                    new LineModel n.id(), n.lines().map (x) => x.addr()
              else
                []
            ###
            @similarToggle = ko.observable false
            @nearToggle = ko.observable false

    ko.bindingHandlers.toggle =
        init: (element, valueAccessor, allBindingsAccessor, viewModel) ->
            opts = allBindingsAccessor().toggleOpts
            $(element).click ->
                $(".#{opts.layout}").toggle()
                $("i", element).toggleClass "icon-folder-open"
                $("i", element).toggleClass "icon-folder-close"

    viewModel = new  ViewModel()

    ko.applyBindings viewModel

    createMap()
    createSelector()
    loadMap()