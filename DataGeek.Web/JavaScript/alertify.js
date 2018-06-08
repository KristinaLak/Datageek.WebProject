/*! alertifyjs - v1.4.1 - Mohammad Younes <Mohammad@alertifyjs.com> (http://alertifyjs.com) */
!function(a){"use strict";function b(a,b){a.className+=" "+b}function c(a,b){for(var c=b.split(" "),d=0;d<c.length;d+=1)a.className=a.className.replace(" "+c[d],"")}function d(){return"rtl"===a.getComputedStyle(document.body).direction}function e(){return document.documentElement&&document.documentElement.scrollTop||document.body.scrollTop}function f(){return document.documentElement&&document.documentElement.scrollLeft||document.body.scrollLeft}function g(a){for(;a.lastChild;)a.removeChild(a.lastChild)}function h(a,b){return function(){if(arguments.length>0){for(var c=[],d=0;d<arguments.length;d+=1)c.push(arguments[d]);return c.push(a),b.apply(a,c)}return b.apply(a,[null,a])}}function i(a,b){return{index:a,button:b,cancel:!1}}function j(){function a(a,b){for(var c in b)b.hasOwnProperty(c)&&(a[c]=b[c]);return a}function b(a){var b=d[a].dialog;return b&&"function"==typeof b.__init&&b.__init(b),b}function c(b,c,e,f){var g={dialog:null,factory:c};return void 0!==f&&(g.factory=function(){return a(new d[f].factory,new c)}),e||(g.dialog=a(new g.factory,q)),d[b]=g}var d={};return{defaults:l,dialog:function(d,e,f,g){if("function"!=typeof e)return b(d);if(this.hasOwnProperty(d))throw new Error("alertify.dialog: name already exists");var h=c(d,e,f,g);f?this[d]=function(){if(0===arguments.length)return h.dialog;var b=a(new h.factory,q);return b&&"function"==typeof b.__init&&b.__init(b),b.main.apply(b,arguments),b.show.apply(b)}:this[d]=function(){if(h.dialog&&"function"==typeof h.dialog.__init&&h.dialog.__init(h.dialog),0===arguments.length)return h.dialog;var a=h.dialog;return a.main.apply(h.dialog,arguments),a.show.apply(h.dialog)}},closeAll:function(a){for(var b=m.slice(0),c=0;c<b.length;c+=1){var d=b[c];(void 0===a||a!==d)&&d.close()}},setting:function(a,c,d){if("notifier"===a)return r.setting(c,d);var e=b(a);return e?e.setting(c,d):void 0},set:function(a,b,c){return this.setting(a,b,c)},get:function(a,b){return this.setting(a,b)},notify:function(a,b,c,d){return r.create(b,d).push(a,c)},message:function(a,b,c){return r.create(null,c).push(a,b)},success:function(a,b,c){return r.create("success",c).push(a,b)},error:function(a,b,c){return r.create("error",c).push(a,b)},warning:function(a,b,c){return r.create("warning",c).push(a,b)},dismissAll:function(){r.dismissAll()}}}var k={ENTER:13,ESC:27,F1:112,F12:123,LEFT:37,RIGHT:39},l={modal:!0,basic:!1,frameless:!1,movable:!0,resizable:!0,closable:!0,closableByDimmer:!0,maximizable:!0,startMaximized:!1,pinnable:!0,pinned:!0,padding:!0,overflow:!0,maintainFocus:!0,transition:"pulse",autoReset:!0,notifier:{delay:5,position:"bottom-right"},glossary:{title:"AlertifyJS",ok:"OK",cancel:"Cancel",acccpt:"Accept",deny:"Deny",confirm:"Confirm",decline:"Decline",close:"Close",maximize:"Maximize",restore:"Restore"},theme:{input:"ajs-input",ok:"ajs-ok",cancel:"ajs-cancel"}},m=[],n=function(){return document.addEventListener?function(a,b,c,d){a.addEventListener(b,c,d===!0)}:document.attachEvent?function(a,b,c){a.attachEvent("on"+b,c)}:void 0}(),o=function(){return document.removeEventListener?function(a,b,c,d){a.removeEventListener(b,c,d===!0)}:document.detachEvent?function(a,b,c){a.detachEvent("on"+b,c)}:void 0}(),p=function(){var a,b,c=!1,d={animation:"animationend",OAnimation:"oAnimationEnd oanimationend",msAnimation:"MSAnimationEnd",MozAnimation:"animationend",WebkitAnimation:"webkitAnimationEnd"};for(a in d)if(void 0!==document.documentElement.style[a]){b=d[a],c=!0;break}return{type:b,supported:c}}(),q=function(){function j(a){if(!a.__internal){delete a.__init,null===ua&&document.body.setAttribute("tabindex","0");var c;"function"==typeof a.setup?(c=a.setup(),c.options=c.options||{},c.focus=c.focus||{}):c={buttons:[],focus:{element:null,select:!1},options:{}},"object"!=typeof a.hooks&&(a.hooks={});var d=[];if(Array.isArray(c.buttons))for(var e=0;e<c.buttons.length;e+=1){var f=c.buttons[e],g={};for(var i in f)f.hasOwnProperty(i)&&(g[i]=f[i]);d.push(g)}var j=a.__internal={isOpen:!1,activeElement:document.body,timerIn:void 0,timerOut:void 0,buttons:d,focus:c.focus,options:{title:void 0,modal:void 0,basic:void 0,frameless:void 0,pinned:void 0,movable:void 0,resizable:void 0,autoReset:void 0,closable:void 0,closableByDimmer:void 0,maximizable:void 0,startMaximized:void 0,pinnable:void 0,transition:void 0,padding:void 0,overflow:void 0,onshow:void 0,onclose:void 0,onfocus:void 0},resetHandler:void 0,beginMoveHandler:void 0,beginResizeHandler:void 0,bringToFrontHandler:void 0,modalClickHandler:void 0,buttonsClickHandler:void 0,commandsClickHandler:void 0,transitionInHandler:void 0,transitionOutHandler:void 0},k={};k.root=document.createElement("div"),k.root.className=xa.base+" "+xa.hidden+" ",k.root.innerHTML=wa.dimmer+wa.modal,k.dimmer=k.root.firstChild,k.modal=k.root.lastChild,k.modal.innerHTML=wa.dialog,k.dialog=k.modal.firstChild,k.dialog.innerHTML=wa.reset+wa.commands+wa.header+wa.body+wa.footer+wa.resizeHandle+wa.reset,k.reset=[],k.reset.push(k.dialog.firstChild),k.reset.push(k.dialog.lastChild),k.commands={},k.commands.container=k.reset[0].nextSibling,k.commands.pin=k.commands.container.firstChild,k.commands.maximize=k.commands.pin.nextSibling,k.commands.close=k.commands.maximize.nextSibling,k.header=k.commands.container.nextSibling,k.body=k.header.nextSibling,k.body.innerHTML=wa.content,k.content=k.body.firstChild,k.footer=k.body.nextSibling,k.footer.innerHTML=wa.buttons.auxiliary+wa.buttons.primary,k.resizeHandle=k.footer.nextSibling,k.buttons={},k.buttons.auxiliary=k.footer.firstChild,k.buttons.primary=k.buttons.auxiliary.nextSibling,k.buttons.primary.innerHTML=wa.button,k.buttonTemplate=k.buttons.primary.firstChild,k.buttons.primary.removeChild(k.buttonTemplate);for(var l=0;l<a.__internal.buttons.length;l+=1){var m=a.__internal.buttons[l];ta.indexOf(m.key)<0&&ta.push(m.key),m.element=k.buttonTemplate.cloneNode(),m.element.innerHTML=m.text,"string"==typeof m.className&&""!==m.className&&b(m.element,m.className);for(var n in m.attrs)"className"!==n&&m.attrs.hasOwnProperty(n)&&m.element.setAttribute(n,m.attrs[n]);"auxiliary"===m.scope?k.buttons.auxiliary.appendChild(m.element):k.buttons.primary.appendChild(m.element)}a.elements=k,j.resetHandler=h(a,T),j.beginMoveHandler=h(a,X),j.beginResizeHandler=h(a,ba),j.bringToFrontHandler=h(a,x),j.modalClickHandler=h(a,N),j.buttonsClickHandler=h(a,P),j.commandsClickHandler=h(a,B),j.transitionInHandler=h(a,U),j.transitionOutHandler=h(a,V),a.set("title",void 0===c.options.title?s.defaults.glossary.title:c.options.title),a.set("modal",void 0===c.options.modal?s.defaults.modal:c.options.modal),a.set("basic",void 0===c.options.basic?s.defaults.basic:c.options.basic),a.set("frameless",void 0===c.options.frameless?s.defaults.frameless:c.options.frameless),a.set("movable",void 0===c.options.movable?s.defaults.movable:c.options.movable),a.set("resizable",void 0===c.options.resizable?s.defaults.resizable:c.options.resizable),a.set("autoReset",void 0===c.options.autoReset?s.defaults.autoReset:c.options.autoReset),a.set("closable",void 0===c.options.closable?s.defaults.closable:c.options.closable),a.set("closableByDimmer",void 0===c.options.closableByDimmer?s.defaults.closableByDimmer:c.options.closableByDimmer),a.set("maximizable",void 0===c.options.maximizable?s.defaults.maximizable:c.options.maximizable),a.set("startMaximized",void 0===c.options.startMaximized?s.defaults.startMaximized:c.options.startMaximized),a.set("pinnable",void 0===c.options.pinnable?s.defaults.pinnable:c.options.pinnable),a.set("pinned",void 0===c.options.pinned?s.defaults.pinned:c.options.pinned),a.set("transition",void 0===c.options.transition?s.defaults.transition:c.options.transition),a.set("padding",void 0===c.options.padding?s.defaults.padding:c.options.padding),a.set("overflow",void 0===c.options.overflow?s.defaults.overflow:c.options.overflow),"function"==typeof a.build&&a.build()}document.body.appendChild(a.elements.root)}function l(){ra=a.scrollX,sa=a.scrollY}function q(){a.scrollTo(ra,sa)}function r(){for(var a=0,d=0;d<m.length;d+=1){var e=m[d];(e.isModal()||e.isMaximized())&&(a+=1)}0===a?c(document.body,xa.noOverflow):a>0&&document.body.className.indexOf(xa.noOverflow)<0&&b(document.body,xa.noOverflow)}function t(a,d,e){"string"==typeof e&&c(a.elements.root,xa.prefix+e),b(a.elements.root,xa.prefix+d),ua=a.elements.root.offsetWidth}function u(a){a.get("modal")?(c(a.elements.root,xa.modeless),a.isOpen()&&(ka(a),J(a),r())):(b(a.elements.root,xa.modeless),a.isOpen()&&(ja(a),J(a),r()))}function v(a){a.get("basic")?b(a.elements.root,xa.basic):c(a.elements.root,xa.basic)}function w(a){a.get("frameless")?b(a.elements.root,xa.frameless):c(a.elements.root,xa.frameless)}function x(a,b){for(var c=m.indexOf(b),d=c+1;d<m.length;d+=1)if(m[d].isModal())return;return document.body.lastChild!==b.elements.root&&(document.body.appendChild(b.elements.root),m.splice(m.indexOf(b),1),m.push(b),S(b)),!1}function y(a,d,e,f){switch(d){case"title":a.setHeader(f);break;case"modal":u(a);break;case"basic":v(a);break;case"frameless":w(a);break;case"pinned":K(a);break;case"closable":M(a);break;case"maximizable":L(a);break;case"pinnable":G(a);break;case"movable":_(a);break;case"resizable":fa(a);break;case"transition":t(a,f,e);break;case"padding":f?c(a.elements.root,xa.noPadding):a.elements.root.className.indexOf(xa.noPadding)<0&&b(a.elements.root,xa.noPadding);break;case"overflow":f?c(a.elements.root,xa.noOverflow):a.elements.root.className.indexOf(xa.noOverflow)<0&&b(a.elements.root,xa.noOverflow);break;case"transition":t(a,f,e)}"function"==typeof a.hooks.onupdate&&a.hooks.onupdate.call(a,d,e,f)}function z(a,b,c,d,e){var f={op:void 0,items:[]};if("undefined"==typeof e&&"string"==typeof d)f.op="get",b.hasOwnProperty(d)?(f.found=!0,f.value=b[d]):(f.found=!1,f.value=void 0);else{var g;if(f.op="set","object"==typeof d){var h=d;for(var i in h)b.hasOwnProperty(i)?(b[i]!==h[i]&&(g=b[i],b[i]=h[i],c.call(a,i,g,h[i])),f.items.push({key:i,value:h[i],found:!0})):f.items.push({key:i,value:h[i],found:!1})}else{if("string"!=typeof d)throw new Error("args must be a string or object");b.hasOwnProperty(d)?(b[d]!==e&&(g=b[d],b[d]=e,c.call(a,d,g,e)),f.items.push({key:d,value:e,found:!0})):f.items.push({key:d,value:e,found:!1})}}return f}function A(a){var b;O(a,function(a){return b=a.invokeOnClose===!0}),!b&&a.isOpen()&&a.close()}function B(a,b){var c=a.srcElement||a.target;switch(c){case b.elements.commands.pin:b.isPinned()?D(b):C(b);break;case b.elements.commands.maximize:b.isMaximized()?F(b):E(b);break;case b.elements.commands.close:A(b)}return!1}function C(a){a.set("pinned",!0)}function D(a){a.set("pinned",!1)}function E(a){b(a.elements.root,xa.maximized),a.isOpen()&&r()}function F(a){c(a.elements.root,xa.maximized),a.isOpen()&&r()}function G(a){a.get("pinnable")?b(a.elements.root,xa.pinnable):c(a.elements.root,xa.pinnable)}function H(a){var b=f();a.elements.modal.style.marginTop=e()+"px",a.elements.modal.style.marginLeft=b+"px",a.elements.modal.style.marginRight=-b+"px"}function I(a){var b=parseInt(a.elements.modal.style.marginTop,10),c=parseInt(a.elements.modal.style.marginLeft,10);if(a.elements.modal.style.marginTop="",a.elements.modal.style.marginLeft="",a.elements.modal.style.marginRight="",a.isOpen()){var d=0,g=0;""!==a.elements.dialog.style.top&&(d=parseInt(a.elements.dialog.style.top,10)),a.elements.dialog.style.top=d+(b-e())+"px",""!==a.elements.dialog.style.left&&(g=parseInt(a.elements.dialog.style.left,10)),a.elements.dialog.style.left=g+(c-f())+"px"}}function J(a){a.get("modal")||a.get("pinned")?I(a):H(a)}function K(a){a.get("pinned")?(c(a.elements.root,xa.unpinned),a.isOpen()&&I(a)):(b(a.elements.root,xa.unpinned),a.isOpen()&&!a.isModal()&&H(a))}function L(a){a.get("maximizable")?b(a.elements.root,xa.maximizable):c(a.elements.root,xa.maximizable)}function M(a){a.get("closable")?(b(a.elements.root,xa.closable),pa(a)):(c(a.elements.root,xa.closable),qa(a))}function N(a,b){var c=a.srcElement||a.target;return ya||c!==b.elements.modal||b.get("closableByDimmer")!==!0||A(b),ya=!1,!1}function O(a,b){for(var c=0;c<a.__internal.buttons.length;c+=1){var d=a.__internal.buttons[c];if(!d.element.disabled&&b(d)){var e=i(c,d);"function"==typeof a.callback&&a.callback.apply(a,[e]),e.cancel===!1&&a.close();break}}}function P(a,b){var c=a.srcElement||a.target;O(b,function(a){return a.element===c&&(za=!0)})}function Q(a){if(za)return void(za=!1);var b=m[m.length-1],c=a.keyCode;return 0===b.__internal.buttons.length&&c===k.ESC&&b.get("closable")===!0?(A(b),!1):ta.indexOf(c)>-1?(O(b,function(a){return a.key===c}),!1):void 0}function R(a){var b=m[m.length-1],c=a.keyCode;if(c===k.LEFT||c===k.RIGHT){for(var d=b.__internal.buttons,e=0;e<d.length;e+=1)if(document.activeElement===d[e].element)switch(c){case k.LEFT:return void d[(e||d.length)-1].element.focus();case k.RIGHT:return void d[(e+1)%d.length].element.focus()}}else if(c<k.F12+1&&c>k.F1-1&&ta.indexOf(c)>-1)return a.preventDefault(),a.stopPropagation(),O(b,function(a){return a.key===c}),!1}function S(a,b){if(b)b.focus();else{var c=a.__internal.focus,d=c.element;switch(typeof c.element){case"number":a.__internal.buttons.length>c.element&&(d=a.get("basic")===!0?a.elements.reset[0]:a.__internal.buttons[c.element].element);break;case"string":d=a.elements.body.querySelector(c.element);break;case"function":d=c.element.call(a)}"undefined"!=typeof d&&null!==d||0!==a.__internal.buttons.length||(d=a.elements.reset[0]),d&&d.focus&&(d.focus(),c.select&&d.select&&d.select())}}function T(a,b){if(!b)for(var c=m.length-1;c>-1;c-=1)if(m[c].isModal()){b=m[c];break}if(b&&b.isModal()){var d,e=a.srcElement||a.target,f=e===b.elements.reset[1]||0===b.__internal.buttons.length&&e===document.body;f&&(b.get("maximizable")?d=b.elements.commands.maximize:b.get("closable")&&(d=b.elements.commands.close)),void 0===d&&("number"==typeof b.__internal.focus.element?e===b.elements.reset[0]?d=b.elements.buttons.auxiliary.firstChild||b.elements.buttons.primary.firstChild:f&&(d=b.elements.reset[0]):e===b.elements.reset[0]&&(d=b.elements.buttons.primary.lastChild||b.elements.buttons.auxiliary.lastChild)),S(b,d)}}function U(a,b){clearTimeout(b.__internal.timerIn),S(b),q(),za=!1,"function"==typeof b.get("onfocus")&&b.get("onfocus").call(b),o(b.elements.dialog,p.type,b.__internal.transitionInHandler),c(b.elements.root,xa.animationIn)}function V(a,b){clearTimeout(b.__internal.timerOut),o(b.elements.dialog,p.type,b.__internal.transitionOutHandler),$(b),ea(b),b.isMaximized()&&!b.get("startMaximized")&&F(b),s.defaults.maintainFocus&&b.__internal.activeElement&&(b.__internal.activeElement.focus(),b.__internal.activeElement=null)}function W(a,b){b.style.left=a[Da]-Ba+"px",b.style.top=a[Ea]-Ca+"px"}function X(a,c){if(null===Fa&&!c.isMaximized()&&c.get("movable")){var d;if("touchstart"===a.type?(a.preventDefault(),d=a.targetTouches[0],Da="clientX",Ea="clientY"):0===a.button&&(d=a),d){Aa=c,Ba=d[Da],Ca=d[Ea];var e=c.elements.dialog;return b(e,xa.capture),e.style.left&&(Ba-=parseInt(e.style.left,10)),e.style.top&&(Ca-=parseInt(e.style.top,10)),W(d,e),b(document.body,xa.noSelection),!1}}}function Y(a){if(Aa){var b;"touchmove"===a.type?(a.preventDefault(),b=a.targetTouches[0]):0===a.button&&(b=a),b&&W(b,Aa.elements.dialog)}}function Z(){if(Aa){var a=Aa.elements.dialog;Aa=null,c(document.body,xa.noSelection),c(a,xa.capture)}}function $(a){Aa=null;var b=a.elements.dialog;b.style.left=b.style.top=""}function _(a){a.get("movable")?(b(a.elements.root,xa.movable),a.isOpen()&&la(a)):($(a),c(a.elements.root,xa.movable),a.isOpen()&&ma(a))}function aa(a,b,c){var e=b,f=0,g=0;do f+=e.offsetLeft,g+=e.offsetTop;while(e=e.offsetParent);var h,i;c===!0?(h=a.pageX,i=a.pageY):(h=a.clientX,i=a.clientY);var j=d();if(j&&(h=document.body.offsetWidth-h,isNaN(Ga)||(f=document.body.offsetWidth-f-b.offsetWidth)),b.style.height=i-g+Ja+"px",b.style.width=h-f+Ja+"px",!isNaN(Ga)){var k=.5*Math.abs(b.offsetWidth-Ha);j&&(k*=-1),b.offsetWidth>Ha?b.style.left=Ga+k+"px":b.offsetWidth>=Ia&&(b.style.left=Ga-k+"px")}}function ba(a,c){if(!c.isMaximized()){var d;if("touchstart"===a.type?(a.preventDefault(),d=a.targetTouches[0]):0===a.button&&(d=a),d){Fa=c,Ja=c.elements.resizeHandle.offsetHeight/2;var e=c.elements.dialog;return b(e,xa.capture),Ga=parseInt(e.style.left,10),e.style.height=e.offsetHeight+"px",e.style.minHeight=c.elements.header.offsetHeight+c.elements.footer.offsetHeight+"px",e.style.width=(Ha=e.offsetWidth)+"px","none"!==e.style.maxWidth&&(e.style.minWidth=(Ia=e.offsetWidth)+"px"),e.style.maxWidth="none",b(document.body,xa.noSelection),!1}}}function ca(a){if(Fa){var b;"touchmove"===a.type?(a.preventDefault(),b=a.targetTouches[0]):0===a.button&&(b=a),b&&aa(b,Fa.elements.dialog,!Fa.get("modal")&&!Fa.get("pinned"))}}function da(){if(Fa){var a=Fa.elements.dialog;Fa=null,c(document.body,xa.noSelection),c(a,xa.capture),ya=!0}}function ea(a){Fa=null;var b=a.elements.dialog;"none"===b.style.maxWidth&&(b.style.maxWidth=b.style.minWidth=b.style.width=b.style.height=b.style.minHeight=b.style.left="",Ga=Number.Nan,Ha=Ia=Ja=0)}function fa(a){a.get("resizable")?(b(a.elements.root,xa.resizable),a.isOpen()&&na(a)):(ea(a),c(a.elements.root,xa.resizable),a.isOpen()&&oa(a))}function ga(){for(var a=0;a<m.length;a+=1){var b=m[a];b.get("autoReset")&&($(b),ea(b))}}function ha(b){1===m.length&&(n(a,"resize",ga),n(document.body,"keyup",Q),n(document.body,"keydown",R),n(document.body,"focus",T),n(document.documentElement,"mousemove",Y),n(document.documentElement,"touchmove",Y),n(document.documentElement,"mouseup",Z),n(document.documentElement,"touchend",Z),n(document.documentElement,"mousemove",ca),n(document.documentElement,"touchmove",ca),n(document.documentElement,"mouseup",da),n(document.documentElement,"touchend",da)),n(b.elements.commands.container,"click",b.__internal.commandsClickHandler),n(b.elements.footer,"click",b.__internal.buttonsClickHandler),n(b.elements.reset[0],"focus",b.__internal.resetHandler),n(b.elements.reset[1],"focus",b.__internal.resetHandler),za=!0,n(b.elements.dialog,p.type,b.__internal.transitionInHandler),b.get("modal")||ja(b),b.get("resizable")&&na(b),b.get("movable")&&la(b)}function ia(b){1===m.length&&(o(a,"resize",ga),o(document.body,"keyup",Q),o(document.body,"keydown",R),o(document.body,"focus",T),o(document.documentElement,"mousemove",Y),o(document.documentElement,"mouseup",Z),o(document.documentElement,"mousemove",ca),o(document.documentElement,"mouseup",da)),o(b.elements.commands.container,"click",b.__internal.commandsClickHandler),o(b.elements.footer,"click",b.__internal.buttonsClickHandler),o(b.elements.reset[0],"focus",b.__internal.resetHandler),o(b.elements.reset[1],"focus",b.__internal.resetHandler),n(b.elements.dialog,p.type,b.__internal.transitionOutHandler),b.get("modal")||ka(b),b.get("movable")&&ma(b),b.get("resizable")&&oa(b)}function ja(a){n(a.elements.dialog,"focus",a.__internal.bringToFrontHandler,!0)}function ka(a){o(a.elements.dialog,"focus",a.__internal.bringToFrontHandler,!0)}function la(a){n(a.elements.header,"mousedown",a.__internal.beginMoveHandler),n(a.elements.header,"touchstart",a.__internal.beginMoveHandler)}function ma(a){o(a.elements.header,"mousedown",a.__internal.beginMoveHandler),o(a.elements.header,"touchstart",a.__internal.beginMoveHandler)}function na(a){n(a.elements.resizeHandle,"mousedown",a.__internal.beginResizeHandler),n(a.elements.resizeHandle,"touchstart",a.__internal.beginResizeHandler)}function oa(a){o(a.elements.resizeHandle,"mousedown",a.__internal.beginResizeHandler),o(a.elements.resizeHandle,"touchstart",a.__internal.beginResizeHandler)}function pa(a){n(a.elements.modal,"click",a.__internal.modalClickHandler)}function qa(a){o(a.elements.modal,"click",a.__internal.modalClickHandler)}var ra,sa,ta=[],ua=null,va=a.navigator.userAgent.indexOf("Safari")>-1&&a.navigator.userAgent.indexOf("Chrome")<0,wa={dimmer:'<div class="ajs-dimmer"></div>',modal:'<div class="ajs-modal" tabindex="0"></div>',dialog:'<div class="ajs-dialog" tabindex="0"></div>',reset:'<button class="ajs-reset"></button>',commands:'<div class="ajs-commands"><button class="ajs-pin"></button><button class="ajs-maximize"></button><button class="ajs-close"></button></div>',header:'<div class="ajs-header"></div>',body:'<div class="ajs-body"></div>',content:'<div class="ajs-content"></div>',footer:'<div class="ajs-footer"></div>',buttons:{primary:'<div class="ajs-primary ajs-buttons"></div>',auxiliary:'<div class="ajs-auxiliary ajs-buttons"></div>'},button:'<button class="ajs-button"></button>',resizeHandle:'<div class="ajs-handle"></div>'},xa={base:"alertify",prefix:"ajs-",hidden:"ajs-hidden",noSelection:"ajs-no-selection",noOverflow:"ajs-no-overflow",noPadding:"ajs-no-padding",modeless:"ajs-modeless",movable:"ajs-movable",resizable:"ajs-resizable",capture:"ajs-capture",fixed:"ajs-fixed",closable:"ajs-closable",maximizable:"ajs-maximizable",maximize:"ajs-maximize",restore:"ajs-restore",pinnable:"ajs-pinnable",unpinned:"ajs-unpinned",pin:"ajs-pin",maximized:"ajs-maximized",animationIn:"ajs-in",animationOut:"ajs-out",shake:"ajs-shake",basic:"ajs-basic",frameless:"ajs-frameless"},ya=!1,za=!1,Aa=null,Ba=0,Ca=0,Da="pageX",Ea="pageY",Fa=null,Ga=Number.Nan,Ha=0,Ia=0,Ja=0;return{__init:j,isOpen:function(){return this.__internal.isOpen},isModal:function(){return this.elements.root.className.indexOf(xa.modeless)<0},isMaximized:function(){return this.elements.root.className.indexOf(xa.maximized)>-1},isPinned:function(){return this.elements.root.className.indexOf(xa.unpinned)<0},maximize:function(){return this.isMaximized()||E(this),this},restore:function(){return this.isMaximized()&&F(this),this},pin:function(){return this.isPinned()||C(this),this},unpin:function(){return this.isPinned()&&D(this),this},moveTo:function(a,b){if(!isNaN(a)&&!isNaN(b)){var c=this.elements.dialog,e=c,f=0,g=0;c.style.left&&(f-=parseInt(c.style.left,10)),c.style.top&&(g-=parseInt(c.style.top,10));do f+=e.offsetLeft,g+=e.offsetTop;while(e=e.offsetParent);var h=a-f,i=b-g;d()&&(h*=-1),c.style.left=h+"px",c.style.top=i+"px"}return this},resizeTo:function(a,b){var c=parseFloat(a),d=parseFloat(b),e=/(\d*\.\d+|\d+)%/;if(!isNaN(c)&&!isNaN(d)&&this.get("resizable")===!0){(""+a).match(e)&&(c=c/100*document.documentElement.clientWidth),(""+b).match(e)&&(d=d/100*document.documentElement.clientHeight);var f=this.elements.dialog;"none"!==f.style.maxWidth&&(f.style.minWidth=(Ia=f.offsetWidth)+"px"),f.style.maxWidth="none",f.style.minHeight=this.elements.header.offsetHeight+this.elements.footer.offsetHeight+"px",f.style.width=c+"px",f.style.height=d+"px"}return this},setting:function(a,b){var c=this,d=z(this,this.__internal.options,function(a,b,d){y(c,a,b,d)},a,b);if("get"===d.op)return d.found?d.value:"undefined"!=typeof this.settings?z(this,this.settings,this.settingUpdated||function(){},a,b).value:void 0;if("set"===d.op){if(d.items.length>0)for(var e=this.settingUpdated||function(){},f=0;f<d.items.length;f+=1){var g=d.items[f];g.found||"undefined"==typeof this.settings||z(this,this.settings,e,g.key,g.value)}return this}},set:function(a,b){return this.setting(a,b),this},get:function(a){return this.setting(a)},setHeader:function(b){return"string"==typeof b?(g(this.elements.header),this.elements.header.innerHTML=b):b instanceof a.HTMLElement&&this.elements.header.firstChild!==b&&(g(this.elements.header),this.elements.header.appendChild(b)),this},setContent:function(b){return"string"==typeof b?(g(this.elements.content),this.elements.content.innerHTML=b):b instanceof a.HTMLElement&&this.elements.content.firstChild!==b&&(g(this.elements.content),this.elements.content.appendChild(b)),this},showModal:function(a){return this.show(!0,a)},show:function(a,d){if(j(this),this.__internal.isOpen){$(this),ea(this),b(this.elements.dialog,xa.shake);var e=this;setTimeout(function(){c(e.elements.dialog,xa.shake)},200)}else{if(this.__internal.isOpen=!0,m.push(this),s.defaults.maintainFocus&&(this.__internal.activeElement=document.activeElement),"function"==typeof this.prepare&&this.prepare(),ha(this),void 0!==a&&this.set("modal",a),l(),r(),"string"==typeof d&&""!==d&&(this.__internal.className=d,b(this.elements.root,d)),this.get("startMaximized")?this.maximize():this.isMaximized()&&F(this),J(this),c(this.elements.root,xa.animationOut),b(this.elements.root,xa.animationIn),clearTimeout(this.__internal.timerIn),this.__internal.timerIn=setTimeout(this.__internal.transitionInHandler,p.supported?1e3:100),va){var f=this.elements.root;f.style.display="none",setTimeout(function(){f.style.display="block"},0)}ua=this.elements.root.offsetWidth,c(this.elements.root,xa.hidden),"function"==typeof this.hooks.onshow&&this.hooks.onshow.call(this),"function"==typeof this.get("onshow")&&this.get("onshow").call(this)}return this},close:function(){return this.__internal.isOpen&&(ia(this),c(this.elements.root,xa.animationIn),b(this.elements.root,xa.animationOut),clearTimeout(this.__internal.timerOut),this.__internal.timerOut=setTimeout(this.__internal.transitionOutHandler,p.supported?1e3:100),b(this.elements.root,xa.hidden),ua=this.elements.modal.offsetWidth,"undefined"!=typeof this.__internal.className&&""!==this.__internal.className&&c(this.elements.root,this.__internal.className),"function"==typeof this.hooks.onclose&&this.hooks.onclose.call(this),"function"==typeof this.get("onclose")&&this.get("onclose").call(this),m.splice(m.indexOf(this),1),this.__internal.isOpen=!1,r()),this},closeOthers:function(){return s.closeAll(this),this}}}(),r=function(){function d(a){a.__internal||(a.__internal={position:s.defaults.notifier.position,delay:s.defaults.notifier.delay},l=document.createElement("DIV"),i(a)),l.parentNode!==document.body&&document.body.appendChild(l)}function e(a){a.__internal.pushed=!0,m.push(a)}function f(a){m.splice(m.indexOf(a),1),a.__internal.pushed=!1}function i(a){switch(l.className=q.base,a.__internal.position){case"top-right":b(l,q.top+" "+q.right);break;case"top-left":b(l,q.top+" "+q.left);break;case"bottom-left":b(l,q.bottom+" "+q.left);break;default:case"bottom-right":b(l,q.bottom+" "+q.right)}}function j(d,i){function j(a,b){b.dismiss(!0)}function m(a,b){o(b.element,p.type,m),l.removeChild(b.element)}function s(a){return a.__internal||(a.__internal={pushed:!1,delay:void 0,timer:void 0,clickHandler:void 0,transitionEndHandler:void 0,transitionTimeout:void 0},a.__internal.clickHandler=h(a,j),a.__internal.transitionEndHandler=h(a,m)),a}function t(a){clearTimeout(a.__internal.timer),clearTimeout(a.__internal.transitionTimeout)}return s({element:d,push:function(a,c){if(!this.__internal.pushed){e(this),t(this);var d,f;switch(arguments.length){case 0:f=this.__internal.delay;break;case 1:"number"==typeof a?f=a:(d=a,f=this.__internal.delay);break;case 2:d=a,f=c}return"undefined"!=typeof d&&this.setContent(d),r.__internal.position.indexOf("top")<0?l.appendChild(this.element):l.insertBefore(this.element,l.firstChild),k=this.element.offsetWidth,b(this.element,q.visible),n(this.element,"click",this.__internal.clickHandler),this.delay(f)}return this},ondismiss:function(){},callback:i,dismiss:function(a){return this.__internal.pushed&&(t(this),("function"!=typeof this.ondismiss||this.ondismiss.call(this)!==!1)&&(o(this.element,"click",this.__internal.clickHandler),"undefined"!=typeof this.element&&this.element.parentNode===l&&(this.__internal.transitionTimeout=setTimeout(this.__internal.transitionEndHandler,p.supported?1e3:100),c(this.element,q.visible),"function"==typeof this.callback&&this.callback.call(this,a)),f(this))),this},delay:function(a){if(t(this),this.__internal.delay="undefined"==typeof a||isNaN(+a)?r.__internal.delay:+a,this.__internal.delay>0){var b=this;this.__internal.timer=setTimeout(function(){b.dismiss()},1e3*this.__internal.delay)}return this},setContent:function(b){return"string"==typeof b?(g(this.element),this.element.innerHTML=b):b instanceof a.HTMLElement&&this.element.firstChild!==b&&(g(this.element),this.element.appendChild(b)),this},dismissOthers:function(){return r.dismissAll(this),this}})}var k,l,m=[],q={base:"alertify-notifier",message:"ajs-message",top:"ajs-top",right:"ajs-right",bottom:"ajs-bottom",left:"ajs-left",visible:"ajs-visible",hidden:"ajs-hidden"};return{setting:function(a,b){if(d(this),"undefined"==typeof b)return this.__internal[a];switch(a){case"position":this.__internal.position=b,i(this);break;case"delay":this.__internal.delay=b}return this},set:function(a,b){return this.setting(a,b),this},get:function(a){return this.setting(a)},create:function(a,b){d(this);var c=document.createElement("div");return c.className=q.message+("string"==typeof a&&""!==a?" ajs-"+a:""),j(c,b)},dismissAll:function(a){for(var b=m.slice(0),c=0;c<b.length;c+=1){var d=b[c];(void 0===a||a!==d)&&d.dismiss()}}}}(),s=new j;s.dialog("alert",function(){return{main:function(a,b,c){var d,e,f;switch(arguments.length){case 1:e=a;break;case 2:"function"==typeof b?(e=a,f=b):(d=a,e=b);break;case 3:d=a,e=b,f=c}return this.set("title",d),this.set("message",e),this.set("onok",f),this},setup:function(){return{buttons:[{text:s.defaults.glossary.ok,key:k.ESC,invokeOnClose:!0,className:s.defaults.theme.ok}],focus:{element:0,select:!1},options:{maximizable:!1,resizable:!1}}},build:function(){},prepare:function(){},setMessage:function(a){this.setContent(a)},settings:{message:void 0,onok:void 0,label:void 0},settingUpdated:function(a,b,c){switch(a){case"message":this.setMessage(c);break;case"label":this.__internal.buttons[0].element&&(this.__internal.buttons[0].element.innerHTML=c)}},callback:function(a){if("function"==typeof this.get("onok")){var b=this.get("onok").call(this,a);"undefined"!=typeof b&&(a.cancel=!b)}}}}),s.dialog("confirm",function(){function a(a){null!==c.timer&&(clearInterval(c.timer),c.timer=null,a.__internal.buttons[c.index].element.innerHTML=c.text)}function b(b,d,e){a(b),c.duration=e,c.index=d,c.text=b.__internal.buttons[d].element.innerHTML,c.timer=setInterval(h(b,c.task),1e3),c.task(null,b)}var c={timer:null,index:null,text:null,duration:null,task:function(b,d){if(d.isOpen()){if(d.__internal.buttons[c.index].element.innerHTML=c.text+" (&#8207;"+c.duration+"&#8207;) ",c.duration-=1,-1===c.duration){a(d);var e=d.__internal.buttons[c.index],f=i(c.index,e);"function"==typeof d.callback&&d.callback.apply(d,[f]),f.close!==!1&&d.close()}}else a(d)}};return{main:function(a,b,c,d){var e,f,g,h;switch(arguments.length){case 1:f=a;break;case 2:f=a,g=b;break;case 3:f=a,g=b,h=c;break;case 4:e=a,f=b,g=c,h=d}return this.set("title",e),this.set("message",f),this.set("onok",g),this.set("oncancel",h),this},setup:function(){return{buttons:[{text:s.defaults.glossary.ok,key:k.ENTER,className:s.defaults.theme.ok},{text:s.defaults.glossary.cancel,key:k.ESC,invokeOnClose:!0,className:s.defaults.theme.cancel}],focus:{element:0,select:!1},options:{maximizable:!1,resizable:!1}}},build:function(){},prepare:function(){},setMessage:function(a){this.setContent(a)},settings:{message:null,labels:null,onok:null,oncancel:null,defaultFocus:null,reverseButtons:null},settingUpdated:function(a,b,c){switch(a){case"message":this.setMessage(c);break;case"labels":"ok"in c&&this.__internal.buttons[0].element&&(this.__internal.buttons[0].text=c.ok,this.__internal.buttons[0].element.innerHTML=c.ok),"cancel"in c&&this.__internal.buttons[1].element&&(this.__internal.buttons[1].text=c.cancel,this.__internal.buttons[1].element.innerHTML=c.cancel);break;case"reverseButtons":this.elements.buttons.primary.appendChild(c===!0?this.__internal.buttons[0].element:this.__internal.buttons[1].element);break;case"defaultFocus":this.__internal.focus.element="ok"===c?0:1}},callback:function(b){a(this);var c;switch(b.index){case 0:"function"==typeof this.get("onok")&&(c=this.get("onok").call(this,b),"undefined"!=typeof c&&(b.cancel=!c));break;case 1:"function"==typeof this.get("oncancel")&&(c=this.get("oncancel").call(this,b),"undefined"!=typeof c&&(b.cancel=!c))}},autoOk:function(a){return b(this,0,a),this},autoCancel:function(a){return b(this,1,a),this}}}),s.dialog("prompt",function(){var b=document.createElement("INPUT"),c=document.createElement("P");

return{main:function(a,b,c,d,e){var f,g,h,i,j;switch(arguments.length){case 1:g=a;break;case 2:g=a,h=b;break;case 3:g=a,h=b,i=c;break;case 4:g=a,h=b,i=c,j=d;break;case 5:f=a,g=b,h=c,i=d,j=e}return this.set("title",f),this.set("message",g),this.set("value",h),this.set("onok",i),this.set("oncancel",j),this},setup:function(){return{buttons:[{text:s.defaults.glossary.ok,key:k.ENTER,className:s.defaults.theme.ok},{text:s.defaults.glossary.cancel,key:k.ESC,invokeOnClose:!0,className:s.defaults.theme.cancel}],focus:{element:b,select:!0},options:{maximizable:!1,resizable:!1}}},build:function(){b.className=s.defaults.theme.input,b.setAttribute("type","text"),b.value=this.get("value"),this.elements.content.appendChild(c),this.elements.content.appendChild(b)},prepare:function(){},setMessage:function(b){"string"==typeof b?(g(c),c.innerHTML=b):b instanceof a.HTMLElement&&c.firstChild!==b&&(g(c),c.appendChild(b))},settings:{message:void 0,labels:void 0,onok:void 0,oncancel:void 0,value:"",type:"text",reverseButtons:void 0},settingUpdated:function(a,c,d){switch(a){case"message":this.setMessage(d);break;case"value":b.value=d;break;case"type":switch(d){case"text":case"color":case"date":case"datetime-local":case"email":case"month":case"number":case"password":case"search":case"tel":case"time":case"week":b.type=d;break;default:b.type="text"}break;case"labels":d.ok&&this.__internal.buttons[0].element&&(this.__internal.buttons[0].element.innerHTML=d.ok),d.cancel&&this.__internal.buttons[1].element&&(this.__internal.buttons[1].element.innerHTML=d.cancel);break;case"reverseButtons":this.elements.buttons.primary.appendChild(d===!0?this.__internal.buttons[0].element:this.__internal.buttons[1].element)}},callback:function(a){var c;switch(a.index){case 0:this.settings.value=b.value,"function"==typeof this.get("onok")&&(c=this.get("onok").call(this,a,this.settings.value),"undefined"!=typeof c&&(a.cancel=!c));break;case 1:"function"==typeof this.get("oncancel")&&(c=this.get("oncancel").call(this,a),"undefined"!=typeof c&&(a.cancel=!c))}}}}),"object"==typeof module&&"object"==typeof module.exports?module.exports=s:"function"==typeof define&&define.amd?define([],function(){return s}):a.alertify||(a.alertify=s)}("undefined"!=typeof window?window:this);