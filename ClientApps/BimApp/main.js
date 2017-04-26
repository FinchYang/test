
function OnNewShellUI( shellUI ) {
	/// <summary>The entry point of ShellUI module.</summary>
	/// <param name="shellUI" type="MFiles.ShellUI">The new shell UI object.</param> 
	shellUI.Events.Register(MFiles.Event.NewShellFrame, newShellFrameHandler());
}

function newShellFrameHandler() {
	/// <summary>Event handler to handle new shell frame object creations.</summary>
    /// <param name="shellFrame" type="MFiles.ShellFrame">The new shell frame object.</param>   

    return function(shellFrame) {
        // Register to listen the started event.        
        shellFrame.Events.Register(MFiles.Event.Started, getShellFrameStartedHandler(shellFrame));
    }
}

function getShellFrameStartedHandler(shellFrame) {
	/// <summary>Gets the event handler for "started" event of a shell frame.</summary>
	/// <param name="shellFrame" type="MFiles.ShellFrame">The current shell frame object.</param>
	/// <returns type="MFiles.Event.OnStarted">The event handler object</returns>
    
	// Return the handler function for Started event.
	return function() {
		/// <summary>The "started" event handler implementation for a shell frame.</summary>
	    // Shell frame object is now started. Check if this is the root view.
		bim.addBimCmd(shellFrame);

        bim.createBIMCmds(shellFrame);
		
	}	
}