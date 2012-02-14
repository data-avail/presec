(function() {
  var __hasProp = Object.prototype.hasOwnProperty,
    __extends = function(child, parent) { for (var key in parent) { if (__hasProp.call(parent, key)) child[key] = parent[key]; } function ctor() { this.constructor = child; } ctor.prototype = parent.prototype; child.prototype = new ctor; child.__super__ = parent.prototype; return child; };

  define("controller", function(controller) {
    var FindController;
    return FindController = (function(_super) {

      __extends(FindController, _super);

      function FindController() {
        FindController.__super__.constructor.call(this, "find", "Find/index");
      }

      return FindController;

    })(controller.Controller);
  });

}).call(this);
