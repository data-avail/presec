$ ->
    #events
    $("#search_button").click ->
        OData.read "/Service.svc/Stations?addr=#{$("#search_field").val()}", (data) ->
            viewModel = new ViewModel()
            ko.mapping.fromJS data, {}, viewModel
            ko.applyBindings viewModel
    #ko
    class ViewModel
        constructor: ->
            @results = ko.observableArray()
            @first = ko.computed
                read: -> @results()[0]
                deferEvaluation: true
                owner: @

    #ko.applyBindings new ViewModel()
