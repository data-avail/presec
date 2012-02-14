(function() {

  define(function() {
    var Controller;
    return Controller = (function() {

      function Controller(modelName, indexViewName, editViewName, detailsUrl) {
        this.modelName = modelName;
        this.indexViewName = indexViewName;
        this.editViewName = editViewName;
        this.detailsUrl = detailsUrl;
      }

      Controller.prototype.index = function() {
        var _this = this;
        if (this.modelName && this.indexViewName) {
          return require(this.modelName, function(m) {
            _this._clearViews();
            ko.applyBindings(m);
            return _this._setView(_this.indexViewName);
          });
        }
      };

      Controller.prototype.edit = function(id) {};

      Controller.prototype.details = function(id) {};

      Controller.prototype.view = function(name) {
        var _this = this;
        return require(name, function(v) {
          _this._clearViews();
          return _this._setView(v);
        });
      };

      Controller.prototype._clearViews = function() {};

      Controller.prototype._setView = function(viewName) {};

      return Controller;

    })();
  });

}).call(this);
