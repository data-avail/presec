define ->
  class Addr
    constructor: () ->
      @line = ko.obsrvable() #street name
      @lineType = ko.observable() #street type, see yandex types : street, broadway so on
      @buliding = ko.observable() #see yandex (examp: 2c3к45а)
      @room = ko.observable()
      @aux = ko.observable()
      @phones = ko.obbservableArray() #array of phone numbers (only digits, ext included)
  Addr : Addr