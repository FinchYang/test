///////// localization
function localization() {

	// getDateFormat.
	this.getDateFormat = function() {
	
		// Get short date format (in Windows-specific format).
		var dateFormat = this.getCalendarInfo( "DateFormat" );
		
		// Do mapping from Windows date format to jQuery UI datapicker format.
		//
		// Windows format returned by getText function consists of:
		//
		// d     Day of the month as digits without leading zeros for single-digit days.
		// dd    Day of the month as digits with leading zeros for single-digit days.
		// ddd   Abbreviated day of the week.
		// dddd  Day of the week.
		//
		// M     Month as digits without leading zeros for single-digit months.
		// MM    Month as digits with leading zeros for single-digit months.
		// MMM   Abbreviated month.
		// MMMM  Month.
		//
		// y     Year represented only by the last digit.
		// yy    Year represented only by the last two digits. A leading zero is added for single-digit years.
		// yyyy  Year represented by a full four or five digits, depending on the calendar used.
		// yyyyy Behaves identically to "yyyy".
		//
		// jQuery Datepicker format consists of:
		//
		// d - day of month (no leading zero)
		// dd - day of month (two digit)
		// o - day of the year (no leading zeros)
		// oo - day of the year (three digit)
		// D - day name short
		// DD - day name long
		// m - month of year (no leading zero)
		// mm - month of year (two digit)
		// M - month name short
		// MM - month name long
		// y - year (two digit)
		// yy - year (four digit) 
		
		
		// Convert day.
		if ( dateFormat.indexOf("dddd") != -1 )
			dateFormat = dateFormat.replace("dddd", "DD");
		else if ( dateFormat.indexOf("ddd") != -1 )
			dateFormat = dateFormat.replace("ddd", "D");
		
		// Convert month.
		if ( dateFormat.indexOf("MMMM") != -1 )
			dateFormat = dateFormat.replace("MMMM", "MM");
		else if ( dateFormat.indexOf("MMM") != -1 )
			dateFormat = dateFormat.replace("MMM", "M");
		else if ( dateFormat.indexOf("MM") != -1 )
			dateFormat = dateFormat.replace("MM", "mm");
		else if ( dateFormat.indexOf("M") != -1 )
			dateFormat = dateFormat.replace("M", "m");			

		// Convert year.
		if ( dateFormat.indexOf("yyyyy") != -1 )
			dateFormat = dateFormat.replace("yyyyy", "yy");
		else if ( dateFormat.indexOf("yyyy") != -1 )
			dateFormat = dateFormat.replace("yyyy", "yy");
		else if ( dateFormat.indexOf("yy") != -1 )
			dateFormat = dateFormat.replace("yy", "y");			
				
		// Return converted dataFormat.
		return dateFormat;
	};
	
	// getTimeInfo.
	this.getTimeInfo = function() {
	
		// Get time format (in Windows-specific format).
		var timeFormat = this.getCalendarInfo( "TimeFormat" );
		
		// Check if we should use 12-hour or 24-hour clock.
		var use24Hours = false;
		if ( timeFormat.indexOf("H") != -1 )
			use24Hours = true;
			
		// Return time format info used for date control.	
		return {
			show24Hours: use24Hours,
			showSeconds: true,
			separator: ":",
			ampmPrefix: ' ',
			ampmNames: ['AM', 'PM']
		};
	};
	
	// getDayNamesMin.
	// This function returns truncated weekday names, starting from the sunday. 
	this.getDayNamesMin = function() {
	
		var dayNamesMin = [];
		for ( var i = 0; i < 7; i++ ) {

			var index = ( i == 0 ) ? 7 : i;
		
			// This call to getCalendarInfo returns truncated weekday name, 1 = monday, 7 = sunday.
			var dayNameMin = this.getCalendarInfo( "DayNameMin-" + index );
			dayNamesMin.push( dayNameMin );
		}	
		return dayNamesMin;
	};
	
	// getDayNames.
	// This function returns weekday names, starting from the sunday.
	this.getDayNames = function() {
	
		var dayNames = [];
		for ( var i = 0; i < 7; i++ ) {
		
			var index = ( i == 0 ) ? 7 : i;
		
			// This call to getCalendarInfo returns weekday name, 1 = monday, 7 = sunday.
			var dayName = this.getCalendarInfo( "DayName-" + index );
			dayNames.push( dayName );
		}	
		return dayNames;
	};
	
	// getMonthNamesMin.
	this.getMonthNamesMin = function() {
	
		var monthNamesMin = [];
		for ( var i = 0; i < 12; i++ ) {
		
			var monthNameMin = this.getCalendarInfo( "MonthNameMin-" + ( i + 1 ) );
			monthNamesMin.push( monthNameMin );
		}	
		return monthNamesMin;
	};
	
	// getMonthNames.
	this.getMonthNames = function() {
	
		var monthNames = [];
		for ( var i = 0; i < 12; i++ ) {
		
			var monthName = this.getCalendarInfo( "MonthName-" + ( i + 1 ) );
			monthNames.push( monthName );
		}	
		return monthNames;
	};
		
    // getCalendarInfo.
	this.getCalendarInfo = function( id ) {
	
	    // Get localized calendar info from MFShell.
		var localizedString = null;
		try {
		    localizedString = MFiles.GetCalendarInfo( id );
		}
		catch ( ex ) {
		}
		return ( localizedString != null ) ? localizedString : "";
	};
		
	// getText.
	this.getText = function( id ) {
	
		// Get localized text from MFShell.
		var localizedString = null;
		try {
			localizedString = MFiles.GetStringResource( id );
		}
		catch ( ex ) {
		}
		return ( localizedString != null ) ? localizedString : "";
	};

	this.replacePlaceholder = function( message, value ) {

		// Make a simple replacement.
		var i = message.indexOf( "%s" );
		var text = message.substr( 0, i ) + value + message.substr( i + 2 );

		return text;
	}
     
    // Map resource strings to the IDs in MFRes.dll.   
	this.strings = {};
	this.strings.IDS_STRING_YES = this.getText( 16225 );
	this.strings.IDS_STRING_NO = this.getText( 16226 );
	this.strings.IDS_MSG_VALUE_X_NOT_IN_LIST_BOX_DO_YOU_WANT_TO_ADD_NEW_VALUE = this.getText( 27077 );
	this.strings.IDS_MSG_MCOMBO_TEXT_NOT_INLISTBOX = this.getText( 28050 );
	this.strings.IDS_STRING_YES = this.getText( 16225 );
	this.strings.IDS_STRING_NO = this.getText( 16226 );
	this.strings.IDS_CONTROLHELPER_BTNTITLE_PREVIOUS = this.getText( 28067 );
	this.strings.IDS_CONTROLHELPER_BTNTITLE_NEXT = this.getText( 28068 );
	this.strings.IDS_STR_PROPERTYDEF_WORKFLOW = this.getText( 12066 );
	this.strings.IDS_STR_PROPERTYDEF_STATE = this.getText( 12067 );
	this.strings.IDS_CONTROLHELPER_DROPDOWN_SEARCHING_INDICATOR = this.getText( 28069 );
	this.strings.IDS_CONTROLHELPER_DROPDOWN_FIRST_X_VALUES = this.getText( 28074 );
	this.strings.IDS_CONTROLHELPER_DROPDOWN_SEARCHING_FIRST_X_SUGGESTIONS = this.getText( 28070 );
	this.strings.IDS_CONTROLHELPER_DROPDOWN_NO_MATCHES_FOUND = this.getText( 28073 );
	this.strings.IDS_CONTROLHELPER_ADD_VALUE_COMMAND_TEXT = this.getText( 28066 );
	this.strings.IDS_STR_SELECT_TEMPLATE = this.getText( 26378 );
	this.strings.IDS_STR_TYPE_SEARCH_WORDS = this.getText( 26379 );
	this.strings.IDS_STR_GROUPNAME_ALL = this.getText( 26380 );
	this.strings.IDS_STR_METADATACARD_ADD_PROPERTY_ACTION_LABEL = this.getText( 27600 );
    this.strings.IDS_METADATACARD_CONTENT_VARIES_TEXT = this.getText( 27604 );
    this.strings.IDS_METADATACARD_TOOLBAR_TOOLTIP_ADDFIELD = this.getText( 27605 );
    this.strings.IDS_METADATACARD_TOOLBAR_TOOLTIP_REMOVEFIELD = this.getText( 27606 );
    this.strings.IDS_METADATACARD_TOOLBAR_TOOLTIP_REFRESH = this.getText( 27607 );
    this.strings.IDS_METADATACARD_TOOLBAR_TOOLTIP_ADDVALUE = this.getText( 27608 );
    this.strings.IDS_METADATACARD_TOOLBAR_TOOLTIP_EDIT = this.getText( 27609 );
    this.strings.IDS_METADATACARD_LABEL_PERMISSIONS = this.getText( 27610 );
    this.strings.IDS_METADATACARD_BUTTON_COMMENTS = this.getText( 27611 );
    this.strings.IDS_METADATACARD_BUTTON_COMMENTS_X = this.getText( 27612 );
    this.strings.IDS_METADATACARD_BUTTON_PROPERTIES = this.getText( 27613 );
    this.strings.IDS_METADATACARD_BUTTON_DISCARD = this.getText( 27614 );
    this.strings.IDS_METADATACARD_BUTTON_CLOSE = this.getText( 27615 );
    this.strings.IDS_METADATACARD_LABEL_SOURCEFILE = this.getText( 27616 );
    this.strings.IDS_METADATACARD_LABEL_SOURCEFILES = this.getText( 27617 );
    this.strings.IDS_METADATACARD_LABEL_PERMISSIONS_X = this.getText( 27618 );
    this.strings.IDS_METADATACARD_BUTTON_SHOWPREVIEW = this.getText( 27619 );
    this.strings.IDS_METADATACARD_BUTTON_HIDEPREVIEW = this.getText( 27620 );
    this.strings.IDS_METADATACARD_COMMAND_POPOUT = this.getText( 27583 );
    this.strings.IDS_METADATACARD_COMMAND_SAVE = this.getText( 27593 );
    this.strings.IDS_METADATACARD_COMMAND_CREATE = this.getText( 27584 );
    this.strings.IDS_METADATACARD_COMMAND_SKIPTHIS = this.getText( 27586 );
    this.strings.IDS_METADATACARD_COMMAND_CREATEALL = this.getText( 27587 );
    this.strings.IDS_METADATACARD_COMMAND_CANCEL = this.getText( 27585 );
	this.strings.IDS_METADATACARD_ASSIGNEDTO_BUTTON_MARKCOMPLETE_TOOLTIP = this.getText( 27621 );
	this.strings.IDS_HOMESCREEN_TITLE_CREATE_NEW = this.getText( 27622 );
	this.strings.IDS_HOMESCREEN_TITLE_LEARNING_RESOURCES = this.getText( 27623 );
	this.strings.IDS_HOMESCREEN_SUBTITLE_LEARN_MORE = this.getText( 27624 );
	this.strings.IDS_HOMESCREEN_SUBTITLE_GET_HELP = this.getText( 27625 );
	this.strings.IDS_HOMESCREEN_ITEM_OVERVIEW_VIDEO = this.getText( 27626 );
	this.strings.IDS_HOMESCREEN_ITEM_GUIDED_TOUR = this.getText( 27627 );
	this.strings.IDS_HOMESCREEN_ITEM_TRAINING = this.getText( 27628 );
	this.strings.IDS_HOMESCREEN_ITEM_USER_GUIDE = this.getText( 27629 );
	this.strings.IDS_HOMESCREEN_ITEM_TECHNICAL_DOCUMENTATION = this.getText( 27630 );
	this.strings.IDS_HOMESCREEN_ITEM_COMMUNITY = this.getText( 27631 );
	this.strings.IDS_HOMESCREEN_ITEM_CONTACT_SUPPORT = this.getText( 27632 );
	this.strings.IDS_HOMESCREEN_TITLE_VIEWS = this.getText( 27633 );
    this.strings.IDS_METADATACARD_MULTILINEPROP_LINKTEXT_MORE = this.getText( 27634 );
    this.strings.IDS_METADATACARD_MULTILINEPROP_LINKTEXT_LESS = this.getText( 27635 );
    this.strings.IDS_METADATACARD_ERRORINFO_LINKTEXT_DETAILS = this.getText( 27636 );
	this.strings.IDS_HOMESCREEN_NEW_DOCUMENT_FROM_SCANNER = this.getText( 27637 );
    this.strings.IDS_METADATACARD_AUTOMATIC_VALUE = this.getText( 27648 );
	this.strings.IDS_METADATACARD_LABEL_SHOW_ALL = this.getText( 27650 );
	this.strings.IDS_METADATACARD_LABEL_HIDE = this.getText( 27651 );
	this.strings.IDS_METADATACARD_LABEL_WRITE_COMMENT = this.getText( 27652 );
	this.strings.IDS_METADATACARD_LABEL_REMOVE_PROPERTY = this.getText( 27653 );
	this.strings.IDS_METADATACARD_BUTTON_SETTINGSMENU = this.getText( 27654 );
	this.strings.IDS_METADATACARD_BUTTON_EXPAND_TITLE = this.getText( 27655 );
	this.strings.IDS_METADATACARD_BUTTON_COLLAPSE_TITLE = this.getText( 27656 );
	this.strings.IDS_METADATACARD_ITEM_TOGGLE_LOCATION = this.getText( 27657 );
	this.strings.IDS_METADATACARD_ITEM_POPOUT = this.getText( 27658 );
	this.strings.IDS_METADATACARD_ITEM_HELP = this.getText( 27659 );
	this.strings.IDS_METADATACARD_LABEL_ONE_FILE = this.getText( 27660 );
	this.strings.IDS_METADATACARD_LABEL_N_FILES = this.getText( 27661 );
    this.strings.IDS_METADATACARD_BUTTON_MINIMIZE = this.getText( 27662 );
    this.strings.IDS_METADATACARD_BUTTON_RESTORE = this.getText( 27663 );
    this.strings.IDS_HOMESCREEN_TABNAME = this.getText( 27664 );  // This string id (27664) is used directly in the home.js so don't change it. Or if it is changed then change also the corresponding value in home.js.
	this.strings.IDS_HOMESCREEN_NEW_VIEW = this.getText( 27665 );
	this.strings.IDS_METADATACARD_PERMISSIONS_CHANGED = this.getText( 26908 );
	this.strings.IDS_METADATACARD_CLASS_X = this.getText( 27666 );
	this.strings.IDS_LINK_SHOW_OBJECT_IN_VAULT_X = this.getText( 15195 );
	this.strings.IDS_LINK_SHOW_OBJECT_IN_ANOTHER_VAULT = this.getText( 15118 );
	this.strings.IDS_METADATACARD_BUTTON_TODAY = this.getText( 27667 );
	this.strings.IDS_TEMPLATESELECTOR_SELECT_CLASS = this.getText( 27669 );
	this.strings.IDS_TEMPLATESELECTOR_GROUPNAME_RECENTLY_USED_TEMPLATES = this.getText(	27670 );
	this.strings.IDS_TEMPLATESELECTOR_GROUPNAME_ALL_TEMPLATES = this.getText( 27671 );
	this.strings.IDS_TEMPLATESELECTOR_GROUPNAME_BLANK_TEMPLATES = this.getText( 27672 );
	this.strings.IDS_TEMPLATESELECTOR_GROUPTITLE_RECENTLY_USED_TEMPLATES = this.getText( 27673 );
	this.strings.IDS_TEMPLATESELECTOR_GROUPTITLE_BLANK_TEMPLATES = this.getText( 27674 );
	this.strings.IDS_TEMPLATESELECTOR_GROUPTITLE_TEMPLATES = this.getText( 27675 );
	this.strings.IDS_TEMPLATESELECTOR_GROUPTITLE_ALL_TEMPLATES = this.getText( 27676 );
	this.strings.IDS_TEMPLATESELECTOR_GROUPTITLE_DEFAULT_TEMPLATE = this.getText( 27677 );
	this.strings.IDS_TEMPLATESELECTOR_GROUPTITLE_OTHER_TEMPLATES = this.getText( 27678 );
	this.strings.IDS_TEMPLATESELECTOR_TIP_NO_TEMPLATES_FOR_THIS_CLASS = this.getText( 27679 );
	this.strings.IDS_TEMPLATESELECTOR_TIP_NO_TEMPLATES_MATCHING_SEARCH = this.getText( 27680 );
	this.strings.IDS_TEMPLATESELECTOR_TIP_NO_RECENTLY_USED_TEMPLATES = this.getText( 27681 );
	this.strings.IDS_TEMPLATESELECTOR_TIP_SELECT_CLASS_TO_VIEW_TEMPLATES = this.getText( 27682 );
};
