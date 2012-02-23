$ ->
  map = null
  gCollection = new YMaps.GeoObjectCollection()
  fCollection = new YMaps.GeoObjectCollection()

  ini = ->
      gStyle = new YMaps.Style()
      gStyle.iconStyle = new YMaps.IconStyle(new YMaps.Template('<div class="place agreg">$[iconContent]</div>'))
      gStyle.iconStyle.offset = new YMaps.Point -20, -40
      sStyle = new YMaps.Style()
      sStyle.iconStyle = new YMaps.IconStyle(new YMaps.Template('<div class="place station">$[iconContent]</div>'))
      sStyle.iconStyle.offset = new YMaps.Point -20, -40
      hStyle = new YMaps.Style()
      hStyle.iconStyle = new YMaps.IconStyle(new YMaps.Template('<div class="place home">$[iconContent]</div>'))
      hStyle.iconStyle.offset = new YMaps.Point -20, -40

      YMaps.Styles.add "user#agreg", gStyle
      YMaps.Styles.add "user#station", sStyle
      YMaps.Styles.add "user#home", hStyle
    
      YMaps.Placemark.prototype.setCustomIconContent = (content) ->
        @iconContent = content
        @update()

  iniStationPlacemark = (placemark, id) ->
    placemark.id = id
    placemark.name = "Участок: " + id
    placemark.setCustomIconContent id
    YMaps.Events.observe placemark, placemark.Events.Click, (prk) ->
      findStation prk.id

  loadMap = ->
    bounds = map.getBounds()
    zoom = "street"
    if map.getZoom() <= 10
      zoom = "city"
    else if map.getZoom() <= 13
      zoom = "district"
    id = "#{bounds._left};#{bounds._bottom};#{bounds._right};#{bounds._top};#{zoom}"
    gCollection.removeAll()
    OData.read "/Service/PresecService.svc/MapRegions('#{id}')?$expand=coords", (result) ->
        $(result.coords).each ->
          id = @descr if @type == 2
          if fCollection.filter((x) -> x.id.toString() == id).length == 0
            style = if @type != 2 then "user#agreg" else "user#station"
            placemark = new YMaps.Placemark new YMaps.GeoPoint(@lat, @lon), {draggable: false, hideIcon: false, style : style}
            if @type != 2
                placemark.name = @descr
                placemark.description = "Всего участков: " + @count
                placemark.setCustomIconContent @count
            else
                iniStationPlacemark placemark, id
            gCollection.add placemark

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
      placemark = new YMaps.Placemark map.getCenter(), {draggable: false, style : "user#home"}
      iniStationPlacemark placemark, viewModel.id()
      fCollection.add placemark
     
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

    ini()
    createMap()
    createSelector()
    loadMap()