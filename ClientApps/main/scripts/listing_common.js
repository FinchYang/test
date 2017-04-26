
/*
 * require home_common.js
*/


function initializeLayout(dashboard, shellFrame) {

    // Apply right-to-left layout if necessary.
    if (dashboard.UseRightToLeftLayout) {

        // Apply right-to-left layout.
        $("html").attr("dir", "rtl");
        $("html").addClass("mf-rtl");

    } else {

        // Apply left-to-right layout.
        $("html").attr("dir", "ltr");
        $("html").addClass("mf-ltr");
    }

    // Apply compact layout if necessary.
    if (shellFrame.UseCompactLayout) {

        // Apply compact layout.
        $(".mf-listing-header").addClass("mf-compact-layout");
        $(".mf-listing-header-text").addClass("mf-compact-layout");
    }
}

function createListingForPath(shellFrame, loc, container, viewPath, postProcessing) {

    try {

        // Create the listing and bind to it.
        var listing = createAdditionalListingForPath(shellFrame, loc, container, viewPath);
        var objectsCount = listing.Items.GetObjectVersionsCount();

        // Set the first column to auto-fit.
        listing.AutoFitFirstColumn = true;
        listing.ColumnHeadersVisible = true;
        // Perform post-processing.
        postProcessing(container, objectsCount);
    }
    catch (err) {

        // Hide this listing.
        container.data("disabled", true);
        container.hide();
    }

    // Update the header of the listing.
    updateHeaderState(listing, container.find(".mf-listing-header"));
    return listing;
}

function createListing(shellFrame, loc, container, viewID, postProcessing) {

    try {

        // Create the listing and bind to it.
        var listing = createAdditionalListing(shellFrame, loc, container, viewID);
        var objectsCount = listing.Items.GetObjectVersionsCount();

        // Set the first column to auto-fit.
        listing.AutoFitFirstColumn = true;
        listing.ColumnHeadersVisible = true;

        // Perform post-processing.
        postProcessing(container, objectsCount);
    }
    catch (err) {

        // Hide this listing.
        container.data("disabled", true);
        container.hide();
    }

    // Update the header of the listing.
    updateHeaderState(listing, container.find(".mf-listing-header"));
    return listing;
}

function getActiveListingChangedHandler(shellFrame) {
    /// <summary>Called when the active listing changes.</summary>

    return function (previousActiveListing, newActiveListing) {

        // Currently nothing to do here.
    }
}

function resizedWithHeight(height) {
    return function () {

        if (height) {
            var windowHeight = window.innerHeight;
            var fullHeight = 100 * (1 - height / windowHeight);
            $("#fullareadiv").height(fullHeight + "%");
        }

        var container = $("#listings.mf-panel-container");
        var totalHeight = container.height();


        // A panel may be completely invisible, or might show its header only.
        // The remaining height is divided between panels with visible content
        // in proportion of their "height numbers".

        // Calculate the total of the "height numbers" of panels with visible content,
        // as well as the total height of the headers of all visible panels.
        var headerHeightTotal = 0;
        var visibleHeightNumbers = 0;
        var visiblePanels = 0;
        container.find(".mf-panel:visible").each(function () {

            // Add to "height numbers total" if the content of this panel is visible.
            if ($(this).find(".mf-listing-content:visible").length >= 1)
                visibleHeightNumbers += $(this).data("heightNumber");

            // Always add to header height total;
            headerHeightTotal += $(this).find(".mf-listing-header").outerHeight();

            // Count the number of visible panels (i.e., at least header is visible).
            visiblePanels++;

        });

        // Resize visible panels.
        var remainingHeight = totalHeight;
        var totalHeightForPanelContent = totalHeight - headerHeightTotal;
        var panelIndex = 0;
        var lastPanelWithVisibleContent = null;
        container.find(".mf-panel:visible").each(function () {

            // Use existing header height.
            var headerHeight = $(this).find(".mf-listing-header").outerHeight();

            // Calculate the panel height (header + content).
            var panelHeight = 0;
            if ($(this).find(".mf-listing-content:visible").length >= 1) {

                // The panel content is visible.

                // Treat the last visible panel in a different way.
                if (panelIndex == visiblePanels - 1) {

                    // Last visible panel takes up all remaining space.
                    panelHeight = remainingHeight;
                } else {

                    // Calculate the appropriate panel height.
                    var heightNumber = $(this).data("heightNumber");
                    panelHeight = headerHeight + parseInt((heightNumber / visibleHeightNumbers) * totalHeightForPanelContent);
                }

                // Remember the last panel with visible content.
                lastPanelWithVisibleContent = $(this);
            } else {

                // Only the header is visible.
                panelHeight = headerHeight;
            }

            // Never exceed remaining height.
            if (panelHeight > remainingHeight)
                panelHeight = remainingHeight;

            // Resize the panel.
            $(this).height(panelHeight);
            remainingHeight -= panelHeight;

            // Resize content.
            setListingContentHeight($(this));

            // Update panel index.
            panelIndex++;

        });

        // If there is still some remaining height left, make the last panel with visible content taller.
        if (remainingHeight > 0 && lastPanelWithVisibleContent != null) {

            // Make the last panel with visible content taller.
            var newHeight = lastPanelWithVisibleContent.height() + remainingHeight;
            lastPanelWithVisibleContent.height(newHeight);
            setListingContentHeight(lastPanelWithVisibleContent);
        }

    };
}


