// anonymous closure encapsulating jQuery shorthand
(function ($, undefined) {

	// declare static method/property namespace for M-Files (mf) if it doesn't exist
	// accessed like: $.mf.myMFilesMethod()
	if(!$.mf) {
		$.mf = {};
	}
	
	// shortcut lookup for the MFDataType of each  MFStatusType
	// for example: MFDataType = statusDataTypes[MFStatusType]
	var statusDataTypes = [8,9,7,9,2,8,9,8,1,1];
	
	$.extend($.mf, {
		
		search: function(conditions) {
		    /// <summary>Assumes 'this.vault' has been set (i.e., $.mf.vault).</summary>
			/// <param name="name" type="object"></param>
			/// <returns type="MFilesAPI.ObjectSearchResults"></returns>
						
			var search = MFiles.CreateInstance("SearchConditions");
			
			$.each(conditions, function(p,v) {
				//try {
					var c = MFiles.CreateInstance("SearchCondition"),
						dataType, lookups;
					c.conditionType = 1;
					
					if( v === undefined || v === null || ($.isArray(v) && v.length === 0 ) ) {
						return;
					} else if( p.indexOf("s") === 0 && $.isNumeric(p.substr(1)) ) {
						p = parseInt(p.substr(1));
						c.Expression.SetStatusValueExpression(p, MFiles.CreateInstance("DataFunctionCall"));
						dataType = statusDataTypes[p];
					} else if( $.isNumeric(p) ) {
						p = parseInt(p);
						c.Expression.DataPropertyValuePropertyDef = p;
						dataType = $.mf.vault.PropertyDefOperations.GetPropertyDef(p).DataType;
					}else{
						alert("搜索条件不正确: " + p + ": " + v); //failed to process condition:
						return;
					}
							
					switch(dataType) {
					
					case 9:
					case 10: //multi lookup
						if( $.isArray(v) ) {
							if( v.length === 1) {
								v = v.shift();
							} else if( v.length > 1) {
								dataType = 10;
								lookups = MFiles.CreateInstance("Lookups");
								$.each(v, function(i,v) {
									var lookup = MFiles.CreateInstance("Lookup");
									lookup.Item = v;
									lookups.Add(-1, lookup);
								});
								//alert("Setting Multi-Select Lookup for property: " + p);
								c.TypedValue.SetValueToMultiSelectLookup(lookups);
								break;
							}
						}
						//fall through!
					
					default:
						if(v === undefined || v === null) {
							//alert("Setting " + p + " (" + dataType + ") to null!");
							c.TypedValue.SetValueToNull(dataType);
						} else {
							//alert("Setting " + p + " (" + dataType + ") to " + v);
							c.TypedValue.SetValue(dataType, v);
						}
					
					}
					
					search.Add(-1, c);
				/*
				} catch(e) {
					alert("failed to process condition: " + p + ": " + v + " (" + dataType + ")");
				}
				*/
				
					
			});
			
			return $.mf.vault.ObjectSearchOperations.SearchForObjectsByConditions(search, 0, false);
			
		}
		
	});


})(jQuery);