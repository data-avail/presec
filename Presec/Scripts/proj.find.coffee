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
            if @type == 2
                YMaps.Events.observe placemark, placemark.Events.Click, (prk) ->
                  findStation prk.id

  createMap = ->
    map = new YMaps.Map $("#map")[0]
    map.setCenter new YMaps.GeoPoint(37.64, 55.76), 10
    map.enableScrollZoom()
    map.addOverlay gCollection
    map.addOverlay fCollection
    YMaps.Events.observe map, map.Events.BoundsChange, (object) -> loadMap()

  findStation = (search) ->
    OData.read "/Service/PresecService.svc/Stations('#{search}')?$expand=near,boundary/matches,similar/lines/matches,foundBy/found/matches,foundBy/point", (data) ->
      fCollection.removeAll()
      ko.mapping.fromJS data, {}, viewModel
      geo = viewModel.station().geo
      if geo then map.setCenter new YMaps.GeoPoint(geo.lat(), geo.lon()), 15
      placemark = new YMaps.Placemark map.getCenter(), {draggable: false, style : "default#attentionIcon"}
      placemark.id = placemark.name = placemark.description = viewModel.id()
      fCollection.add placemark
      YMaps.Events.observe placemark, placemark.Events.Click, (prk) ->
        findStation prk.id
     
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


    $("#search_button").click ->
        findStation $("#search_field").val()

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
            @matchType = ko.observable()
            @foundBy = ko.observable()
            @similarToggle = ko.observable false
            @nearToggle = ko.observable false

        showStation: (data) ->
            findStation data.id()

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