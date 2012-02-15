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
            @first = ko.computed
                read: -> @results()[0]
                deferEvaluation: true
                owner: @
            @similars = ko.computed
                read: -> 
                  if @results().length > 1
                    res = []
                    for x in @results().splice 0, 1
                      line = new LineModel x.id(), x.lines().filter (y) => new RegExp(".*#{@search()}.*", "i").test y.addr()
                      res.concat line
                    res
                deferEvaluation: true
                owner: @          
    viewModel = new  ViewModel()

    ko.applyBindings viewModel
