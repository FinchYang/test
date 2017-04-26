var CC = CC || {};
(function (u, undefined) {
    var category = { 
        /*function:创建零时视图
         * parentId:父容器Id
         * createPath：创建路径 例：系统邮件//收件箱//...
         * viewId：搜索出的视图Id*/
        create_dashboard: function (shellFrame, loc, parentId , createPath, vault, viewId) {
                try {
                  var listingElem = '<div id="listings" class="mf-panel-container" style="min-width: 790px; height: auto;">' +
                              '<div id="listing_temp" class="mf-panel" style="width: 100%; height: 50%; padding: 0px;margin: 0px; border: 0px; overflow: hidden;">' +
                              '<div class="mf-listing-content" style="height: 900px">' +
                              '</div>' +
                              '</div>' +
                              '</div>';
                $('#listings').remove();
                $('#' + parentId).append(listingElem);
               //根据路径创建对应Listing dom.text()
                  var listing = createListingForPath(shellFrame, loc, $('#listing_temp'), createPath, 
                    function (container, objectsCount) {
                        // Specify the relative height of this listing.
                        var heightNumber = 20;
                        if (objectsCount <= 2)
                            heightNumber = 15;
                        if (objectsCount <= 1)
                            heightNumber = 10;
                        if (objectsCount > 20)
                            heightNumber = 30; 
                        container.data("heightNumber", heightNumber); 
                    });
             
                     // Listen to the ActiveListingChanged event.
                    shellFrame.Events.Register(MFiles.Event.ActiveListingChanged, getActiveListingChangedHandler(shellFrame));

                    listing.Events.Register(MFiles.Event.SelectionChanged, function(selItems) {
                      if (selItems.ObjectVersions.Count !== 0) shellFrame.RightPane.Visible = true;
                      else shellFrame.RightPane.Visible = false;
                    });
            
                    // Resize panes. 
					$(window).resize(resizedWithHeight($("#search-head").height()));
					resizedWithHeight($("#search-head").height())();  
                } catch (error) {} 
                return listing;
        }
    };
    u.category = category;
})(CC);