function resized() {
    /// <summary>Resize panes after the window has been resized.</summary>
    //var height = $("#newTask").height();

    var container = $("#listings.mf-panel-container");
    var totalHeight = container.height();


    // A panel may be completely invisible, or might show its header only.
    // The remaining height is divided between panels with visible content
    // in proportion of their "height numbers".

    // Calculate the total of the "height numbers" of panels with visible content,
    // as well as the total height of the headers of all visible panels.
    var headerHeightTotal = 0;
    var visibleHeightNumbers = 0;
    var visiblePanels = 0;
    container.find(".mf-panel:visible").each(function () {

        // Add to "height numbers total" if the content of this panel is visible.
        if ($(this).find(".mf-listing-content:visible").length >= 1)
            visibleHeightNumbers += $(this).data("heightNumber");

        // Always add to header height total;
        headerHeightTotal += $(this).find(".mf-listing-header").outerHeight();

        // Count the number of visible panels (i.e., at least header is visible).
        visiblePanels++;

    });

    // Resize visible panels.
    var remainingHeight = totalHeight;
    var totalHeightForPanelContent = totalHeight - headerHeightTotal;
    var panelIndex = 0;
    var lastPanelWithVisibleContent = null;
    container.find(".mf-panel:visible").each(function () {

        // Use existing header height.
        var headerHeight = $(this).find(".mf-listing-header").outerHeight();

        // Calculate the panel height (header + content).
        var panelHeight = 0;
        if ($(this).find(".mf-listing-content:visible").length >= 1) {

            // The panel content is visible.

            // Treat the last visible panel in a different way.
            if (panelIndex == visiblePanels - 1) {

                // Last visible panel takes up all remaining space.
                panelHeight = remainingHeight;
            } else {

                // Calculate the appropriate panel height.
                var heightNumber = $(this).data("heightNumber");
                panelHeight = headerHeight + parseInt((heightNumber / visibleHeightNumbers) * totalHeightForPanelContent);
            }

            // Remember the last panel with visible content.
            lastPanelWithVisibleContent = $(this);
        } else {

            // Only the header is visible.
            panelHeight = headerHeight;
        }

        // Never exceed remaining height.
        if (panelHeight > remainingHeight)
            panelHeight = remainingHeight;

        // Resize the panel.
        $(this).height(panelHeight);
        remainingHeight -= panelHeight;

        // Resize content.
        setListingContentHeight($(this));

        // Update panel index.
        panelIndex++;

    });

    // If there is still some remaining height left, make the last panel with visible content taller.
    if (remainingHeight > 0 && lastPanelWithVisibleContent != null) {

        // Make the last panel with visible content taller.
        var newHeight = lastPanelWithVisibleContent.height() + remainingHeight;
        lastPanelWithVisibleContent.height(newHeight);
        setListingContentHeight(lastPanelWithVisibleContent);
    }
}



function resetTempSearchView(vault) {
    // var folderDefs = shellFrame.CurrentFolder;
    // var currPath = shellFrame.CurrentPath;
    // var tempViewRoot = vault.ViewOperations.GetBuiltInView(MFBuiltInViewLatestSearches);
    // var searchName = tempViewRoot.Name;
    // var index = currPath.indexOf(searchName);    
    // if (index === -1 || currPath.length === searchName.length) return;

    vault.ViewOperations.ResetFolderUIStateForSpecialLocation(true, false, MFFolderUIStateLocationTypeSearchResultsContainer);

    // var uiState = vault.ViewOperations.GetFolderUIStateForFolder(false, folderDefs, false);
    // uiState.ListingUIState.ViewMode = MFFolderListingViewModeDetails;
    // var nameColumn;
    // var scoreColumn;
    // var createdDateCol;
    // var createdByCol;
    // var allCount =  uiState.ListingUIState.Columns.Count;
    // for (var i = allCount; i >= 1; i--) {
    //     uiState.ListingUIState.Columns.Item(i).Width =90;
    //     var id = uiState.ListingUIState.Columns.Item(i).ID;
    //     if (id === MFFolderColumnIdName) {
    //         uiState.ListingUIState.Columns.Item(i).Visible = true;
    //         nameColumn = uiState.ListingUIState.Columns.Item(i);
    //     } else if (id === MFFolderColumnIdScore){
    //         uiState.ListingUIState.Columns.Item(i).Visible = true;
    //         scoreColumn = uiState.ListingUIState.Columns.Item(i);                    
    //     } else if (id === MFFolderColumnIdDateCreated) {
    //         uiState.ListingUIState.Columns.Item(i).Visible = true;
    //         createdDateCol = uiState.ListingUIState.Columns.Item(i);   
    //     } else if (id === MFBuiltInPropertyDefCreatedBy) {
    //         uiState.ListingUIState.Columns.Item(i).Visible = true;
    //         createdByCol = uiState.ListingUIState.Columns.Item(i);   
    //     }
    //     uiState.ListingUIState.Columns.Remove(i);                
    // }
    // var index = 0;
    // if (nameColumn) {
    //     nameColumn.Position = index++;
    //     uiState.ListingUIState.Columns.Add(-1, nameColumn);
    // }
    // if (scoreColumn) {
    //     scoreColumn.Position = index++;
    //     uiState.ListingUIState.Columns.Add(-1, scoreColumn);
    // }
    // if (createdByCol) {
    //     createdByCol.Position = index++;
    //     uiState.ListingUIState.Columns.Add(-1, createdByCol);
    // }
    // if (createdDateCol) {
    //     createdDateCol.Position = index++;
    //     uiState.ListingUIState.Columns.Add(-1, createdDateCol);
    // }
    // vault.ViewOperations.SaveFolderUIStateForFolder(false, false, folderDefs, false, uiState);
}