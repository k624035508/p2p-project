module.exports = {

  // Default for the style loading
  styleLoader: 'style-loader!css-loader!autoprefixer-loader!less-loader',
    
  scripts: {
    'transition': false,
    'alert': false,
    'button': false,
    'carousel': true,
    'collapse': true,
    'dropdown': true,
    'modal': true,
    'tooltip': true,
    'popover': true,
    'scrollspy': true, // 标书
    'tab': true,
    'affix': true
  },
  styles: {
    "mixins": true,

    "normalize": true,
    "print": true,

    "scaffolding": true,
    "type": true,
    "code": true,
    "grid": true,
    "tables": true,
    "forms": true,
    "buttons": true,

    "component-animations": true,
    "glyphicons": false, // ie8 @font-face 会出问题，所以字体样式只能放到 <head> 处加载
    "dropdowns": true,
    "button-groups": true,
    "input-groups": true,
    "navs": false,
    "navbar": false,
    "breadcrumbs": true,
    "pagination": true,
    "pager": true,
    "labels": true,
    "badges": true,
    "jumbotron": true,
    "thumbnails": true,
    "alerts": true,
    "progress-bars": true,
    "media": true,
    "list-group": true,
    "panels": true,
    "wells": true,
    "close": true,

    "modals": true,
    "tooltip": true,
    "popovers": true,
    "carousel": true,

    "utilities": true,
    "responsive-utilities": true
  }
};

