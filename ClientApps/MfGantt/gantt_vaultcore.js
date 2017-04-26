
function OnNewVaultCore(vaultCore) {
	/// <summary>The entry point of VaultCore module.</summary>
	/// <param name="vaultCore" type="MFiles.VaultCore">The new vault core object.</param> 

	if (typeof JSON !== 'object') {
		JSON = getJSON();
	}

	// Register to listen new vault entry creation event. 
	vaultCore.Events.OnNewVaultEntry = newVaultEntryHandler;
}

function newVaultEntryHandler(vaultEntry) {
	/// <summary>Event handler to handle new vault entry object creations.</summary>
	/// <param name="vaultEntry" type="MFiles.VaultEntry">The new vault entry object.</param> 

	new VaultChangeTracker(vaultEntry);
}

function VaultChangeTracker(vaultEntry) {
	/// <summary>VaultChangeTracker constructor</summary>
	/// <param name="vaultEntry" type="MFiles.VaultEntry">The new vault entry object to listen to for changes</param> 

	var that = this,
        clientID = 0,
        changes = {};

	// bind shellui delegation via notification event
	vaultEntry.Events.Register(Event_Notification, function (target, cmd, arg) {

		// check what the target of the notification is
		if (target == "VaultChangeTracker") {

			// the notification is for us, try to pass the command on to one of our methods
			try {

				return that[cmd](arg);

			} catch (e) {

				return '{"error":"' + e.message + '"}';
			}
		}

	});

	// BIND TO UPDATE EVENTS

	// properties set
	vaultEntry.Events.Register(Event_PropertiesOfObjectVersionSet, function (objectVersion) {
		that.setUpdated(objectVersion.ObjVer);
	});

	// object checked in
	vaultEntry.Events.Register(Event_ObjectCheckedIn, function (objectVersion) {
		that.setUpdated(objectVersion.ObjVer);
	});

	// objects checked in
	vaultEntry.Events.Register(Event_ObjectsCheckedIn, function (objectVersions) {
		for (objectVersion in objectVersions) {
			that.setUpdated(objectVersion.ObjVer);
		}
	});

	// object checked out
	vaultEntry.Events.Register(Event_ObjectCheckedOut, function (objectVersion) {
		that.setUpdated(objectVersion.ObjVer);
	});

	// objects checked out
	vaultEntry.Events.Register(Event_ObjectsCheckedOut, function (objectVersions) {
		for (objectVersion in objectVersions) {
			that.setUpdated(objectVersion.ObjVer);
		}
	});

	// object checkout undone
	vaultEntry.Events.Register(Event_ObjectCheckoutUndone, function (objectVersion) {
		that.setUpdated(objectVersion.ObjVer);
	});

	// objects checkout undone
	vaultEntry.Events.Register(Event_ObjectCheckoutsUndone, function (objectVersions) {
		for (objectVersion in objectVersions) {
			that.setUpdated(objectVersion.ObjVer);
		}
	});


	// BIND TO CREATE EVENTS

	// object created
	vaultEntry.Events.Register(Event_ObjectCreated, function (objectVersion) {
		that.setCreated(objectVersion.ObjVer);
	});

	// objects created
	vaultEntry.Events.Register(Event_ObjectUndeleted, function (objectVersion) {
		that.setCreated(objectVersion.ObjVer);
	});

	// objects undeleted
	vaultEntry.Events.Register(Event_ObjectsUndeleted, function (objectVersions) {
		for (objectVersion in objectVersions) {
			that.setCreated(objectVersion.ObjVer);
		}
	});


	// BIND TO REMOVE/DELETE EVENTS

	// object removed
	vaultEntry.Events.Register(Event_ObjectRemoved, function (objectVersion) {
		that.setDeleted(objectVersion.ObjVer);
	});

	// objects removed
	vaultEntry.Events.Register(Event_ObjectsRemoved, function (objectVersions) {
		for (objectVersion in objectVersions) {
			that.setDeleted(objectVersion.ObjVer);
		}
	});

	// object destroyed
	vaultEntry.Events.Register(Event_ObjectDestroyed, function (objID) {
		that.setDeleted(objID);
	});

	// objects destroyed
	vaultEntry.Events.Register(Event_ObjectsDestroyed, function (ObjIDs) {
		for (objID in ObjIDs) {
			that.setDeleted(objID);
		}
	});



	this.attach = function () {
		/// <summary> attaches a client listener </summary>
		/// <returns type="number"> id of the new client </returns>

		// generate new client id
		var id = ++clientID;

		// create client event storage structure
		this.reset(id);

		// return the client's id
		return id;
	};

	this.detach = function (id) {
		/// <summary> detaches the client , i.e. no events will be stored for them anymore </summary>

		// removes client storage structure
		delete changes[id];

	};

	this.reset = function (id) {
		/// <summary> creates client event storage structure </summary>

		changes[id] = {
			created: [],
			updated: [],
			deleted: []
		};

	};

	this.getChanges = function (id) {
		/// <summary> Gets the changes to the vault for the client since last it queried </summary>
		/// <param name="id" type="number"> The id of the client </param>

		// declare our json string response
		var str;

		// check if we tracking for this client
		if (changes[id]) {

			// client info found, stringify it into json
			str = JSON.stringify(changes[id]);

		} else {

			// no client info found, throw error
			throw new Error("无法识别客户端."); //"Client id not recognized."
		}

		// reset our event storage structure for this client (release old data)
		this.reset(id);

	
		return str;
	};

	this.setUpdated = function (obj) {
		try {
			for (id in changes) {
				changes[id].updated.push(obj.Type + "-" + obj.ID);
			}
		} catch (e) {

		}
	}

	this.setCreated = function (obj) {
		try {
			for (id in changes) {
				changes[id].created.push(obj.Type + "-" + obj.ID);
			}
		} catch (e) {

		}
	}

	this.setDeleted = function (obj) {
		try {
			for (id in changes) {
				changes[id].deleted.push(obj.Type + "-" + obj.ID);
			}
		} catch (e) {

		}
	}


}