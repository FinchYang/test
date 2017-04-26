// Called when a new shell UI is started.
function OnNewShellUI( shellUI ) {

	// Return ShellUI Event Handlers
	return {
        
        OnNewShellFrame: function (shellFrame) {
			// Called when a new shell window is created.

			// Declare variables scoped to the shellFrame.
			var ganttCmd,
				ganttVisible = false,
				customData = {};
	
			function toggleGantt() {

				ganttVisible = !ganttVisible;

				// check if we should show the custom gantt content
				if(!ganttVisible) {

					// no custom content, show default m-files panes

					// show default content in the listing, right and bottom panes
					shellFrame.ShowDefaultContent();
					shellFrame.RightPane.ShowDefaultContent();

					//clear selection
					shellFrame.Listing.UnselectAll();

					// detach dashboard's vault listener if there was one
					if (true || customData.vaultListenerID) {
						shellUI.NotifyVaultEntry("VaultChangeTracker", "detach", customData.vaultListenerID);
						delete customData.vaultListenerID
					}
							
				} else {

					// show our custom gantt content

					// overrirde the listing dashboard
					shellFrame.ShowDashboard("gantt", customData);
							
				}

			}





			// Return ShellFrame Event Handlers
			return {

				OnStarted: function () {			

					// Add command to toggle the chart On or Off
					ganttCmd = shellFrame.Commands.CreateCustomCommand("甘特"); //Gantt
					shellFrame.Commands.SetIconFromPath(ganttCmd, "images/chart_gantt.ico");
					
					// fails on windows without a task pane, like History and Relationships
					try {
						shellFrame.TaskPane.AddCustomCommandToGroup(ganttCmd, TaskPaneGroup_ViewAndModify, 1);
					} catch(e) {}

					// Register handler to listen custom commands.
					shellFrame.Commands.Events.OnCustomCommand = function (command) {

	
						// Toggle the gantt chart if the gantt command was triggered.
						if( command == ganttCmd )
							toggleGantt();
					};
					// Launch the gantt chart right away, if we're just refreshing.
					/*
					var key = "Gantt.Refresh:" + shellFrame.CurrentPath;
					var data = MFiles.RetrieveStringData(key, true);
					if( data )
						toggleGantt();
					*/

				}, 
				
				OnNewShellListing: function (shellListing) {
					// Called when a new shell listing is created.


					// Return new shell listing Event Handlers
					return {
					
						OnStarted: function() {
							// Called when a new shell listing is started.

							// check if there are objects in the view and that we aren't in the root view!
							if( shellListing.Items.ObjectVersions.Count && shellFrame.CurrentPath.indexOf("工期模块")>=0)  {

								// show gantt command button
								shellFrame.Commands.SetCommandState(ganttCmd, CommandLocation_All, CommandState_Active);

							} else {

								// hide gantt command button
								shellFrame.Commands.SetCommandState(ganttCmd, CommandLocation_All, CommandState_Hidden);
							}
						}
						
					}
				}	

			}; // End ShellFrame Event Handlers
    
		}
		
	}; // End ShellUI Event Handlers
}