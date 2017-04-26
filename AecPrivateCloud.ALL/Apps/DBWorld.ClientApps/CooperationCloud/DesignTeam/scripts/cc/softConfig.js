"use strict";

var softConfigOp = {
	cloudApps: [
        { "appID": "Citrix.MPS.App.Farm1.Microsoft Word 2010", "appName": "Word2010", "extensions": ["doc", "docx"] },
        { "appID": "Citrix.MPS.App.Farm1.Microsoft Excel 201", "appName": "Excel2010", "extensions": ["xls", "xlsx"] },
        { "appID": "Citrix.MPS.App.Farm1.Microsoft PowerPoin", "appName": "MicrosoftPowerPoint2010", "extensions": ["ppt", "pptx"] },

        { "appID": "Citrix.MPS.App.Farm1.Revit201470", "appName": "Revit201470", "extensions": ["rvt", "rfa", "rft", "rte"] },

        { "appID": "Citrix.MPS.App.Farm1.AutoCAD 2010", "appName": "AutoCAD2010", "extensions": ["dwg", "dwf", "dwfx", "dws"] },
			
		{ "appID": "Citrix.MPS.App.Farm1.Navisworks2014", "appName": "Navisworks2014", "extensions": ["nwc", "nwf", "nwd"] },

		{ "appID": "Citrix.MPS.App.Farm1.3dsmax2014", "appName": "3dsmax2014", "extensions": ["max", "fbx"] },

        { "appID": "Citrix.MPS.App.Farm1.Microsoft Project 2", "appName": "Project2010", "extensions": ["mpp", "mpt", "mpd"] },
			
		{ "appID": "Citrix.MPS.App.Farm1.Rhinoceros 50 64-bi", "appName": "Rhinoceros5", "extensions": ["3dm"] }
		],
		
	getAppsByExtension: function (ext) {
	    var res = [];
		var apps = this.cloudApps;
		for (var i = 0; i < apps.length; i++) {
		    var app = apps[i];
		    for (var j = 0; j < app.extensions.length; j++) {
		        var extension = app.extensions[j];
		        if (extension === ext) {
		            res.push({ "ApplicationID": app.appID, "AppName": app.appName });
		        }
		    }
		}
		return res;
	}
}
