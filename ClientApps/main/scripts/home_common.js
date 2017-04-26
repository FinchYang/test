
"use strict";
/************************
 * MF Home  Multi-Listing
 *************************/

function setListingContentHeight( container ) {
	/// <summary>Resizes the listing control to fit the remaining height of the pane.</summary>

    var remainingHeight = container.height() - container.find( ".mf-listing-header" ).outerHeight();
    container.find( ".mf-listing-content" ).height( remainingHeight );
}

function createMainListing( shellFrame, loc, container, relativePath ) {
	/// <summary>Creates the main listing window.</summary>

	// Create the main listing window and bind to it.
    container.find( ".mf-listing-header" ).data( "localization", loc );
    var DocListCtrl = createListingControl( MFiles.CLSID.ShellListingCtrl, container.find( ".mf-listing-content" ) );
    var listingMain = shellFrame.Listing;
    if (!listingMain) {
        listingMain = shellFrame.ActiveListing;
    }
    if (relativePath || relativePath === '') {
        listingMain = shellFrame.CreateAdditionalListingForPath(relativePath);
    }
    listingMain.ColumnHeadersVisible = false;
    DocListCtrl.AttachToListingWindow( listingMain );
    initListingHeader( listingMain, container.find( ".mf-listing-header" ) );
    container.data( "listing", listingMain );

	return listingMain;
}

function createAdditionalListingForPath(shellFrame, loc, container, viewPath) {
    /// <summary>Creates an additional listing window in the shell frame.</summary>

    // Create an additional listing and bind to it.
    container.find(".mf-listing-header").data("localization", loc);
    var DocListCtrl = createListingControl(MFiles.CLSID.ShellListingCtrl, container.find(".mf-listing-content"));
    var listing = shellFrame.CreateAdditionalListingForPath(viewPath);
    listing.GroupViewsAndFolders = false;
    listing.GroupObjectsByObjectType = false;
    DocListCtrl.AttachToListingWindow(listing);
    initListingHeader(listing, container.find(".mf-listing-header"));
    container.data("listing", listing);

    return listing;
}

function createAdditionalListing( shellFrame, loc, container, viewID ) {
	/// <summary>Creates an additional listing window in the shell frame.</summary>

    // Create an additional listing and bind to it.
    container.find( ".mf-listing-header" ).data( "localization", loc );
    var DocListCtrl = createListingControl( MFiles.CLSID.ShellListingCtrl, container.find( ".mf-listing-content" ) );
    var listing = shellFrame.CreateAdditionalListingForView( viewID );
    listing.GroupViewsAndFolders = false;
    listing.GroupObjectsByObjectType = false;
    DocListCtrl.AttachToListingWindow( listing );
    initListingHeader( listing, container.find( ".mf-listing-header" ) );
    container.data( "listing", listing );

    return listing;
}

function createListingControl( clsid, container ) {
	/// <summary>Creates a new listing control.</summary>

	// Check arguments.
    if( container.length != 1 )
        return;

	// Add the ActiveX control to the DOM.
    container.html("<object classid='clsid:" + clsid + "' style='width: 100%; height: 100%;'> </object>");
    // TODO: Remove quotes from the CLSID for safety.
    var obj = container.find( "object" ).get( 0 );

    return obj;
}

