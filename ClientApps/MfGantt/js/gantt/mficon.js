// anonymous closure encapsulating jQuery shorthand
( function( $, undefined ) {

	// declare static method/property namespace for M-Files (mf) if it doesn't exist
	// accessed like: $.mf.myMFilesMethod()
	if( !$.mf ) {
		$.mf = {};
	}

	// "closure global" to cache path to loaded icons
	var iconCache = {};

	$.extend( $.mf, {

		icon: function( valueList, valueListItem ) {
			/// <signature>
			///     <summary>
			///      Resolves the icon for an ObjectType, saves it locally, and returns the path.
			///      Assumes 'this.vault' has been set (i.e., $.mf.vault).
			///     </summary>
			///     <param name="objectTypeID" type="Number"></param>
			///     <returns type="string">Icon Path</returns>
			/// </signature>
			/// <signature>
			///     <summary>
			///      Resolves the icon for a ValueListItem, saves it locally, and returns the path.
			///      Assumes 'this.vault' has been set (i.e., $.mf.vault).
			///     </summary>
			///     <param name="valueList" type="Number"></param>
			///     <param name="valueListItem" type="Number"></param>
			///     <returns type="string">Icon Path</returns>
			/// </signature>


			var key = ( valueListItem === undefined ) ? "0-" + valueList : valueList + "-" + valueListItem,
                path,
				fs,
				icon,
				stream,
				adBinaryMode = 1,
				adCreateOverwriteMode = 2;

			//check cache
			path = iconCache[key];

			// found something in the cache! return it and be done
			if( path !== undefined ) {
				return path;
			}

			// Start up the file sytem object and build our path
			fs = new ActiveXObject( 'Scripting.FileSystemObject' );
			path = fs.GetSpecialFolder( 2 );

			// Create ui ext icon folder
			path += "\\M-Files_UIEXT_Icons";
			if( !fs.FolderExists( path ) ) {
				fs.CreateFolder( path );
			}

			// create a folder for each vault
			path += "\\" + $.mf.vault.getGUID();
			if( !fs.FolderExists( path ) ) {
				fs.CreateFolder( path );
			}

			// add file to path
			path = path += "\\" + key + ".ico";

			// if file already exists, don't load it...
			if( fs.FileExists( path ) ) {
				return path;
			}


			// just in case something goes wrong (like the item doesn't have an icon)
			try {

				// load the image
				if( valueListItem === undefined ) {

					// ObjectType icon
					icon = $.mf.vault.ObjectTypeOperations.GetObjectType( valueList ).Icon;

				} else {

					//ValueList icon
					icon = $.mf.vault.ValueListItemOperations.GetValueListItemByID( valueList, valueListItem ).Icon;
				}

				// save icon data to file
				var stream = new ActiveXObject( "ADODB.Stream" );
				stream.Type = adBinaryMode;
				stream.Open();
				stream.Write( icon );
				stream.SaveToFile( path, adCreateOverwriteMode );

				// save to cache
				iconCache[key] = path;

				// return the local path to the icon
				return path;

			} catch( e ) {

				// cache that we were unable to generate an icon
				iconCache[key] = "";
			}

		}

	} );

} )( jQuery );