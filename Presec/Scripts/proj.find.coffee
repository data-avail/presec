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
                read: -> @results().filter (x) => x != @results()[0]
                owner: @

    viewModel = new  ViewModel()

    ko.applyBindings viewModel