function initListingHeader( listing, header ) {
	/// <summary>Initializes the specifies listing header.</summary>

	// Add the content elements to the header.
	header.append(
			'<div class="mf-listing-header-collapsed-state">' +
				'<div class="mf-listing-header-expanded"></div>' +
				'<div class="mf-listing-header-collapsed"></div>' +
			'</div>' +
			'<div class="mf-listing-header-text"></div>' );

	{
		// Register the event handler for content changes.
		var handler = getContentChangedHandler( listing, header );
		listing.Events.Register( MFiles.Event.ContentChanged, handler );

		// Call the handler once to initialize the header.
		handler( listing.Items );
	}

	{
		// Register the event handler for listing activation.
		var handler = getListingActivatedHandler( listing, header );
		listing.Events.Register( MFiles.Event.ListingActivated, handler );
	}

	{
		// Register the event handler for listing deactivation.
		var handler2 = getListingDeactivatedHandler( listing, header );
		listing.Events.Register( MFiles.Event.ListingDeactivated, handler2 );
	}

	// Update header state initially.
	updateHeaderState( listing, header );

	// Attach click handler.
    header.click( function() {
	
		// Activate this listing window.
		listing.ActivateListing();

		// Toggle visibility.
		$( this ).next().toggle();
		updateHeaderState( listing, header );
		resized();

	} );
}

function updateHeaderState( listing, header ) {
	/// <summary>Updates the active/inactive state of the listing header.</summary>

	// Update the active/inactive state.
	if( listing && listing.IsActive ) {

		// Make this header the only active header.
		$( ".mf-listing-header" ).removeClass( "mf-listing-active" );
		header.addClass( "mf-listing-active" );
	}
	else {

		// Remove the active state from this header.
		header.removeClass( "mf-listing-active" );
	}

	// Update the visibility of the expand/collapse buttons.
	if( header.next().is( ":visible" ) ) {

		// The listing is in expanded state => show the collapse button.
		header.find( ".mf-listing-header-collapsed-state .mf-listing-header-expanded" ).show();
		header.find( ".mf-listing-header-collapsed-state .mf-listing-header-collapsed" ).hide();
	}
	else {

		// The listing is in collapsed state => show the expand button.
		header.find( ".mf-listing-header-collapsed-state .mf-listing-header-expanded" ).hide();
		header.find( ".mf-listing-header-collapsed-state .mf-listing-header-collapsed" ).show();
	}
}

function getContentChangedHandler( listing, header ) {
	/// <summary>Returns the handler for the ContentChanged event.</summary>

    return function( items ) {

		var loc = header.data( "localization" );

		// Update the header text.
		var title = listing.CurrentPath;
		if( title == "" )
			title = loc.strings.IDS_HOMESCREEN_TITLE_VIEWS;
		else
			title += " (" + items.GetObjectVersionsCount() + ")";
		header.find( ".mf-listing-header-text" ).text( title );
    }
}

function getListingActivatedHandler( listing, header ) {

	return function( previousActiveListing ) {

		// Update the header's state.
		updateHeaderState( listing, header );
	}
}

function getListingDeactivatedHandler( listing, header ) {

	return function( newActiveListing ) {

		// Update the header's state.
		updateHeaderState( listing, header );
	}
}

function openNewBrowserWindow( url ) {

	try {

		// Open a new browser window.
		// We use the WScript.Shell method to ensure that the user's default browser is used.
		var oShell = new ActiveXObject( "WScript.Shell" );
		oShell.Run( url );

	} catch( err ) {

		// Ignore errors.
	}
}

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
$.fn.sortElements = ( function() {

	var sort = [].sort;

	return function( comparator, getSortable ) {

		getSortable = getSortable || function() { return this; };

		var placements = this.map( function() {

			var sortElement = getSortable.call( this );
			var parentNode = sortElement.parentNode;

			// Since the element itself will change position, we have
			// to have some way of storing its original position in
			// the DOM. The easiest way is to have a 'flag' node:
			var nextSibling = parentNode.insertBefore( 
								document.createTextNode( '' ),
								sortElement.nextSibling );

			return function() {

				if( parentNode === this ) {
					throw new Error( "You can't sort elements if any one is a descendant of another." );
				}

				// Insert before flag:
				parentNode.insertBefore( this, nextSibling );

				// Remove flag:
				parentNode.removeChild( nextSibling );
			};

		} );

		return sort.call( this, comparator ).each( function( i ) {
			placements[i].call( getSortable.call( this ) );
		} );
	};

} )();
