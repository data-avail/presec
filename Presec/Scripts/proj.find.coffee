$ ->
    #events
    $("#search_button").click ->
        search = $("#search_field").val()
        OData.read "/Service.svc/Stations?addr=#{search}&$expand=lines", (data) ->
          ko.mapping.fromJS data, {}, viewModel
          viewModel.search search
          #ko.applyBindings viewModel
    #ko

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

    viewModel = new  ViewModel()

    ko.applyBindings viewModel
