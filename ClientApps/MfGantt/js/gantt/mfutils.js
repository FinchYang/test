
(function ($) {


		// Standard prototype inherit function
		if (typeof Object.create !== 'function') {
			Object.create = function (p, o) {
				function F() {}
				F.prototype = p;
				return $.extend(new F(), o);
			};
		}

    if ($) {
	

		// declare static method/property namespace for M-Files (mf) if it doesn't exist
		// accessed like: $.mf.myMFilesMethod()
		if( !$.mf ) {
			$.mf = {};
		}

		function GetShellUI( dashboard )
		{
			if( dashboard == null || dashboard == undefined )
				return null;
	
			if( dashboard.Parent == null || dashboard.Parent == undefined )
				return null;
		
			if( dashboard.Parent.ShellUI != null && dashboard.Parent.ShellUI != undefined )
				return dashboard.Parent.ShellUI;
		
			if( dashboard.Parent.ShellFrame.ShellUI != null && dashboard.Parent.ShellFrame.ShellUI != null )
				return dashboard.Parent.ShellFrame.ShellUI;
		}

		function GetShellFrame( dashboard )
		{
			if( dashboard == null || dashboard == undefined )
				return null;
	
			if( dashboard.Parent == null || dashboard.Parent == undefined )
				return null;
	
			if( dashboard.Parent.ShellFrame != null && dashboard.Parent.ShellFrame != undefined )
				return dashboard.Parent.ShellFrame;
	
			// Check th ShellUI -property. It must exist for ShellFrame objects.
			if( dashboard.Parent.ShellUI != null && dashboard.Parent.ShellUI != undefined )
				return dashboard.Parent;
	
			return null;
		}

		$.extend($.mf, {

			setDashboard: function(dashboard) {
				this.dashboard = dashboard;
				this.shellUI = GetShellUI(dashboard);
				this.shellFrame = GetShellFrame(dashboard);
				this.vault = this.shellUI.Vault;
			}


		});

	
        // get references to original functions
        var each = $.each,
			map = $.map,
			isPlainObject = $.isPlainObject,
			outerWidth = $.fn.outerWidth,
			outerHeight = $.fn.outerHeight;			

        $.extend({
            each: function (obj, callback) {
                if ( typeof(obj) === "object" && obj.constructor === undefined && obj.count !== undefined) {
					
					//assume we have an M-Files (COM Based) collection
					for (var i = 1 ; i <= obj.count; i++) {
						if ( callback.call( obj.Item(i), i, obj.Item(i) ) === false ) {
							break;
						}
					}					
					return obj;
					
                } else {
                    return each.apply(this, arguments);
                }
            },
			

            map: function (elems, callback, arg) {
                if ( typeof(elems) === "object" && elems.constructor === undefined && elems.count !== undefined) {
					var ret = [];
					//assume we have an M-Files (COM Based) collection
					for (var i = 1 ; i <= elems.count; i++) {
						value = callback( elems.Item(i),i, arg);

						if ( value != null ) {
							ret[ ret.length ] = value;
						}
					}				
					return ret.concat.apply( [], ret );;
					
                } else {
                    return map.apply(this, arguments);
                }
            },
			
			//submitted bug report... http://bugs.jquery.com/ticket/12515
			isPlainObject: function(obj) {
				// Make sure M-Files/COMObjects don't return true to this!
				// allows $.extend() deep copy to preserve them 
				if ( !obj || jQuery.type(obj) !== "object" || /*m-files object*/ obj.constructor === undefined) {
					return false;
				}
				
				return isPlainObject.apply(this, arguments);
			},
			
            outerWidth: function (value) {
                if (typeof (value) != "undefined" && value === value * 1.0) {
                    //return workerFunc.apply(this, [false, value]);					
					return this.width( value - (this.outerWidth() - this.width()) );
                } else {
                    return outerWidth.apply(this, arguments);
                }
            },

            outerHeight: function (value) {
                if (typeof (value) != "undefined" && value === value * 1.0) {
                    //return workerFunc.apply(this, [true, value]);
					return this.height( value - (this.outerHeight() - this.height()) );
                } else {
                    return outerHeight.apply(this, arguments);
                }
            },
			
			normalizeToArray: function (arr) {
				
				if( !arr ) {
					return [];
				} else if ( !$.isArray(arr) ) {
					return [arr];
				} else {
					return arr;
				}
			}
        });









    
	
	
		/**
		 * jQuery.fn.sortElements
		 * --------------
		 * @param Function comparator:
		 *   Exactly the same behaviour as [1,2,3].sort(comparator)
		 *   
		 * @param Function getSortable
		 *   A function that should return the element that is
		 *   to be sorted. The comparator will run on the
		 *   current collection, but you may want the actual
		 *   resulting sort to occur on a parent or another
		 *   associated element.
		 *   
		 *   E.g. $('td').sortElements(comparator, function(){
		 *      return this.parentNode; 
		 *   })
		 *   
		 *   The <td>'s parent (<tr>) will be sorted instead
		 *   of the <td> itself.
		 */
		 // Taken from:  http://james.padolsey.com/javascript/sorting-elements-with-jquery/
		$.fn.sortElements = (function(){
	 
			var sort = [].sort;
	 
			return function(comparator, getSortable) {
	 
				getSortable = getSortable || function(){return this;};
	 
				var placements = this.map(function(){
	 
					var sortElement = getSortable.call(this),
						parentNode = sortElement.parentNode,
	 
						// Since the element itself will change position, we have
						// to have some way of storing its original position in
						// the DOM. The easiest way is to have a 'flag' node:
						nextSibling = parentNode.insertBefore(
							document.createTextNode(''),
							sortElement.nextSibling
						);
	 
					return function() {
	 
						if (parentNode === this) {
							throw new Error(
								"对元素排序失败." //You can't sort elements if any one is a descendant of another
							);
						}
	 
						// Insert before flag:
						parentNode.insertBefore(this, nextSibling);
						// Remove flag:
						parentNode.removeChild(nextSibling);
	 
					};
	 
				});
	 
				return sort.call(this, comparator).each(function(i){
					placements[i].call(getSortable.call(this));
				});
	 
			};
	 
		})();	
	
	}

})(jQuery);



Math.coerce = function(v,min,max) { return Math.max(Math.min(v, min, max)); }