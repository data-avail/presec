$ ->
  map = null
  gCollection = new YMaps.GeoObjectCollection()

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
          gCollection.add placemark

  createMap = ->
    map = new YMaps.Map $("#map")[0]
    map.setCenter new YMaps.GeoPoint(37.64, 55.76), 10
    map.enableScrollZoom()
    map.addOverlay gCollection
    YMaps.Events.observe map, map.Events.BoundsChange, (object) -> loadMap()

  createSelector = ->
      $("#search_field").autocomplete
        minLength : 3,
        autoFocus : true,
        source: (req, res) ->
          OData.read "/Service/PresecService.svc/GeoSuggestions?term=россия, москва, #{req.term}", (data) ->
              res data.results.map( (x)-> label : x.descr, value : x.term, gref : x.refer )
        select: (e, ui) ->
             $(@).data "gref", ui.item.gref

      $("#search_field").keypress (e) ->
        if(e.keyCode == 13)
          e.preventDefault()
          $("#search_button").click()

    $(".toggle_layout").hide()
    #events
    $("#search_button").click ->
        search = $("#search_field").val()
        gref = $("#search_field").data "gref"
        OData.read "/Service/PresecService.svc/Stations?addr=#{search}&gref=#{gref}&$expand=lines,near/lines", (data) ->
          ko.mapping.fromJS data, {}, viewModel
          viewModel.search search
          if viewModel.first()
            geo = viewModel.first().station.geo
            if geo then map.setCenter new YMaps.GeoPoint(geo.lat(), geo.lon()), 15
            placemark = new YMaps.Placemark map.getCenter(), {draggable: false, style : "default#storehouseIcon"}
            map.addOverlay placemark

    class LineModel
      constructor:(@id, @lines) ->

    class ViewModel
        constructor: ->
            @search = ko.observable()
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