$ ->
    map = null
    createMap = ->
      map = new YMaps.Map $("#map")[0]
      map.setCenter new YMaps.GeoPoint(37.64, 55.76), 10

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