$ ->
  map = null
  gCollection = new YMaps.GeoObjectCollection()
  activePlacemark = null

  ini = ->
      gStyle = new YMaps.Style()
      gStyle.iconStyle = new YMaps.IconStyle(new YMaps.Template('<div class="place agreg">$[iconContent]</div>'))
      gStyle.iconStyle.offset = new YMaps.Point -20, -40
      sStyle = new YMaps.Style()
      sStyle.iconStyle = new YMaps.IconStyle(new YMaps.Template('<div class="place $[className]">$[iconContent]</div>'))
      sStyle.iconStyle.offset = new YMaps.Point -20, -40

      YMaps.Styles.add "user#agreg", gStyle
      YMaps.Styles.add "user#station", sStyle

      YMaps.Placemark.prototype.setCustomIconContent = (content, className) ->
        @iconContent = content
        @className = className
        @update()

  subscribePlacemarkClick = (placemark) ->
    YMaps.Events.observe placemark, placemark.Events.Click, (placemark) ->
      findStation placemark.id, false

  iniPlacemark = (placemark, className) ->
    if placemark.grpCount > 1
      placemark.name = "Участок: #{placemark.id}..."
      placemark.setCustomIconContent "#{placemark.id}+", className
    else
      placemark.name = "Участок: #{placemark.id}"
      placemark.setCustomIconContent placemark.id, className

  showPlacemarks = (placemarks) ->
    for grp in _.toArray _.groupBy(placemarks, (prk) -> prk._point.toString())
      descr = (grp.map (x) -> x.id).toString()
      prk = _.sortBy(grp, (x) -> x.id)[0]
      prk.grpCount = grp.length
      iniPlacemark prk, "station"
      prk.description = descr
      subscribePlacemarkClick prk
      gCollection.add prk
      resetActivePlacemark activePlacemark

  resetActivePlacemark = (newActivePlacemark) ->
    if activePlacemark
      prk = gCollection._objects.filter((x) -> x.id == activePlacemark.id)[0]
      if prk then iniPlacemark prk, "station"
    if newActivePlacemark
        prk = gCollection._objects.filter((x) -> x._point.equals newActivePlacemark._point)[0]
        if prk
          prk.id = newActivePlacemark.id
          iniPlacemark prk, "home"
    activePlacemark = newActivePlacemark

  loadMap = ->
    bounds = map.getBounds()
    zoom = "street"
    if map.getZoom() <= 10
      zoom = "city"
    else if map.getZoom() <= 13
      zoom = "district"
    id = "#{bounds._left};#{bounds._bottom};#{bounds._right};#{bounds._top};#{zoom}"
    OData.read "/Service/PresecService.svc/MapRegions('#{id}')?$expand=coords", (result) ->
      prks = []
      gCollection.removeAll()
      $(result.coords).each ->
        id = @descr if @type == 2
        style = if @type != 2 then "user#agreg" else "user#station"
        placemark = new YMaps.Placemark new YMaps.GeoPoint(@lat, @lon), {draggable: false, hideIcon: false, style : style}
        if @type != 2
            placemark.name = @descr
            placemark.description = "Всего участков: " + @count
            placemark.setCustomIconContent @count
            gCollection.add placemark
        else
            placemark.id = id
            prks.push placemark
      if prks.length then showPlacemarks prks


  createMap = ->
    map = new YMaps.Map $("#map")[0]
    map.setCenter new YMaps.GeoPoint(37.64, 55.76), 10
    map.enableScrollZoom()
    map.addOverlay gCollection
    YMaps.Events.observe map, map.Events.BoundsChange, (object) -> loadMap()

  findStation = (search, setCenter) ->
    if activePlacemark and activePlacemark.id == search then return
    OData.read "/Service/PresecService.svc/Stations('#{search}')?$expand=near,boundary/matches,similar/lines/matches,foundBy/found/matches,foundBy/point", (data) ->
      ko.mapping.fromJS data, {}, viewModel
      geo = viewModel.station().geo
      activePrk = null
      if geo
        pt = new YMaps.GeoPoint geo.lat(), geo.lon()
        activePrk = new YMaps.Placemark pt, {draggable: false, hideIcon: false, style : "user#station"}
        activePrk.id = viewModel.id()
      if setCenter and pt and !pt.equals map.getCenter()
        map.setCenter pt, 15
      resetActivePlacemark activePrk

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
        findStation $("#search_field").val(), true

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
            findStation data.id(), true

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