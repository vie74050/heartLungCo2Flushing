mergeInto(LibraryManager.library, {

  BrowserApplicationStarted: function () {
    window.FromUnity_ApplicationStarted();
  },

  BrowserSelect: function (str) {
    window.FromUnity_Select(UTF8ToString(str));
  },

  BrowserHover: function(str) {
    window.FromUnity_Hover(UTF8ToString(str));
  }
  
});