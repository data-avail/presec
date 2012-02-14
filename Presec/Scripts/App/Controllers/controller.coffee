define ->
  class Controller
    constructor: (@modelName, @indexViewName, @editViewName, @detailsUrl) ->
    index: ->
      if @modelName and @indexViewName
        require @modelName, (m) =>
          @_clearViews()
          ko.applyBindings m
          @_setView @indexViewName
    edit: (id)->
    details: (id)->
    view:(name) ->
      require name, (v) =>
        @_clearViews()
        @_setView(v)

    _clearViews:() ->
    _setView:(viewName) ->