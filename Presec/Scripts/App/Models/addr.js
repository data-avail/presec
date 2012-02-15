(function() {
  define(function() {
    var Addr;
    Addr = (function() {
      function Addr() {
        this.line = ko.obsrvable();
        this.lineType = ko.observable();
        this.buliding = ko.observable();
        this.room = ko.observable();
        this.aux = ko.observable();
        this.phones = ko.obbservableArray();
      }
      return Addr;
    })();
    return {
      Addr: Addr
    };
  });
}).call(this);
