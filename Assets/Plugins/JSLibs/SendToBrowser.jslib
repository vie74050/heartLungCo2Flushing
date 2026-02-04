mergeInto(LibraryManager.library, {

  BrowserApplicationStarted: function (str) {
    window.FromUnity_ApplicationStarted(UTF8ToString(str));
  },
  
  BrowserSelect: function (str) {
    window.FromUnity_Select(UTF8ToString(str));
  },

  BrowserHover: function(str) {
    window.FromUnity_Hover(UTF8ToString(str));
  }
  
});