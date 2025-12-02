mergeInto(LibraryManager.library, {

  BrowserApplicationStarted: function () {
    window.FromUnity_ApplicationStarted();
  },

  BrowserSelect: function (str) {
    window.FromUnity_Select(Pointer_stringify(str));
  }
  
});