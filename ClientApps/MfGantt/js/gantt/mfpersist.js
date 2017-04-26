// anonymous closure encapsulating jQuery shorthand
( function( $, undefined ) {

	// declare static method/property namespace for M-Files (mf) if it doesn't exist
	// accessed like: $.mf.myMFilesMethod()
	if( !$.mf ) {
		$.mf = {};
	}

	$.extend( $.mf, {

		settings: function( name, type, data ) {
			/// <summary>Stores/Retreives settings from NamedValueStorage. Assumes 'this.vault' has been set (i.e., $.mf.vault).</summary>
			/// <param name="name" type="string">
			///	 The settings namespace in dot-notation. Corresponds to MFNamedValue Namespace like so: "my.namespace.key"
			///  User settings will have the user id auto-appended to the namespace.
			/// </param>
			/// <param name="type" type="MFiles.MFNamedValueType">User Setting:4, Common View Setting:6)</param>
			/// <param name="data" type="object">Optional</param>
			/// <returns type="bool or object">
			///	 bool: success, if 'data' paramter is provided (storage operation)
			///  object: data, if 'data' paramter is omitted (retrieval operation)
			/// </returns>

			var lastDot = name.lastIndexOf( "." ),
				ns = name.substr( 0, lastDot ),
				key = name.substr( lastDot + 1 ),
				values;

			// append current user's id to namespace if this is a user setting			
			if( type === 4 ) {
				ns += "." + this.vault.SessionInfo.UserID;
			}

			if( data ) {

				// serialize and save the settings data
				values = MFiles.CreateInstance( "NamedValues" );
				values.Value( key ) = JSON.stringify( data, null );
				try {
					this.vault.NamedValueStorageOperations.SetNamedValues( type, ns, values );
					return true;
				} catch( e ) {
					alert( e );
					return false;
				}

			} else {

				// retrieve and unserialize the settings data
				values = this.vault.NamedValueStorageOperations.GetNamedValues( type, ns );
				if( values.Value( key ) ) {
					return JSON.parse( values.Value( key ) );
				} else {
					return {};
				}
			}
		}
	} );


} )( jQuery );