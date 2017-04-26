/*
 信息聚合
*/
var CC = CC || {};
(function (u, undefined) {
    var aggregation = {
        setModule: function (shellFrame, loc ,element, pathArray, imgArray, titleArray) {
            for (var i = 0; i < pathArray.length; i++) {
                this.createDashboard(shellFrame, loc, element, pathArray[i], imgArray[i], titleArray[i],i);
            }
        },

        /*创建临时视图*/
        createDashboard: function (shellFrame, loc, element, path, img, title,i) {
            try {
                
                var module1 = '<div id="module" class="module">' +
                               '<div id="headTitle">' +
                               '<img id="moduleImg" src="'+ img +'" />' +
                               '<div id="moduleTitle">'+ title +'</div>' +
                               '</div>' +
                              '<div id="listings'+ i +'" class="mf-panel-container" style="min-width: 350px; height: auto;">' +
                                  '<div id="listing_model'+ i +'" class="mf-panel" style="width: 100%; height: 50%; padding: 0px;margin: 0px; border: 0px; overflow: hidden;">' +
                                  '<div class="mf-listing-content mf-content">' +
                                  '</div>' +
                                  '</div>' +
                                  '</div>' +
                             '</div>';
            
                $('#' + element).append(module1);
                var listing = createListingForPath(shellFrame, loc, $('#listing_model'+i), path,
                    function (container, objectsCount) {
                        var heightNumber = 20;
                        if (objectsCount <= 2)
                            heightNumber = 15;
                        if (objectsCount <= 1)
                            heightNumber = 10;
                        if (objectsCount > 20)
                            heightNumber = 30;
                        container.data("heightNumber", heightNumber);
                    });

                if (listing) {
                  CC.SetCommTheme(listing);
                //  alert("mfflow in home.js");
                  listing.Events.Register(Event_SelectionChanged, CC.mfflow.selectionChangedHandler(shellFrame));
                }
                 
            } catch (e) { }
        },
    };
    u.aggregation = aggregation;
})(CC);