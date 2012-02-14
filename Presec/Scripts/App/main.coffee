$ ->
  require "router", (router) ->
    router.startRouting (action) ->
      $(".nav li.active").toggleClass "active"
      $(".nav li").has("a[href='#{action}']").toggleClass "active"
