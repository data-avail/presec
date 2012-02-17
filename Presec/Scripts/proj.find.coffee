$ ->
    map = null
    createMap = ->
      map = new YMaps.Map $("#map")[0]
      map.setCenter new YMaps.GeoPoint(37.64, 55.76), 10

    createSelector = ->
      search = $("#search_field")
      $("#search_field").autocomplete
        minLength : 2,
        source: (req, res) ->
          geocoder = new YMaps.Geocoder "Россия, Московская область, город Москва, улица " + req.term
          YMaps.Events.observe geocoder, geocoder.Events.Load, ->
            res @_objects.map( (x) -> label : x.text, value : x.text, key : x._point )
          ###
          $.ajax
            url : "http://geocode-maps.yandex.ru/1.x/"
            data :
              geocode : "Россия, город Москва, улица #{req.term}"
              search_type : "all"
              lang : "ru-RU"
              key : "AOpIPk8BAAAAVW-PBgIAY2B20rPw1PcOX0Gkn6Ah7e15L9QAAAAAAAAAAACXy_5ZdWMyS9NOY137nkvh99lqew=="
              format : "json"
              ll : "37.617671000000016,55.75576799999372"
              spn: "1.51062"
            success: (data) ->
              res data.response.GeoObjectCollection.featureMember
                #.filter((x) -> x.GeoObject.metaDataProperty.GeocoderMetaData.kind == "street" || x.GeoObject.metaDataProperty.GeocoderMetaData.kind == "district")
                .map( (x)-> label : x.GeoObject.name, value : x.GeoObject.name, key : x.GeoObject.Point )
          ###


    $(".toggle_layout").hide()
    #events
    $("#search_button").click ->
        search = $("#search_field").val()
        OData.read "/Service.svc/Stations?addr=#{search}&$expand=lines,near/lines", (data) ->
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