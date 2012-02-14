(function() {

  $(function() {
    return require("router", function(router) {
      return router.startRouting(function(action) {
        $(".nav li.active").toggleClass("active");
        return $(".nav li").has("a[href='" + action + "']").toggleClass("active");
      });
    });
  });

}).call(this);
